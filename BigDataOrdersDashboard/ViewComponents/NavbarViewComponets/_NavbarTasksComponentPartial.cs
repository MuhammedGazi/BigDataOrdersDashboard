using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.NavbarViewComponets
{
    public class _NavbarTasksComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
