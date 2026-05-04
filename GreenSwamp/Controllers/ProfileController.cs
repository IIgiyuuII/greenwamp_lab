using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwamp.Data;
using GreenSwamp.Models;

namespace GreenSwamp.Controllers
{
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Index(string username)
        {
            var user = await _context.Users
                .Include(u => u.Posts.Where(p => p.ParentPostId == null))
                    .ThenInclude(p => p.Interactions)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            // Сортируем посты по дате
            user.Posts = user.Posts.OrderByDescending(p => p.CreatedAt).ToList();

            // Подсчет взаимодействий
            var postInteractions = new Dictionary<int, Dictionary<string, int>>();
            foreach (var post in user.Posts)
            {
                var counts = new Dictionary<string, int>
                {
                    ["likes"] = post.Interactions.Count(i => i.InteractionType == "like"),
                    ["reribbs"] = post.Interactions.Count(i => i.InteractionType == "reribb"),
                    ["comments"] = post.Interactions.Count(i => i.InteractionType == "comment")
                };
                postInteractions[post.PostId] = counts;
            }

            ViewBag.PostInteractions = postInteractions;

            return View(user);
        }
    }
}