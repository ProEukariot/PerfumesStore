namespace PerfumesStore.Models
{
	public class GiftPack : ItemUnit
	{
		public GiftPack() : base()
		{
		}

		public GiftPack(int id, string name, string state, decimal price, string desc) 
            : base(id, name, state, price, desc)
        {
            
        }
    }
}
