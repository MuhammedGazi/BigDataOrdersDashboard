using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardLowStockProductsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
