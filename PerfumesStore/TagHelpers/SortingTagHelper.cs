using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PerfumesStore.Models;

namespace PerfumesStore.TagHelpers
{
	public class SortingTagHelper : TagHelper
	{
		private IUrlHelperFactory _urlFactory { get; set; }

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext Context { get; set; } = null!;

		public string Action { get; set; } = "";

		public SortingViewModel CurrentModel { get; set; }

		public string SortTarget { get; set; }

		public SortingTagHelper(IUrlHelperFactory urlFactory)
		{
			_urlFactory = urlFactory;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			string target = SortTarget;
			SortState state;

			switch (target)
			{
				case "Name":
					state = CurrentModel.NameSort;
					//output.Content.SetContent("Name");
					break;
				case "Brand":
					state = CurrentModel.BrandSort;
					//output.Content.SetContent("Brand");
					break;
				case "Price":
					state = CurrentModel.PriceSort;
					//output.Content.SetContent("Price");
					break;
				default:
					state = CurrentModel.Current;
					output.Content.SetContent("default");
					break;
			}

			output.TagName = "a";
			

			IUrlHelper urlHelper = _urlFactory.GetUrlHelper(Context);

			string url = urlHelper.Action(Action, new { 
				sort = state,
				name = Context.HttpContext.Request.Query["name"], 
				price = Context.HttpContext.Request.Query["price"], 
				brand = Context.HttpContext.Request.Query["brand"], 
				state = Context.HttpContext.Request.Query["state"],
				page = Context.HttpContext.Request.Query["page"],
			})!;

			output.Attributes.Add("class", "link");
			output.Attributes.SetAttribute("href", url ?? "");
		}
	}
}
