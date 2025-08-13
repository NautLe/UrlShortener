using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public UrlController(ApplicationDbContext context, ShortCodeGenerator codeGenerator, IMemoryCache cache)
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _cache = cache;
        }

        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] ShortenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string cacheKey = $"shorten_{request.OriginalUrl}";

            // ✅ Thử lấy shortCode từ cache
            if (_cache.TryGetValue(cacheKey, out string cachedShortCode))
            {
                var cachedResult = $"{Request.Scheme}://{Request.Host}/{cachedShortCode}";
                return Ok(cachedResult);
            }

            var shortCode = _codeGenerator.Generate(request.OriginalUrl);

            // Check trùng trong DB
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

            // ✅ Lưu vào cache trong 30 phút
            _cache.Set(cacheKey, shortCode, TimeSpan.FromMinutes(30));

            var result = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(result);
        }
    }
}
