using Concurrency.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Concurrency.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        public async Task<IActionResult> Update(int id)
        {
            return View(await _context.Products.FindAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Update(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException exception)
            {
                var exceptionEntry = exception.Entries.First();
                var currentProduct = exceptionEntry.Entity as Product;
                var databaseValues = exceptionEntry.GetDatabaseValues();
                var clientValues = exceptionEntry.CurrentValues;
                if (databaseValues == null)
                {
                    ModelState.AddModelError(string.Empty, "Bu ürün başka bir kullanıcı tarafından silindi");
                }
                else
                {
                    var databaseProduct = databaseValues.ToObject() as Product;
                    ModelState.AddModelError(string.Empty, "Bu ürün başka bir kullanıcı tarafından güncellendi");
                }


                return View(product);
            }
        }
    }
}
