namespace PerfumesStore.Models
{
	public class HistoryOrderViewModel
	{
        public int ItemId { get; set; }

		public string ItemName { get; set; }

		public decimal Price { get; set; }

        public int OrderId { get; set; }

        public int Quantity { get; set; }

        public string Type { get; set; }

        public decimal TotalPrice  { get; set; }

        public DateTime Created_at { get; set; }

        public int ClientId { get; set; }
    }
}
