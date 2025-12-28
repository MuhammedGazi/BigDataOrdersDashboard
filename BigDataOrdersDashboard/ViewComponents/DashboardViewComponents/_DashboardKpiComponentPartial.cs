using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardKpiComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
