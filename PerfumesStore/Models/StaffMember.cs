using System.ComponentModel.DataAnnotations;

namespace PerfumesStore.Models
{
	public class StaffMember
	{
		public int Id { get; set; }

		public string Username { get; set; }

		public string Email { get; set; }

        public string Address { get; set; }

        public DateTime Created_at { get; set; }

        public StaffMember(int id, string name, string email, string address, DateTime dateTime)
        {
            Id = id;
            Username = name;
            Email = email;
            Address = address;
            Created_at = dateTime;
        }
    }
}
