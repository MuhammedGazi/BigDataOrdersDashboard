using Microsoft.AspNetCore.Mvc;

namespace BigDataOrdersDashboard.ViewComponents.NavbarViewComponets
{
    public class _NavbarQuickMenuComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
