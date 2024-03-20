namespace PerfumesStore.Models.DbStatisticsViewModels
{
	public class GeneralSeasonalStat
	{
        public int Year { get; set; }

        public string Season { get; set; }

        public decimal Sum { get; set; }

        public decimal? Prev { get; set; }

        public decimal? Diff { get; set; }
    }
}
