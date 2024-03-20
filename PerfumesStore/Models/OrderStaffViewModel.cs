namespace PerfumesStore.Models
{
	public class OrderStaffViewModel
	{
        public int Id { get; set; }

        public string Status { get; set; }

        public decimal Total { get; set; }

        public DateTime Created_at { get; set; }

        public int Client_id { get; set; }
    }
}
