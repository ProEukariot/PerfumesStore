using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PerfumesStore.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine(HttpContext.Session.Id);
			Console.WriteLine(User.Identity.Name);

			base.OnActionExecuting(context);
        }
    }
}
