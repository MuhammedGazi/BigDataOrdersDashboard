using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardLast5ReviewsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
