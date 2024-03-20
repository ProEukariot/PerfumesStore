using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PerfumesStore.Models
{
	public class Perfume : ItemUnit
	{
		public string Brand { get; set; }

		public Perfume() : base()
		{
		}

		public Perfume(int id, string name, string brand, string state, decimal price, string desc)
			: base(id, name, state, price, desc)
		{
			Brand = brand;
		}

	}
}
