using Microsoft.AspNetCore.Mvc;
using PerfumesStore.Models;

namespace PerfumesStore.ViewComponents
{
	public class CartViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			{
				//string? cartItems = HttpContext.Session.GetString("Cart");

				//List<Perfume> perfumes = new() { };

				//if (string.IsNullOrEmpty(cartItems))
				//{
				//	return View(perfumes);
				//}

				//string[] items = cartItems.Split("|", StringSplitOptions.RemoveEmptyEntries);

				////-- db conn
				//foreach (var item in items)
				//{
				//	perfumes.Add(new Perfume() { Id = int.Parse(item), Name = "PerfumesName"});
				//}
			}

			CartViewModel vm = HttpContext.Session.GetJson<CartViewModel>("Cart");
			
			return View(vm);
		}
	}
}
