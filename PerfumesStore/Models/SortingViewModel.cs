using Microsoft.AspNetCore.Mvc;

namespace PerfumesStore.Models
{
	public class SortingViewModel
	{
        public SortState Current { get; }

		public SortState NameSort { get; }
		
		public SortState BrandSort { get; }
	
		public SortState PriceSort { get; }

        public SortingViewModel(SortState state)
        {
			Current = state;
			NameSort = state == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
			BrandSort = state == SortState.BrandAsc ? SortState.BrandDesc : SortState.BrandAsc;
			PriceSort = state == SortState.PriceAsc ? SortState.PriceDesc : SortState.PriceAsc;
		}

        public IEnumerable<ItemUnit> Sort(IEnumerable<ItemUnit> collection)
		{
			collection = Current switch
			{
				SortState.NameAsc => collection.OrderBy(p => p.Name),
				SortState.NameDesc => collection.OrderByDescending(p => p.Name),
				//SortState.BrandAsc => collection.OrderBy(p => p.Brand),
				//SortState.BrandDesc => collection.OrderByDescending(p => p.Brand),
				SortState.PriceAsc => collection.OrderBy(p => p.Price),
				SortState.PriceDesc => collection.OrderByDescending(p => p.Price),
				_ => collection
			};


			return collection;
		}
	}

	public enum SortState
	{
		NameAsc,
		NameDesc,
		BrandAsc,
		BrandDesc,
		PriceAsc,
		PriceDesc,
	}
}
