using System.Runtime.CompilerServices;
using Npgsql;

namespace PerfumesStore
{
	public class DBNullToIntExtension
	{
		public static int? ToNullableInt(object value)
		{
			return value == DBNull.Value ? null : (int)value;
		}
	}
}
