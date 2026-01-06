using BigDataOrdersDashboard.Context;
using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.CustomerAnalyticsViewComponents
{
    public class _CustomerAnalyticsMainStatisticsComponentPartial : ViewComponent
    {
        private readonly BigDataOrdersDbContext _context;

        public _CustomerAnalyticsMainStatisticsComponentPartial(BigDataOrdersDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {

            var totalCustomerCount = _context.Customers.Count();

            ViewBag.TotalCustomerCount = totalCustomerCount;

            var totalOrderCount = _context.Orders.Count();

            var averageOrderPerCustomerCount = totalOrderCount / totalCustomerCount;

            ViewBag.AverageOrderPerCustomerCount = averageOrderPerCustomerCount;

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var activeCustomerCount = _context.Orders.Where(o => o.OrderDate >= threeMonthsAgo).Select(o => o.CustomerId).Distinct().Count();

            ViewBag.ActiveCustomerCount = activeCustomerCount;

            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var inactiveCustomerCount = _context.Customers.Count(c => !_context.Orders.Any(o => o.CustomerId == c.CustomerId && o.OrderDate >= sixMonthsAgo));

            ViewBag.InactiveCustomerCount = inactiveCustomerCount;

            return View();
        }
    }
}       