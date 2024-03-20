using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using PerfumesStore.Models;

namespace PerfumesStore
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddSession(opt => { });

			builder.Services.AddControllersWithViews();

			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
			{
				options.LoginPath = "/Login";
				options.AccessDeniedPath = "/Forbidden";
				options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

			});
			builder.Services.AddAuthorization();

			builder.Services.AddTransient<NpgsqlConnection>(c =>
			{
				string connection = builder.Configuration.GetConnectionString("Default")!;
				return new NpgsqlConnection(connection);
			});

			var app = builder.Build();

			app.UseHsts();

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseSession(new SessionOptions() { IdleTimeout = TimeSpan.FromMinutes(60) });

			app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id:int?}");

			app.Run();
		}
	}
}