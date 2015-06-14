using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoveLowestHWgrade
{
    class Program
    {
        const string SCORE_TYPE_TO_REMOVE = "homework";
        const string CONNECTION_STRING = "mongodb://localhost:27017";
        const string DB_NAME = "school";
        private const string COLLECTION_NAME = "students";

        static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            var client = new MongoClient(CONNECTION_STRING);

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Student>(COLLECTION_NAME);

            var students = await collection.Find(new BsonDocument()).ToListAsync();

            foreach (var student in students)
            {
                var lowestHomeworkScore = new List<Grade>();
                lowestHomeworkScore = FindLowestHomeworkScore(student.Scores);

                await collection.UpdateOneAsync(s => s.Id == student.Id, Builders<Student>.Update.Set(s => s.Scores, lowestHomeworkScore));
            }
        }

        private static List<Grade> FindLowestHomeworkScore(IList<Grade> scores)
        {
            var result = double.MaxValue;

            foreach (var score in scores)
            {
                if (score.Type == SCORE_TYPE_TO_REMOVE && result > score.Score)
                {
                    result = score.Score;
                }
            }

            var resultList = scores.Where(x => x.Score != result).ToList();
            return resultList;

        }

        internal class Student
        {
            public double Id { get; set; }

            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("scores")]
            public IList<Grade> Scores { get; set; }
        }

        internal class Grade
        {
            [BsonElement("type")]
            public string Type { get; set; }

            [BsonElement("score")]
            public double Score { get; set; }
        }
    }
}
