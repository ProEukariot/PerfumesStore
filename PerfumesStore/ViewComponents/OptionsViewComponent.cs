using Microsoft.AspNetCore.Mvc;

namespace PerfumesStore.ViewComponents
{
	public class OptionsViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
