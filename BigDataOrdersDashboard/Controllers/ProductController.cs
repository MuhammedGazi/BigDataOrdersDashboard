using BigDataOrdersDashboard.Context;
using BigDataOrdersDashboard.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BigDataOrdersDashboard.Controllers
{
    public class ProductController : Controller
    {
        private readonly BigDataOrdersDbContext _context;

        public ProductController(BigDataOrdersDbContext context)
        {
            _context = context;
        }

        private void GetCategory()
        {
            var categories = _context.Categories.ToList();
            ViewBag.categories = (from category in categories
                                  select new SelectListItem
                                  {
                                      Text = category.CategoryName,
                                      Value = category.CategoryId.ToString()
                                  }).ToList();
        }
        public IActionResult ProductList(int page = 1)
        {
            //var values = _context.Products.ToList();
            //return View(values);

            int pageSize = 12; // her sayfada 12 kayıt
            var values = _context.Products
                                 .OrderBy(p => p.ProductId)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .Include(y => y.Category)
                                 .ToList();

            int totalCount = _context.Products.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(values);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            GetCategory();
            return View();
        }
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(ProductList));
        }

        [HttpGet]
        public IActionResult UpdateProduct(int id)
        {
            GetCategory();
            var values = _context.Products.Find(id);
            return View(values);
        }
        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(ProductList));
        }

        public IActionResult DeleteProduct(int id)
        {
            var values = _context.Products.Find(id);
            _context.Products.Remove(values);
            _context.SaveChanges();
            return RedirectToAction(nameof(ProductList));
        }
    }
}
