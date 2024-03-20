using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Npgsql;
using PerfumesStore.Models;
using System.Data;
using System.Security.Claims;

namespace PerfumesStore.Controllers
{
	[Authorize(Roles = "Staff")]
	public class StaffController : Controller
	{
		private NpgsqlConnection db;

		public StaffController(NpgsqlConnection db)
		{
			this.db = db;
		}

		public IActionResult Settings()
		{
			return View();
		}

		public async Task<IActionResult> TakeOrder()
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			List<OrderStaffViewModel> orders = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "SELECT id, total, status, created_at, client_id FROM orders " +
							"WHERE status = 'created' OR status = 'packed' " +
							"ORDER BY created_at ASC; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								OrderStaffViewModel ord = new()
								{
									Id = (int)reader["id"],
									Status = reader["status"].ToString(),
									Total = (decimal)reader["total"],
									Created_at = Convert.ToDateTime(reader["created_at"]),
									Client_id = (int)reader["client_id"],
								};
								Console.WriteLine(ord.Id);
								orders.Add(ord);
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

		[HttpPost]
		public async Task<IActionResult> TakePacking(int id)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"UPDATE orders SET " +
							$"finished_by = {User.FindFirstValue("id")}, " +
							$"status = 'packing' " +
							$"WHERE id = {id}; ";

						Console.WriteLine(cmd.CommandText);

						await cmd.ExecuteNonQueryAsync();


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

			return RedirectToAction("Packing", new { linkActive = "staff-packing" });
		}

		public async Task<IActionResult> Packing()
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			List<OrderStaffViewModel> orders = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT id, total, status, created_at, client_id FROM orders " +
							$"WHERE status = 'packing' AND finished_by = {User.FindFirstValue("id")}";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								OrderStaffViewModel ord = new()
								{
									Id = (int)reader["id"],
									Status = reader["status"].ToString(),
									Total = (decimal)reader["total"],
									Created_at = Convert.ToDateTime(reader["created_at"]),
									Client_id = (int)reader["client_id"],
								};
								orders.Add(ord);
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

		[HttpPost]
		public async Task<IActionResult> FinishPacking(int id)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"UPDATE orders SET " +
							$"status = 'packed', finished_at = NOW() " +
							$"WHERE id = {id}; ";

						Console.WriteLine(cmd.CommandText);

						await cmd.ExecuteNonQueryAsync();


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

			return RedirectToAction("TakeOrder", new { linkActive = "staff-takeorder" });
		}

		[HttpPost]
		public async Task<IActionResult> TakeDelivery(int id)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"UPDATE orders SET " +
							$"status = 'delivering' " +
							$"WHERE id = {id}; " +
							$"INSERT INTO shipping_details(id, status, courier) VALUES " +
							$"({id}, 'delivering', {User.FindFirstValue("id")}); ";

						Console.WriteLine(cmd.CommandText);

						await cmd.ExecuteNonQueryAsync();

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

			return RedirectToAction("Delivery", new { linkActive = "staff-delivery" });
		}

		public async Task<IActionResult> Delivery()
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			List<DeliveryViewModel> deliveries = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT sp.id, sp.status, total, email, address, courier FROM shipping_details sp " +
							$"INNER JOIN orders o ON o.id = sp.id " +
							$"INNER JOIN staff_client_shipping_details c ON c.id = o.client_id " +
							$"WHERE courier = {User.FindFirstValue("id")}; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								DeliveryViewModel d = new()
								{
									Id = (int)reader["id"],
									Email = reader["email"].ToString(),
									Address = reader["address"].ToString(),
									Courier = (int)reader["courier"],
									Status = reader["status"].ToString(),
								};

								deliveries.Add(d);
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

			return View(deliveries);
		}

		public async Task<IActionResult> FinishDelivery(int id)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"UPDATE shipping_details SET " +
							$"status = 'delivered', delivered_at = NOW() " +
							$"WHERE id = {id}; " +
							$"UPDATE orders SET " +
							$"status = 'delivered' " +
							$"WHERE id = {id}; ";

						Console.WriteLine(cmd.CommandText);

						await cmd.ExecuteNonQueryAsync();
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

			return RedirectToAction("TakeOrder", new { linkActive = "staff-takeorder" });
		}
	}
}
