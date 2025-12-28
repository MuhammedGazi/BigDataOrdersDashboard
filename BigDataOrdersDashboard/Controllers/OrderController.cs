using BigDataOrdersDashboard.Context;
using BigDataOrdersDashboard.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BigDataOrdersDashboard.Controllers
{
    public class OrderController : Controller
    {
        private readonly BigDataOrdersDbContext _context;

        public OrderController(BigDataOrdersDbContext context)
        {
            _context = context;
        }

        private void GetProductAndCustomer()
        {
            //şimdilik kullanmıyoruz çok fazla ürün ve müşteri var revize edilecek
            var products = _context.Products.ToList();
            ViewBag.product = (from product in products
                               select new SelectListItem
                               {
                                   Text = product.ProductName,
                                   Value = product.ProductId.ToString()
                               }).ToList();

            var customers = _context.Customers.ToList();
            ViewBag.customer = (from customer in customers
                                select new SelectListItem
                                {
                                    Text = customer.CustomerName,
                                    Value = customer.CustomerId.ToString()
                                }).ToString();
        }
        public IActionResult OrderList(int page = 1)
        {
            int pageSize = 12; // her sayfada 12 kayıt
            var values = _context.Orders
                                 .OrderBy(p => p.OrderId)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .Include(x => x.Product)
                                 .Include(y => y.Customer)
                                 .ToList();

            int totalCount = _context.Orders.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(values);
        }

        [HttpGet]
        public IActionResult CreateOrder()
        {
            GetProductAndCustomer();
            return View();
        }
        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            order.OrderDate = DateTime.Now;
            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrderList));
        }

        [HttpGet]
        public IActionResult UpdateOrder(int id)
        {
            GetProductAndCustomer();
            var value = _context.Orders.Find(id);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateOrder(Order order)
        {
            order.OrderDate = DateTime.Now;
            _context.Orders.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrderList));
        }

        public IActionResult DeleteOrder(int id)
        {
            var value = _context.Orders.Find(id);
            _context.Orders.Remove(value);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrderList));
        }
    }
}
