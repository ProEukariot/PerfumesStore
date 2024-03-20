using System.ComponentModel;

namespace PerfumesStore.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Username { get; set; }

		public string Email { get; set; }

		public string Address { get; set; }

		public UserRoles Role { get; set; }

		public string Password { get; set; }

        public DateTime Created_at { get; set; }

        public User()
        {
        }

        public User(string name, string pass)
        {
			Username = name;
			Password = pass;
		}
    }

	public enum UserRoles
	{
		[Description("User")] User,

		[Description("Staff")] Staff,

		[Description("Admin")] Admin,

	}
}
