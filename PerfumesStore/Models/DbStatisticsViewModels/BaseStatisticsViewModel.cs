namespace PerfumesStore.Models.DbStatisticsViewModels
{
	public class BaseStatisticsViewModel
	{
		public IEnumerable<OrdersWithOnlyGiftPacks> ordersWithOnlyGiftPacks;

		public IEnumerable<int> CommonDiscounts;

		public IEnumerable<string> BrandsInEachOrder;

		public IEnumerable<CustomerWithOrderPrice> CustomerWithOrderPrice;

        public IEnumerable<UserRankingViewModel> UserRanking { get; set; }

        public IEnumerable<GeneralSeasonalStat> GeneralSeasonalStats { get; set; }

		public IEnumerable<TopPerfumeByBrand> TopOfEachBrand { get; set; }

	}
}
