using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveLowestHWGrades
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("students");
            var collection = db.GetCollection<BsonDocument>("grades");

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("type", "homework");
            //var filter = new BsonDocument("type", "homework");

            var sortBuilder = Builders<BsonDocument>.Sort;
            var sorter = sortBuilder.Descending("student_id").Descending("score");

            var resultList = await collection.Find(filter).Sort(sorter).ToListAsync();

            for (int i = 1; i < resultList.Count; i++)
            {
                if (resultList[i - 1]["student_id"] != resultList[i]["student_id"])
                {
                    var builderDelete = Builders<BsonDocument>.Filter;
                    var filterDelete = builderDelete.And(builderDelete.Eq("student_id", resultList[i - 1]["student_id"]), builderDelete.Eq("score", resultList[i - 1]["score"]));
                    collection.DeleteOneAsync(filterDelete);
                }

                if (i == resultList.Count - 1 && resultList[i]["type"] == "homework")
                {
                    var builderDelete = Builders<BsonDocument>.Filter;
                    var filterDelete = builderDelete.And(builderDelete.Eq("student_id", resultList[i]["student_id"]), builderDelete.Eq("score", resultList[i]["score"]));
                    collection.DeleteOneAsync(filterDelete);
                }
            }
        }
    }
}
