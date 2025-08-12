using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using System.Linq;

namespace UrlShortener.Controllers
{
    public class RedirectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedirectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Go(string shortCode)
        {
            var link = _context.ShortUrls.FirstOrDefault(x => x.ShortCode == shortCode);
            if (link == null)
                return NotFound("Short link not found.");

            if (string.IsNullOrWhiteSpace(link.OriginalUrl))
                return BadRequest("The original URL is missing.");

            link.ClickCount++;
            _context.SaveChanges();

            return Redirect(link.OriginalUrl);
        }
    }
}
