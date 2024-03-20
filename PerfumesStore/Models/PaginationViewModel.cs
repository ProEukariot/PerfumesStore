namespace PerfumesStore.Models
{
	public class PaginationViewModel
	{
		public int TotalPages { get; private set; }

		public int PageSize { get; set; }

		public int CurrentPage { get; set; }

		public PaginationViewModel(int current, int pageSize, int totalItems)
		{
			CurrentPage = current;
			PageSize = pageSize;
			TotalPages = totalItems / pageSize;
			int remainedItems = totalItems % pageSize;
			if (remainedItems > 0)
				TotalPages++;
		}

		public IEnumerable<ItemUnit> GetPage(IEnumerable<ItemUnit> collection)
		{
			collection = collection.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

			return collection;
		}

		public bool HasPage(int page) => page > 0 && page < TotalPages + 1;
	}
}
