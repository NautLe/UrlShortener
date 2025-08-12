using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Services;

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
        public IActionResult Shorten([FromBody] ShortenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var shortCode = _codeGenerator.Generate(request.OriginalUrl);

            // Check trùng
            while (_context.ShortUrls.Any(u => u.ShortCode == shortCode))
            {
                shortCode = _codeGenerator.Generate(request.OriginalUrl);
            }

            var shortUrl = new ShortUrl
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode
            };

            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            var result = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(result);
        }
    }
}
