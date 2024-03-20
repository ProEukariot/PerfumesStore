using System.ComponentModel.DataAnnotations;

namespace PerfumesStore.Models
{
	public abstract class ItemUnit
	{
		public int Id { get; set; }

        public int? Discount { get; set; }

		public int? DiscountId { get; set; }

		[Required(ErrorMessage = "Name is required! <<<")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Enter sex!<<<")]
		public string State { get; set; }

		[Required(ErrorMessage = "Enter price!<<<")]
		public decimal Price { get; set; }

		[StringLength(80, MinimumLength = 0, ErrorMessage = "0-80 chars<<<")]
		public string Desc { get; set; }

		[Range(1, 10000, ErrorMessage = "Range (0-80)<<<")]
		[Required(ErrorMessage = "Quantity required!<<<")]
		public int Quantity { get; set; }

		public ItemUnit()
        {
            
        }

        public ItemUnit(int id, string name, string state, decimal price, string desc)
        {
			Id = id;
			Name = name;
			State = state;
			Price = price;
			Desc = desc;
        }
    }
}
