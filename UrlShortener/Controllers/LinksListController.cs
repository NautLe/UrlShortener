using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using System.Threading.Tasks;

namespace UrlShortener.Controllers
{
    public class LinksListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinksListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Links
        public async Task<IActionResult> Index()
        {
            var links = await _context.ShortUrls
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(links);
        }

        // GET: /Links/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var link = await _context.ShortUrls.FindAsync(id);
            if (link != null)
            {
                _context.ShortUrls.Remove(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
