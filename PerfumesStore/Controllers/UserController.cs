using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PerfumesStore.Models;
using System.Data;
using System.Security.Claims;

namespace PerfumesStore.Controllers
{
	[Authorize(Roles = "User")]
	public class UserController : Controller
	{
		private NpgsqlConnection db;

		public UserController(NpgsqlConnection db)
		{
			this.db = db;
		}

		public IActionResult Settings()
		{
			return View();
		}

		public async Task<IActionResult> OrderDetails(int id) 
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			List<HistoryOrderViewModel> records = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT * FROM item_records " +
							$"WHERE order_id = {id}; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								HistoryOrderViewModel obj = new()
								{
									ItemId = (int)reader["id"],
									ItemName = reader["name"].ToString(),
									Price = (decimal)reader["price"],
									OrderId = (int)reader["order_id"],
									Quantity = (int)reader["quantity"],
									Type = reader["type"].ToString(),
									TotalPrice = (decimal)reader["total"],
									Created_at = Convert.ToDateTime(reader["created_at"]),
									ClientId = (int)reader["client_id"]
								};

								records.Add(obj);
							}
						}

					}
				}
			}
			catch (Exception e)
			{
				return RedirectToAction("Error", "Home", new { id = 400, message = e.Message });
			}
			finally
			{
				await db.CloseAsync();
				await db.DisposeAsync();
			}

			return View(records);
		}

		public async Task<IActionResult> History()
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			List<GroupedItemViewModel> orders = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						string customer_id = HttpContext.User.FindFirstValue("Id")!;
						Console.WriteLine(customer_id);

						cmd.Connection = db;
						cmd.CommandText = $"SELECT * FROM customer_order WHERE client_id = {customer_id}; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								GroupedItemViewModel obj = new() {
									Id = (int)reader["id"],
									Date = Convert.ToDateTime(reader["created_at"]),
									TotalSum = (decimal)reader["total"],
									Status = reader["status"].ToString(),
								};

								orders.Add(obj);
							}
						}

					}
				}
			}
			catch (Exception e)
			{
				return RedirectToAction("Error", "Home", new { id = 400, message = e.Message });
			}
			finally
			{
				await db.CloseAsync();
				await db.DisposeAsync();
			}

			return View(orders);
		}
	}
}
