using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;
using System.Threading.Tasks;
using System.Linq;

namespace UrlShortener.Controllers
{
    public class LinksListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinksListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LinksList
        public async Task<IActionResult> Index()
        {
            var links = await _context.ShortUrls
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
            return View(links);
        }

        // GET: LinksList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var link = await _context.ShortUrls
                .FirstOrDefaultAsync(m => m.Id == id);

            if (link == null)
                return NotFound();

            _context.ShortUrls.Remove(link);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: LinksList/UpdateShortCode
        [HttpPost]
        public async Task<IActionResult> UpdateShortCode(int id, string fullUrl)
        {
            if (string.IsNullOrWhiteSpace(fullUrl))
                return BadRequest("Short URL cannot be empty.");

            // Lấy shortCode từ full URL
            var uri = new Uri(fullUrl);
            var shortCode = uri.AbsolutePath.Trim('/');

            var link = await _context.ShortUrls.FindAsync(id);
            if (link == null)
                return NotFound();

            // Kiểm tra trùng ShortCode
            bool exists = await _context.ShortUrls
                .AnyAsync(l => l.ShortCode == shortCode && l.Id != id);
            if (exists)
                return Conflict("Short code already exists.");

            link.ShortCode = shortCode;
            _context.Update(link);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
