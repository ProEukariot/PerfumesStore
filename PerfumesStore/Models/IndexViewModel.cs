namespace PerfumesStore.Models
{
	public class IndexViewModel
	{
		public IEnumerable<PerfumesStore.Models.ItemUnit> Perfumes { get; set; } = new List<ItemUnit>();

        public PageParamsViewModel pageParams { get; set; }

		public FilterViewModel Filter { get; set; }

        public SortingViewModel Sorting { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public IndexViewModel() 
        {
            
        }
    }
}
