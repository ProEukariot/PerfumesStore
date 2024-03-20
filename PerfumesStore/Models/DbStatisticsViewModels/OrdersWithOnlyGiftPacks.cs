namespace PerfumesStore.Models.DbStatisticsViewModels
{
	public class OrdersWithOnlyGiftPacks
	{
        public int Id { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime FinishedAt { get; set; }

        public int FinishedBy { get; set; }

        public int ClientId { get; set; }
    }
}
