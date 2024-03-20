namespace PerfumesStore.Models
{
	public class CartItemViewModel
	{
		public int Id { get; set; }

		public string Type { get; set; }

		public string Name { get; set; }

		public decimal Price { get; set; }

		public int Amount { get; set; }

		public decimal TotalPrice { get => Amount * Price; }

	}
}
