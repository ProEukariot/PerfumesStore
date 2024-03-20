namespace PerfumesStore.Models.DbStatisticsViewModels
{
	public class UserRankingViewModel
	{
        public int PerfumesRank { get; set; }

        public int PerfumesBought { get; set; }

        public int PacksRank { get; set; }

        public int PacksBought { get; set; }

        public int PriceRank { get; set; }

        public decimal PriceSpent { get; set; }

        public int UserId { get; set; }
    }
}
