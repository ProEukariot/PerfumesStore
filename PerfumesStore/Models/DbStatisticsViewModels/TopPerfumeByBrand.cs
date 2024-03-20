namespace PerfumesStore.Models.DbStatisticsViewModels
{
	public class TopPerfumeByBrand
	{
        public int rank { get; set; }

        public string Name { get; set; }

        public string Brand { get; set; }

        public int Bought { get; set; }
    }
}
