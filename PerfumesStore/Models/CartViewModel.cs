namespace PerfumesStore.Models
{
	public class CartViewModel
	{
		public CartViewModel()
		{
			cartItems = new List<CartItemViewModel>();
		}

		public IEnumerable<CartItemViewModel> cartItems { get; set; }
		public decimal TotalPrice
		{
			get
			{
				decimal sum = 0;
				foreach (var position in cartItems)
					sum += position.TotalPrice;
				return sum;
			}
		}

		public void AddItem(CartItemViewModel item)
		{
			var cartItemsList = cartItems.ToList();
			var cartItemIndex = cartItemsList.FindIndex(i => i.Id == item.Id);
			// find if in DB there are such a perfume
			var itemIndexDB = cartItemsList.FindIndex(i => i.Id == item.Id);

			if (itemIndexDB < 0)
			{
				// if no return;
			}

			if (cartItemIndex < 0)
			{
				var cartItem = new CartItemViewModel() { Id = item.Id, Name = item.Name, Type = item.Type, Price = item.TotalPrice, Amount = 1 };
				cartItemsList.Add(cartItem);
			}
			else
			{
				cartItemsList[cartItemIndex].Amount++;
			}
			cartItems = cartItemsList;
		}

		public void RemoveItem(int itemId)
		{
			var cartItemsList = cartItems.ToList();
			var cartItemIndex = cartItemsList.FindIndex(i => i.Id == itemId);

			if (cartItemIndex < 0)
				return;

			//if (cartItemIndex < 0)
			//{

			//}

			cartItemsList[cartItemIndex].Amount--;

			if (cartItemsList[cartItemIndex].Amount <= 0)
			{
				cartItemsList.RemoveAt(cartItemIndex);
			}

			cartItems = cartItemsList;
		}

		public void RemoveLineById(int itemId)
		{
			var cartItemsList = cartItems.ToList();
			var cartItemIndex = cartItemsList.FindIndex(i => i.Id == itemId);

			cartItemsList.RemoveAt(cartItemIndex);

			cartItems = cartItemsList;
		}
	}
}
