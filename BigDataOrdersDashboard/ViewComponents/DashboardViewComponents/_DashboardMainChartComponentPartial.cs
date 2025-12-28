using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardMainChartComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
