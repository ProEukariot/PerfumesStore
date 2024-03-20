using System.Text.Json;

namespace PerfumesStore
{
	public static class SessionExtension
	{
		public static void SetJson<T>(this ISession session, string key, T obj)
		{
			session.SetString(key, JsonSerializer.Serialize<T>(obj));
		}

		public static T GetJson<T>(this ISession session, string key)
		{
			var sessionData = session.GetString(key) ?? "";
			return string.IsNullOrEmpty(sessionData) ? default(T) : JsonSerializer.Deserialize<T>(sessionData);
		}
	}
}
