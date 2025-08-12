using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Services;
using System;
using System.Linq;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ShortCodeGenerator _codeGenerator;

        public UrlController(ApplicationDbContext context, ShortCodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
                return BadRequest("Invalid URL format.");

            var existing = _context.ShortUrls.FirstOrDefault(u => u.OriginalUrl == originalUrl);
            if (existing != null)
                return Ok($"{Request.Scheme}://{Request.Host}/{existing.ShortCode}");

            var shortCode = _codeGenerator.Generate(originalUrl);

            int counter = 1;
            while (_context.ShortUrls.Any(u => u.ShortCode == shortCode))
            {
                shortCode = shortCode + counter;
                if (shortCode.Length > 10)
                    shortCode = shortCode.Substring(0, 10);
                counter++;
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
