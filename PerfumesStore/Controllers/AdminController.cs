using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;
using PerfumesStore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using PerfumesStore.Models.DbStatisticsViewModels;
using System.Text.RegularExpressions;

namespace PerfumesStore.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private NpgsqlConnection db;

		public AdminController(NpgsqlConnection db)
		{
			this.db = db;
		}

		public IActionResult AddStaff() => View();

		public IActionResult AddItem() => View();

		public IActionResult Settings()
		{
			return View();
		}

		public async Task<IActionResult> Statistics()
		{

			List<OrdersWithOnlyGiftPacks> ordersWithOnlyGiftPacks = new();
			List<int> commonDiscounts = new();
			List<string> brandsInEachOrder = new();
			List<CustomerWithOrderPrice> customerWithOrderPrice = new();
			List<UserRankingViewModel> userRanking = new();
			List<GeneralSeasonalStat> generalSeasonalStats = new();
			List<TopPerfumeByBrand> topOfEachBrand = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "SELECT o.* FROM orders o INNER JOIN " +
							"(SELECT o.id FROM orders o " +
							"INNER JOIN pack_orders po ON po.order_id = o.id " +
							"except " +
							"SELECT o.id FROM orders o " +
							"INNER JOIN perfume_orders po ON po.order_id = o.id) AS q " +
							"ON q.id = o.id " +
							"ORDER BY o.id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								OrdersWithOnlyGiftPacks order = new()
								{
									Id = (int)reader["id"],
									Total = (decimal)reader["total"],
									Status = reader["status"].ToString(),
									CreatedAt = Convert.ToDateTime(reader["created_at"]),
									ClientId = (int)reader["client_id"]
								};

								ordersWithOnlyGiftPacks.Add(order);
							}
						}

						cmd.CommandText = "SELECT q.discount_id, d.value FROM " +
							"(SELECT p.discount_id FROM perfumes p " +
							"intersect " +
							"SELECT p.discount_id FROM gift_packs p) AS q " +
							"INNER JOIN discounts d ON d.id = q.discount_id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								commonDiscounts.Add((int)reader["value"]);
							}
						}

						cmd.CommandText = "SELECT p.brand FROM Perfumes p " +
							"INNER JOIN perfume_orders po ON po.perfume_id = p.id " +
							"INNER JOIN orders o ON o.id = po.order_id " +
							"GROUP BY p.brand " +
							"HAVING(COUNT(DISTINCT o.id) = (SELECT COUNT(*) FROM orders)); ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								brandsInEachOrder.Add(reader["brand"].ToString());
							}
						}

						cmd.CommandText = "SELECT c.id, c.username, SUM(p.price * po.quantity) FROM Perfumes p " +
							"INNER JOIN perfume_orders po ON po.perfume_id = p.id " +
							"INNER JOIN orders o ON o.id = po.order_id " +
							"INNER JOIN client c ON o.client_id = c.id " +
							"GROUP BY c.id " +
							"ORDER BY SUM(p.price * po.quantity); ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								CustomerWithOrderPrice c = new()
								{
									Username = reader["username"].ToString(),
									Sum = (decimal)reader["sum"],
								};

								customerWithOrderPrice.Add(c);
							}
						}

						cmd.CommandText = "SELECT * FROM users_ranks(); ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								UserRankingViewModel r = new()
								{
									PerfumesRank = Convert.ToInt32(reader["perfumes_rank"]),
									PerfumesBought = Convert.ToInt32(reader["perfumes_bought"] is DBNull ? 0 : reader["perfumes_bought"]),
									PacksRank = Convert.ToInt32(reader["packs_rank"]),
									PacksBought = Convert.ToInt32(reader["packs_bought"] is DBNull ? 0 : reader["packs_bought"]),
									PriceRank = Convert.ToInt32(reader["price_rank"]),
									PriceSpent = Convert.ToInt32(reader["total_sum"]),
									UserId = Convert.ToInt32(reader["user_id"]),
								};

								userRanking.Add(r);
							}
						}

						cmd.CommandText = "SELECT * FROM seasonal_stats(); ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								GeneralSeasonalStat stat = new()
								{
									Year = Convert.ToInt32(reader["year"]),
									Season = reader["season"].ToString(),
									Sum = Convert.ToDecimal(reader["sum"]),
									Diff = (reader["diff"] is DBNull) ? null : Convert.ToDecimal(reader["prev"]),
									Prev = (reader["prev"] is DBNull) ? null : Convert.ToDecimal(reader["diff"]),
								};

								generalSeasonalStats.Add(stat);
							}
						}

						cmd.CommandText = "SELECT * FROM topOfEachBrand(); ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								TopPerfumeByBrand p = new()
								{
									rank = Convert.ToInt32(reader["rank"]),
									Name = Convert.ToString(reader["name"]),
									Brand = Convert.ToString(reader["brand"]),
									Bought = Convert.ToInt32(reader["bought"]),
								};

								topOfEachBrand.Add(p);
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

			BaseStatisticsViewModel baseVM = new()
			{
				ordersWithOnlyGiftPacks = ordersWithOnlyGiftPacks,
				CommonDiscounts = commonDiscounts,
				BrandsInEachOrder = brandsInEachOrder,
				CustomerWithOrderPrice = customerWithOrderPrice,
				UserRanking = userRanking,
				GeneralSeasonalStats = generalSeasonalStats,
				TopOfEachBrand = topOfEachBrand,
			};

			return View(baseVM);
		}

		public async Task<IActionResult> Items()
		{
			int? code = TempData["StatusCode"] as int?;
			if (code.HasValue)
				HttpContext.Response.StatusCode = code.Value;

			List<ItemUnit> items = new();

			try
			{

				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT p.*, pi.quantity FROM perfumes p " +
							$"LEFT JOIN perfume_inventory pi ON pi.perfume_id = p.id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								Perfume p = new Perfume(
									id: (int)reader["id"],
									name: reader["name"].ToString(),
									brand: reader["brand"].ToString(),
									state: reader["state"].ToString(),
									price: (decimal)reader["price"],
									desc: reader["description"].ToString()
									);
								int value;
								int.TryParse(reader["quantity"]?.ToString(), out value);
								p.Quantity = value;
								items.Add(p);
							}
						}

					}

					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "SELECT g.*, pi.quantity FROM gift_packs g " +
							$"LEFT JOIN pack_inventory pi ON pi.pack_id = g.id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								GiftPack p = new GiftPack(
									id: (int)reader["id"],
									name: reader["name"].ToString(),
									state: reader["state"].ToString(),
									price: (decimal)reader["price"],
									desc: reader["description"].ToString()
									);
								int value;
								int.TryParse(reader["quantity"]?.ToString(), out value);
								p.Quantity = value;
								items.Add(p);
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

			return View(items);
		}

		[HttpPost]
		public async Task<IActionResult> AddItem(ItemsSubmitViewModel vm)
		{
			if (ModelState.IsValid)
			{

				try
				{
					await db.OpenAsync();
					if (db.State == System.Data.ConnectionState.Open)
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = db;
							cmd.CommandText = "";

							if (vm.Perfume != null)
							{
								cmd.CommandText = $"INSERT INTO perfumes(name, brand, state, price, description) VALUES " +
									$"((@name), (@brand), (@state), (@price), (@desc)); " +
									$"INSERT INTO perfume_inventory(quantity, delivered_at, perfume_id) VALUES " +
									$"((@quantity), NOW(), (SELECT last_value FROM perfumes_id_seq)); ";
								cmd.Parameters.AddWithValue("name", vm.Perfume.Name);
								cmd.Parameters.AddWithValue("brand", vm.Perfume.Brand);
								cmd.Parameters.AddWithValue("state", vm.Perfume.State);
								cmd.Parameters.AddWithValue("price", vm.Perfume.Price);
								cmd.Parameters.AddWithValue("desc", vm.Perfume.Desc ?? "");
								cmd.Parameters.AddWithValue("quantity", vm.Perfume.Quantity);
							}

							Console.WriteLine(cmd.CommandText);

							if (vm.GiftPack != null)
							{
								cmd.CommandText = $"INSERT INTO gift_packs(name, state, price, description) VALUES " +
								$"((@name), (@state), (@price), (@desc)); " +
									$"INSERT INTO pack_inventory(quantity, delivered_at, pack_id) VALUES " +
									$"((@quantity), NOW(), (SELECT last_value FROM gift_packs_id_seq)); ";
								cmd.Parameters.AddWithValue("name", vm.GiftPack.Name);
								cmd.Parameters.AddWithValue("state", vm.GiftPack.State);
								cmd.Parameters.AddWithValue("price", vm.GiftPack.Price);
								cmd.Parameters.AddWithValue("desc", vm.GiftPack.Desc ?? "");
								cmd.Parameters.AddWithValue("quantity", vm.GiftPack.Quantity);
							}

							Console.WriteLine(cmd.CommandText);

							await cmd.ExecuteNonQueryAsync();

							TempData["StatusCode"] = 201;

							return RedirectToAction("Items", new { linkActive = "admin-items" });
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
			}

			return View(vm);
		}

		public async Task<IActionResult> EditItem(int? id, string type)
		{
			if (!id.HasValue)
				return Content("Empty id");

			GiftPack? pack = null;
			Perfume? perf = null;

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						if (type == "Perfume")
							cmd.CommandText = $"SELECT p.id, name, brand, state, price, description, quantity, value FROM perfumes p " +
								$"INNER JOIN perfume_inventory pi ON pi.perfume_id = p.id " +
								$"LEFT JOIN discounts d ON d.id = p.discount_id " +
								$"WHERE p.id = (@id) ORDER BY name LIMIT 1";
						else if (type == "GiftPack")
							cmd.CommandText = $"SELECT g.id, name, state, price, description, quantity, value FROM gift_packs g " +
								$"INNER JOIN pack_inventory pi ON pi.pack_id = g.id " +
								$"LEFT JOIN discounts d ON d.id = g.discount_id " +
								$"WHERE g.id = (@id) ORDER BY name LIMIT 1";
						else cmd.CommandText = "";

						cmd.Parameters.AddWithValue("id", id.Value);

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								if (type == "Perfume")
								{
									perf = new(
										(int)reader["id"],
										reader["name"].ToString(),
										reader["brand"].ToString(),
										reader["state"].ToString(),
										(decimal)reader["price"],
										reader["description"].ToString()

										)
									{
										Quantity = (int)reader["quantity"],
										Discount = DBNullToIntExtension.ToNullableInt(reader["value"])
									};
								}
								if (type == "GiftPack")
								{
									pack = new(
										(int)reader["id"],
										reader["name"].ToString(),
										reader["state"].ToString(),
										(decimal)reader["price"],
										reader["description"].ToString()
										)
									{
										Quantity = (int)reader["quantity"],
										Discount = DBNullToIntExtension.ToNullableInt(reader["value"])
									};
								}
							}
						}

					}
					if (type == "Perfume")
						return View("EditPerfume", perf);
					if (type == "GiftPack")
						return View("EditGiftPack", pack);

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

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> EditPerfume(Perfume vm)
		{
			if (ModelState.IsValid)
			{

				try
				{
					await db.OpenAsync();
					if (db.State == System.Data.ConnectionState.Open)
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = db;
							cmd.CommandText = "";


							cmd.CommandText = $"UPDATE perfumes SET " +
								$"name = (@name), " +
								$"brand = (@brand), " +
								$"discount_id = (@discount), " +
								$"state = (@state), " +
								$"price = (@price), " +
								$"description = (@desc) " +
								$"WHERE id = (@id); " +
								$"UPDATE perfume_inventory SET " +
								$"quantity = (@quantity), " +
								$"delivered_at = NOW() " +
								$"WHERE perfume_id = (@id); ";

							cmd.Parameters.AddWithValue("name", vm.Name);
							cmd.Parameters.AddWithValue("brand", vm.Brand);
							cmd.Parameters.AddWithValue("discount", vm.DiscountId);
							cmd.Parameters.AddWithValue("state", vm.State);
							cmd.Parameters.AddWithValue("price", vm.Price);
							cmd.Parameters.AddWithValue("desc", vm.Desc);
							cmd.Parameters.AddWithValue("id", vm.Id);
							cmd.Parameters.AddWithValue("quantity", vm.Quantity);

							Console.WriteLine(cmd.CommandText);

							await cmd.ExecuteNonQueryAsync();

							TempData["StatusCode"] = 201;

							return RedirectToAction("Items", new { linkActive = "admin-items" });
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
			}

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> DeletePerfume(int id)
		{
			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"DELETE FROM perfume_inventory WHERE perfume_id = (@id); " +
							$"DELETE FROM perfumes WHERE id = (@id); ";

						cmd.Parameters.AddWithValue("id", id);

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

			TempData["StatusCode"] = 201;

			return RedirectToAction("Items", new { linkActive = "admin-items" });
		}

		[HttpPost]
		public async Task<IActionResult> EditGiftPack(GiftPack vm)
		{
			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "";

						cmd.CommandText = $"UPDATE gift_packs SET " +
							$"name = (@name), " +
							$"state = (@state), " +
							$"discount_id = (@discountid), " +
							$"price = (@price), " +
							$"description = (@desc) " +
							$"WHERE id = (@id); " +
							$"UPDATE pack_inventory SET " +
							$"quantity = (@quantity), " +
							$"delivered_at = NOW() " +
							$"WHERE pack_id = (@id); ";

						cmd.Parameters.AddWithValue("name", vm.Name);
						cmd.Parameters.AddWithValue("state", vm.State);
						cmd.Parameters.AddWithValue("discountid", vm.DiscountId);
						cmd.Parameters.AddWithValue("price", vm.Price);
						cmd.Parameters.AddWithValue("desc", vm.Desc);
						cmd.Parameters.AddWithValue("id", vm.Id);
						cmd.Parameters.AddWithValue("quantity", vm.Quantity);

						Console.WriteLine(cmd.CommandText);

						await cmd.ExecuteNonQueryAsync();

						TempData["StatusCode"] = 201;

						return RedirectToAction("Items", new { linkActive = "admin-items" });
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

			return RedirectToAction("Items", new { linkActive = "admin-items" });
		}

		[HttpPost]
		public async Task<IActionResult> DeletePack(int id)
		{
			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"DELETE FROM pack_inventory WHERE pack_id = (@id); " +
							$"DELETE FROM gift_packs WHERE id = (@id); ";

						cmd.Parameters.AddWithValue("id", id);

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

			TempData["StatusCode"] = 201;

			return RedirectToAction("Items", new { linkActive = "admin-items" });
		}

		public async Task<IActionResult> Staff()
		{
			int? code = TempData["StatusCode"] as int?;
			if (code.HasValue)
				HttpContext.Response.StatusCode = code.Value;

			//db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={username.ToLower()};Password={password}";

			List<StaffMember> staff = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "SELECT id, username, email, address, created_at FROM staff " +
							"WHERE status = 'hired'; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								StaffMember user = new StaffMember(
									id: (int)reader["id"],
									name: reader["username"].ToString(),
									email: reader["email"].ToString(),
									address: reader["address"].ToString(),
									dateTime: Convert.ToDateTime(reader["created_at"])
									);

								staff.Add(user);
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

			return View(staff);
		}

		[HttpPost]
		public async Task<IActionResult> AddStaff(RegistrationViewModel vm)
		{
			//db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={username.ToLower()};Password={password}";

			try
			{
				if (ModelState.IsValid)
				{
					User user = new User(vm.Username, vm.Password) { Email = vm.Email, Address = vm.Address, Role = UserRoles.Staff };

					await db.OpenAsync();

					if (db.State == System.Data.ConnectionState.Open)
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = db;
							cmd.CommandText = $"CREATE USER {user.Username.ToLower()} WITH PASSWORD '{user.Password}' IN ROLE Staff " +
								$"NOSUPERUSER NOCREATEDB NOCREATEROLE LOGIN; " +
								$"INSERT INTO Staff(Username, Email, Address, Created_at, status) VALUES " +
								$"((@username), (@email), (@address), NOW(), 'hired'); ";
							cmd.Parameters.AddWithValue("username", user.Username.ToLower());
							cmd.Parameters.AddWithValue("password", "'" + user.Password + "'");
							cmd.Parameters.AddWithValue("email", user.Email);
							cmd.Parameters.AddWithValue("address", user.Address);
							Console.WriteLine(cmd.CommandText);

							Console.WriteLine(cmd.CommandText);

							await cmd.ExecuteNonQueryAsync();
						}

						TempData["StatusCode"] = 201;

						return RedirectToAction("Staff", new { linkActive = "admin-staff" });

					}
					else
					{
						throw new Exception("Db closed");
					}
				}
			}
			catch (Exception e)
			{
				ModelState.AddModelError("", e.Message);

			}
			finally
			{
				await db.CloseAsync();
				await db.DisposeAsync();
			}

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> FireStaff(int id)
		{
			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						string user = "";
						cmd.Connection = db;
						cmd.CommandText = $"SELECT username FROM staff WHERE id = (@id); ";
						cmd.Parameters.AddWithValue("id", id);
						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
								user = reader["username"].ToString();
						}

						//cmd.CommandText = $"DELETE FROM staff " +
						//	$"WHERE id = {id}; " +
						cmd.CommandText = $"DROP ROLE {user}; " +
					$"UPDATE staff SET " +
					$"status = 'fired' " +
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

			TempData["StatusCode"] = 201;

			return RedirectToAction("Staff", new { linkActive = "admin-staff" });
		}

		public async Task<IActionResult> CheckWork(int id)
		{
			List<WorkViewModel> works = new();

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT * FROM (SELECT status, finished_at, finished_by, 'packing' AS work_type FROM orders " +
							$"UNION ALL " +
							$"SELECT status, delivered_at, courier, 'delivery' FROM shipping_details) AS foo " +
							$"WHERE finished_at > NOW() - interval '60 days' AND finished_by = (@id) " +
							$"ORDER BY finished_at ASC; ";
						cmd.Parameters.AddWithValue("id", id);

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								WorkViewModel work = new()
								{
									Status = reader["status"].ToString(),
									WorkerId = (int)reader["finished_by"],
									WorkType = reader["work_type"].ToString(),
									FinishedAt = Convert.ToDateTime(reader["finished_at"]),
								};

								works.Add(work);
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

			return View(works);
		}
	}
}
