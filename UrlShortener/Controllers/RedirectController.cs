using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using System;
using System.Linq;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/url/shorten
        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
                return BadRequest("Invalid URL format.");

            var shortCode = Guid.NewGuid().ToString().Substring(0, 6);

            // Check for collisions (rare, but safer)
            while (_context.ShortUrls.Any(u => u.ShortCode == shortCode))
            {
                shortCode = Guid.NewGuid().ToString().Substring(0, 6);
            }

            var shortUrl = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode
            };

            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            var result = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(result);
        }

        [HttpGet("{shortCode}")]
        public IActionResult GetOriginalUrl(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return BadRequest("Short code is required.");

            var link = _context.ShortUrls.FirstOrDefault(u => u.ShortCode == shortCode);
            if (link == null)
                return NotFound("Short link not found.");

            return Ok(new
            {
                OriginalUrl = link.OriginalUrl,
                ShortCode = link.ShortCode,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{link.ShortCode}",
                ClickCount = link.ClickCount
            });
        }
    }
}
