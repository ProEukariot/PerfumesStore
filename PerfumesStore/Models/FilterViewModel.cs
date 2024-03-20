using System.Text.RegularExpressions;

namespace PerfumesStore.Models
{
	public class FilterViewModel
	{
		public string NameFilter { get; set; }

		public string BrandFilter { get; set; }

		public string StateFilter { get; set; }

		public decimal PriceFilter { get; set; }

		public FilterViewModel(string name, string brand, string state, decimal price)
		{
			NameFilter = name;
			BrandFilter = brand;
			StateFilter = state;
			PriceFilter = price;
		}

		public IEnumerable<ItemUnit> Filter(IEnumerable<ItemUnit> collection)
		{
			Regex regex = new Regex(NameFilter, RegexOptions.IgnoreCase);
			collection = collection.Where(p => regex.IsMatch(p.Name));

			if (!BrandFilter.ToLower().Equals("All".ToLower()))
				collection = collection.Where(p =>
				{
					if (p is Perfume)
					{
						return (p as Perfume).Brand.ToLower().Equals(BrandFilter.ToLower());
					}
					else
						return false;
				});

			if (!StateFilter.ToLower().Equals("All".ToLower()))
				collection = collection.Where(p => p.State.ToLower().Equals(StateFilter.ToLower()));

			collection = collection.Where(p => p.Price <= PriceFilter);

			return collection;
		}
	}
}
