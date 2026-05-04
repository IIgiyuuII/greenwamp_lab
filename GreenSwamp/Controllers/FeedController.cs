using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwamp.Data;
using GreenSwamp.Models;

namespace GreenSwamp.Controllers
{
    [Route("feed")]
    public class FeedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Event)
                .Where(p => p.ParentPostId == null) 
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            
            var postInteractions = new Dictionary<int, Dictionary<string, int>>();
            foreach (var post in posts)
            {
                var counts = new Dictionary<string, int>
                {
                    ["likes"] = post.Interactions.Count(i => i.InteractionType == "like"),
                    ["reribbs"] = post.Interactions.Count(i => i.InteractionType == "reribb"),
                    ["comments"] = post.Interactions.Count(i => i.InteractionType == "comment"),
                    ["rsvps"] = post.Interactions.Count(i => i.InteractionType == "rsvp")
                };
                postInteractions[post.PostId] = counts;
            }

            
            var trendingPonds = await _context.TrendingPonds
                .OrderByDescending(t => t.RecentPosts)
                .Take(5)
                .ToListAsync();

            ViewBag.PostInteractions = postInteractions;
            ViewBag.TrendingPonds = trendingPonds;

            return View(posts);
        }
    }
}