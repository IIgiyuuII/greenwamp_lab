using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwamp.Data;
using GreenSwamp.Models;

namespace GreenSwamp.Controllers
{
    [Route("ponds")]
    public class PondsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PondsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /ponds - страница со всеми тегами
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ѕолучаем все теги
            var allTags = await _context.Tags
                .OrderByDescending(t => t.UsageCount)
                .ToListAsync();

            
            var tagPostCounts = new Dictionary<int, int>();
            foreach (var tag in allTags)
            {
                var count = await _context.PostTags
                    .CountAsync(pt => pt.TagId == tag.TagId);
                tagPostCounts[tag.TagId] = count;
            }

            
            var trendingPonds = await _context.TrendingPonds
                .OrderByDescending(t => t.RecentPosts)
                .Take(10)
                .ToListAsync();

            ViewBag.AllTags = allTags;
            ViewBag.TagPostCounts = tagPostCounts;  
            ViewBag.TrendingPonds = trendingPonds;

            return View();
        }

        
        [HttpGet("{tagName}")]
        public async Task<IActionResult> TagPosts(string tagName)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.TagName == tagName);

            if (tag == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Event)
                .Where(p => p.PostTags.Any(pt => pt.Tag.TagName == tagName))
                .Where(p => p.ParentPostId == null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postInteractions = new Dictionary<int, Dictionary<string, int>>();
            foreach (var post in posts)
            {
                postInteractions[post.PostId] = new Dictionary<string, int>
                {
                    ["likes"] = post.Interactions.Count(i => i.InteractionType == "like"),
                    ["reribbs"] = post.Interactions.Count(i => i.InteractionType == "reribb"),
                    ["comments"] = post.Interactions.Count(i => i.InteractionType == "comment"),
                    ["rsvps"] = post.Interactions.Count(i => i.InteractionType == "rsvp"),
                };
            }

            var trendingPonds = await _context.TrendingPonds
                .OrderByDescending(t => t.RecentPosts)
                .Take(5)
                .ToListAsync();

            ViewBag.TagName = tagName;
            ViewBag.Tag = tag;
            ViewBag.PostInteractions = postInteractions;
            ViewBag.TrendingPonds = trendingPonds;

            return View(posts);
        }
    }
}