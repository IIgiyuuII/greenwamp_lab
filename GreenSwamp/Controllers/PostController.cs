using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwamp.Data;
using GreenSwamp.Models;

namespace GreenSwamp.Controllers
{
    [Route("feed/post")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{postId:int}")]
        public async Task<IActionResult> Index(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                    .ThenInclude(i => i.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Event)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                return NotFound();
            }

            var counts = new Dictionary<string, int>
            {
                ["likes"] = post.Interactions.Count(i => i.InteractionType == "like"),
                ["reribbs"] = post.Interactions.Count(i => i.InteractionType == "reribb"),
                ["comments"] = post.Interactions.Count(i => i.InteractionType == "comment"),
                ["rsvps"] = post.Interactions.Count(i => i.InteractionType == "rsvp")
            };

            ViewBag.InteractionCounts = counts;

            return View(post);
        }
    }
}