using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PerfumesStore.Models;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PerfumesStore.Controllers
{
	public class LoginController : Controller
	{
		private NpgsqlConnection db;

		public LoginController(NpgsqlConnection db)
		{
			this.db = db;
		}

		[Route("Login")]
		public IActionResult Login() => View();

		[Route("Registration")]
		public IActionResult Registration() => View();

		[Route("Login")]
		[HttpPost]
		public async Task<IActionResult> Login(string? returnUrl, LoginViewModel vm)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id={vm.Username.ToLower()};Password={vm.Password}";
			try
			{
				if (ModelState.IsValid)
				{
					await db.OpenAsync();

					if (db.State == System.Data.ConnectionState.Open)
					{
						List<string> roles = new();

						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = db;
							cmd.CommandText = $"WITH RECURSIVE cte AS ( " +
								$"SELECT oid FROM pg_roles WHERE rolname = '{vm.Username.ToLower()}' " +
								$"UNION ALL " +
								$"SELECT m.roleid " +
								$"FROM cte " +
								$"JOIN pg_auth_members m ON m.member = cte.oid) " +
								$"SELECT oid, oid::regrole::text AS rolename FROM cte; ";

							using (var reader = await cmd.ExecuteReaderAsync())
							{
								while (await reader.ReadAsync())
								{
									Console.WriteLine(reader["rolename"].ToString()!);
									roles.Add(reader["rolename"].ToString()!);
								}
							}
						}

						string? roleDB = roles.FirstOrDefault(r => r.Equals("users") || r.Equals("staff") || r.Equals("admins"), null)!;

						UserRoles role = roleDB switch
						{
							"users" => UserRoles.User,
							"staff" => UserRoles.Staff,
							"admins" => UserRoles.Admin,
							_ => throw new Exception("User not in role!")
						};

						int userId = -1;

						if (role != UserRoles.Admin)
						{
							using (var cmd = new NpgsqlCommand())
							{
								cmd.Connection = db;
								if (role == UserRoles.User)
									cmd.CommandText = $"SELECT id FROM client WHERE username = '{vm.Username.ToLower()}'; ";
								if (role == UserRoles.Staff)
									cmd.CommandText = $"SELECT id FROM staff WHERE username = '{vm.Username.ToLower()}'; ";

								using (var reader = await cmd.ExecuteReaderAsync())
								{
									while (await reader.ReadAsync())
									{
										userId = (int)reader["id"];
										Console.WriteLine("-----LOGIN USER_ID " + userId.ToString());
									}
								}
							}

							if (userId == -1)
							{
								ModelState.AddModelError("", "Wrong data!");
								return View(vm);
							}
						}

						User user = new User(vm.Username, vm.Password) { Role = role, Id = userId };

						var claims = new List<Claim>
						{
							new Claim("Id", user.Id.ToString()),
							new Claim(ClaimTypes.Name, user.Username),
							new Claim("Password", user.Password),
							new Claim(ClaimTypes.Role, user.Role.ToString()),
						};

						ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

						await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

						return RedirectToAction("Hello", new { returnUrl = returnUrl ?? "/" });
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

		[Route("Registration")]
		[HttpPost]
		public async Task<IActionResult> Registration(string? returnUrl, RegistrationViewModel vm)
		{
			db.ConnectionString = $"Server=localhost;Port=5432;Database=PerfumeStore;User Id=postgres;Password=111";
			try
			{
				if (ModelState.IsValid)
				{
					User user = new User(vm.Username, vm.Password) { Email = vm.Email, Address = vm.Address, Role = UserRoles.User };

					await db.OpenAsync();

					if (db.State == System.Data.ConnectionState.Open)
					{
						using (var cmd = new NpgsqlCommand())
						{
							//cmd.Connection = db;
							cmd.Connection = db;
							cmd.CommandText = $"CREATE USER {user.Username.ToLower()} WITH PASSWORD '{user.Password}' IN ROLE Users " +
								$"NOSUPERUSER NOCREATEDB NOCREATEROLE LOGIN; " +
								//$"GRANT SELECT ON client TO {user.Username.ToLower()}; " +
								//$"GRANT USAGE ON POLICY \"access_client_policy\" ON client TO {user.Username.ToLower()}; " +
								$"INSERT INTO Client(Username, Email, Address, Created_at) VALUES " +
								$"('{user.Username.ToLower()}', '{user.Email}', '{user.Address}', NOW()); ";
							Console.WriteLine(cmd.CommandText);
							await cmd.ExecuteNonQueryAsync();
						}

						int userId = -1;

						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = db;
							cmd.CommandText = $"SELECT id FROM client WHERE username = '{vm.Username.ToLower()}'; ";

							using (var reader = await cmd.ExecuteReaderAsync())
							{
								while (await reader.ReadAsync())
								{
									Console.WriteLine("-----LOGIN USER_ID " + userId.ToString());
									userId = (int)reader["id"];
								}
							}
						}

						user.Id = userId;

						var claims = new List<Claim>
						{
							new Claim("Id", user.Id.ToString()),
							new Claim(ClaimTypes.Name, user.Username.ToLower()),
							new Claim("Password", user.Password),
							new Claim(ClaimTypes.Role, user.Role.ToString()),
						};

						ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

						await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

						return RedirectToAction("Hello", new { returnUrl = returnUrl ?? "/" });

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

		[Route("Logout")]
		[Authorize]
		public async Task<IActionResult> Logout(string? returnUrl)
		{
			HttpContext.Session.CommitAsync().GetAwaiter().GetResult();
			HttpContext.Session.Clear();

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return LocalRedirect(returnUrl ?? "/");
		}

		[Route("/Forbidden")]
		public IActionResult Forbidden(string? returnUrl)
		{
			HttpContext.Response.StatusCode = 403;
			return RedirectToAction("Error", "Home", new { id = HttpContext.Response.StatusCode });
		}

		[Authorize]
		public IActionResult Hello(string? returnUrl)
		{
			if (string.IsNullOrEmpty(returnUrl))
				returnUrl = "/";
			return View(new { returnUrl = returnUrl });
		}
	}
}
