using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PerfumesStore.Models;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using PerfumesStore;

namespace PerfumesStore.Controllers
{
	public class HomeController : BaseController
	{
		private IEnumerable<ItemUnit> items;
		private NpgsqlConnection db;

		public HomeController(NpgsqlConnection db)
		{
			this.db = db;
			items = new List<ItemUnit>();
		}

		public async Task<IActionResult> Index(decimal? price, string name = "", string brand = "All", string state = "All", SortState sort = SortState.NameAsc, int page = 1)
		{
			db.ConnectionString = "Server=localhost;Port=5432;Database=PerfumeStore;User Id=guest;Password=111";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "SELECT p.id, name, brand, state, price, description, value FROM perfumes p " +
							"LEFT JOIN discounts d ON d.id = p.discount_id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								var perfume = new Perfume(
									(int)reader["id"],
										reader["name"].ToString(),
										reader["brand"].ToString(),
										reader["state"].ToString(),
										(decimal)reader["price"],
										reader["description"].ToString()
									) {Discount = DBNullToIntExtension.ToNullableInt(reader["value"]) };
								(items as List<ItemUnit>).Add(perfume);
							}
						}
						cmd.CommandText = "SELECT g.id, name, state, price, description, value FROM gift_packs g " +
							"LEFT JOIN discounts d ON d.id = g.discount_id; ";

						Console.WriteLine(cmd.CommandText);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								var pack = new GiftPack(
									(int)reader["id"],
										reader["name"].ToString(),
										reader["state"].ToString(),
										(decimal)reader["price"],
										reader["description"].ToString()
									)
								{ Discount = DBNullToIntExtension.ToNullableInt(reader["value"])  };
								(items as List<ItemUnit>).Add(pack);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
			}
			finally
			{
				await db.CloseAsync();

			}

			var pageParams = new PageParamsViewModel();
			decimal maxPrice = 10000;

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = $"SELECT MAX(price) FROM " +
							$"(SELECT price FROM perfumes " +
							$"UNION SELECT price FROM gift_packs) as q; ";

						Console.WriteLine(cmd.CommandText);
						maxPrice = (decimal)await cmd.ExecuteScalarAsync();

					}
				}
			}
			catch (Exception e)
			{
			}
			finally
			{
				await db.CloseAsync();
				await db.DisposeAsync();
			}

			pageParams.MaxPrice = maxPrice;

			if (!price.HasValue)
				price = maxPrice;

			var filter = new FilterViewModel(name, brand, state, price.Value);

			items = filter.Filter(items);

			var sorting = new SortingViewModel(sort);

			items = sorting.Sort(items);

			int totalSorted = items.Count();

			var paging = new PaginationViewModel(page, 4, totalSorted);

			items = paging.GetPage(items);

			var vm = new IndexViewModel()
			{
				Perfumes = items,
				Filter = filter,
				Sorting = sorting,
				Pagination = paging,
				pageParams = pageParams
			};

			return View(vm);


		}

		public IActionResult Privacy()
		{
			return View();
		}

		[Authorize(Roles = "User")]
		public async Task<IActionResult> ToCart(int? id, string? returnUrl, string type = "perfume", int page = 1)
		{
			if (!id.HasValue)
				return LocalRedirect(returnUrl ?? "/");

			var cartData = HttpContext.Session.GetJson<CartViewModel>("Cart");

			if (cartData == null)
				cartData = new CartViewModel();

			string name = "";
			decimal price = 0;

			db.ConnectionString = "Server=localhost;Port=5432;Database=PerfumeStore;User Id=guest;Password=111";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "";
						if (type.ToLower() == "perfume")
						{
							cmd.CommandText = $"SELECT name, price FROM perfumes " +
								$"WHERE id = (@id); ";
						}
						else if (type.ToLower() == "giftpack")
						{
							cmd.CommandText = $"SELECT name, price FROM gift_packs " +
								$"WHERE id = (@id); ";

						}
						else
						{
							throw new Exception("No such an item!");
						}

						cmd.Parameters.AddWithValue("id", id);


						Console.WriteLine(cmd.CommandText);
						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								name = reader["name"].ToString();
								price = (decimal)reader["price"];

							}
						}

					}
				}
			}
			catch (Exception e)
			{
				return RedirectToAction("Error", "Home", new { id = 400, message = e.Message});
			}
			finally
			{
				await db.CloseAsync();
				await db.DisposeAsync();
			}

			cartData.AddItem(new CartItemViewModel() { Id = id!.Value, Name = name, Type = type, Price = price, Amount = 1 });

			HttpContext.Session.SetJson<CartViewModel>("Cart", cartData);

			returnUrl += $"?page={page}";

			return LocalRedirect(returnUrl ?? "/");
		}

		[Authorize(Roles = "User")]
		public IActionResult RemoveFromCart(int? id, string? returnUrl)
		{
			if (!id.HasValue)
				return LocalRedirect(returnUrl ?? "/");

			var cartData = HttpContext.Session.GetJson<CartViewModel>("Cart");

			cartData?.RemoveItem(id.Value);

			HttpContext.Session.SetJson<CartViewModel>("Cart", cartData);

			return LocalRedirect(returnUrl ?? "/");
		}

		[Authorize(Roles = "User")]
		public IActionResult RemoveLineFromCart(int? id, string? returnUrl)
		{
			if (!id.HasValue)
				return LocalRedirect(returnUrl ?? "/");

			var cartData = HttpContext.Session.GetJson<CartViewModel>("Cart");

			cartData.RemoveLineById(id.Value);

			HttpContext.Session.SetJson<CartViewModel>("Cart", cartData);

			return LocalRedirect(returnUrl ?? "/");
		}

		[Authorize(Roles = "User")]
		public IActionResult ClearCart(string? returnUrl)
		{
			HttpContext.Session.SetString("Cart", "");

			return LocalRedirect(returnUrl ?? "/"); ;
		}

		[Authorize(Roles = "User")]
		public async Task<IActionResult> BuyItems()
		{
			//HttpContext.Session.SetString("Cart", "");

			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={User.FindFirstValue(ClaimTypes.Name).ToLower()};Password={User.FindFirstValue("Password")}";

			try
			{
				await db.OpenAsync();
				if (db.State == System.Data.ConnectionState.Open)
				{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = db;
						cmd.CommandText = "";

						var cartData = HttpContext.Session.GetJson<CartViewModel>("Cart");

						string command = "";
						command += $"INSERT INTO customer_order(created_at, client_id, status) VALUES " +
					$"(NOW(), (@userid), 'created'); ";

						Console.WriteLine("PARAM USER_ID " + User.FindFirstValue("Id"));
						cmd.Parameters.AddWithValue("userid", int.Parse(User.FindFirstValue("Id")!));

						int i = 0;
						foreach (var cartItem in cartData.cartItems)
						{
							if (cartItem.Amount > 0)
							{
								i++;
								if (cartItem.Type.ToLower() == "perfume")
								{
									command += $"UPDATE perfume_inventory " +
										$"SET quantity = quantity - {cartItem.Amount} " +
										$"WHERE perfume_id = {cartItem.Id}; " +
										$"INSERT INTO perfume_orders(quantity, order_id, perfume_id) VALUES " +
										$"({cartItem.Amount}, (SELECT curr_orders_id()), {cartItem.Id}); ";
								}
								else if (cartItem.Type.ToLower() == "giftpack")
								{
									command += $"UPDATE pack_inventory " +
										$"SET quantity = quantity - {cartItem.Amount} " +
										$"WHERE pack_id = {cartItem.Id}; " +
										$"INSERT INTO pack_orders(quantity, order_id, pack_id) VALUES " +
										$"({cartItem.Amount}, (SELECT curr_orders_id()), {cartItem.Id}); ";
								}
								else
								{
									//throw new Exception();
								}

								//Console.WriteLine("PARAM cartItem.Amount " + cartItem.Amount);
								//Console.WriteLine("PARAM cartItem.Id " + cartItem.Id);

								//cmd.Parameters.AddWithValue("cartitemamount", cartItem.Amount);
								//cmd.Parameters.AddWithValue("cartitemid", cartItem.Id);
						
							}
						}

						// insert into orders
						command += "UPDATE customer_order SET " +
							"total = (SELECT count_total_ord(curr_orders_id())) " +
							"WHERE id = (SELECT curr_orders_id()); ";

						if (i == 0)
							command = "";

						Console.WriteLine(command);
						cmd.CommandText = command;

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

			HttpContext.Session.SetString("Cart", "");

			return View("PaymentPage");
		}

		//public IActionResult EditItem(int? id)
		//{
		//	if (!id.HasValue)
		//		return RedirectToAction("Index");


		//	return View(id);
		//}

		public IActionResult Error(int? id, string? message)
		{
			return View(new ErrorViewModel() { ErrorCode = id, ErrorMessage = message });
		}
	}
}