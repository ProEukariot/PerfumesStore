using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PerfumesStore.Models;

namespace PerfumesStore.TagHelpers
{
	public class PaginationTagHelper : TagHelper
	{
		private IUrlHelperFactory _urlFactory { get; set; }

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext Context { get; set; } = null!;

		public string Action { get; set; } = "";

		public PaginationViewModel Paging { get; set; }

		public int ButtonsAside { get; set; } = 1;

#pragma warning disable
		public PaginationTagHelper(IUrlHelperFactory urlFactory)
		{
			_urlFactory = urlFactory;

		}
#pragma warning restore

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			IUrlHelper urlHelper = _urlFactory.GetUrlHelper(Context);

			TagBuilder nav = new TagBuilder("nav");

			TagBuilder ul = new TagBuilder("ul");
			ul.AddCssClass("pagination");

			for (int i = Paging.CurrentPage - ButtonsAside; i < Paging.CurrentPage + 1 + ButtonsAside; i++)
			{
				if (Paging.HasPage(i))
				{
					string url = urlHelper.Action(Action, new { 
						page = i,
						sort = Context.HttpContext.Request.Query["sort"],
						name = Context.HttpContext.Request.Query["name"],
						price = Context.HttpContext.Request.Query["price"],
						brand = Context.HttpContext.Request.Query["brand"],
						state = Context.HttpContext.Request.Query["state"],
					})!;

					TagBuilder li = new TagBuilder("li");
					li.AddCssClass("page-item");
					if (i == Paging.CurrentPage)
						li.AddCssClass("active");

					TagBuilder a = new TagBuilder("a");
					a.AddCssClass("page-link");
					a.Attributes.Add("href", url ?? "");
					a.InnerHtml.SetContent((i).ToString());

					li.InnerHtml.AppendHtml(a);
					ul.InnerHtml.AppendHtml(li);
				}
			}

			nav.InnerHtml.AppendHtml(ul);

			output.TagName = nav.TagName;
			output.MergeAttributes(nav);
			output.Content.AppendHtml(nav.InnerHtml);
		}
	}
}
