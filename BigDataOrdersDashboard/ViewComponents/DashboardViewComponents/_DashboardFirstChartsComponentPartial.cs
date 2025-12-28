using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardFirstChartsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
