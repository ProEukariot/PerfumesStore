using System.ComponentModel.DataAnnotations;

namespace PerfumesStore.Models
{
	public class ItemsSubmitViewModel
	{
		public Perfume? Perfume { get; set; }

		public GiftPack? GiftPack { get; set; }

		//[Required]
		//[Range(1, 10000, ErrorMessage = "Quantitty not in range 1-10000!")]
		//[Display(Name = "Quantity")]
  //      public int ItemsQuantity { get; set; }	

    }
}
