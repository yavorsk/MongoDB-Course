using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest
            var recentPosts = await blogContext.Posts.Find(new BsonDocument())
                                                .Sort(Builders<Post>.Sort.Descending(p => p.CreatedAtUtc))
                                                .Limit(10)
                                                .ToListAsync();

            // Mongo's solution:
            //var recentPosts = await blogContext.Posts.Find(x => true)
            //                            .SortByDescending(x => x.CreatedAtUtc)
            //                            .Limit(10)
            //                            .ToListAsync();

            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // Insert the post into the posts collection
            var post = new Post()
            {
                Title = model.Title,
                Content = model.Content,
                Tags = model.Tags.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries),
                Author = this.User.Identity.Name,
                CreatedAtUtc = DateTime.UtcNow,
                Comments = new List<Comment>()
            };
            await blogContext.Posts.InsertOneAsync(post);

            return RedirectToAction("Post", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier
            var post = await blogContext.Posts.Find(Builders<Post>.Filter.Eq(p => p.Id, id)).FirstOrDefaultAsync();

            // Mongo's solution
            //var post = await blogContext.Posts.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.
            var filter = tag == null
                        ? new BsonDocument()
                        : Builders<Post>.Filter.All(p => p.Tags, new String[] { tag });

            var posts = await blogContext.Posts.Find(filter).ToListAsync();

            //Mongo's solution:
            //Expression<Func<Post, bool>> filter = x => true;

            //if (tag != null)
            //{
            //    filter = x => x.Tags.Contains(tag);
            //}

            //var posts = await blogContext.Posts.Find(filter)
            //    .SortByDescending(x => x.CreatedAtUtc)
            //    .Limit(10)
            //    .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"
            var comment = new Comment()
            {
                Author = this.User.Identity.Name,
                Content = model.Content,
                CreatedAtUtc = DateTime.Now.ToUniversalTime()
            };

            await blogContext.Posts.UpdateOneAsync(p => p.Id == model.PostId,
                                                    Builders<Post>.Update.AddToSet(x => x.Comments, comment));

            // Mongo's solution:
            //await blogContext.Posts.UpdateOneAsync(
            //    x => x.Id == model.PostId,
            //    Builders<Post>.Update.Push(x => x.Comments, comment));

            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}