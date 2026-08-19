using System.Collections.Generic;
using System.IO;
using Durango.Logic.Clusters;
using Durango.Utils;

namespace Durango.Offline;

public static class Servers
{
	public static IEnumerable<Server> GetServers(Dictionary<string, Cluster> clusters)
	{
		string[] directories = AppData.GetDirectories("offline", "*", SearchOption.TopDirectoryOnly);
		string[] array = directories;
		for (int i = 0; i < array.Length; i++)
		{
			string name = new DirectoryInfo(array[i]).Name;
			Cluster cluster = clusters.Get(name);
			if (cluster == null)
			{
				continue;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> name2 in cluster.Names)
			{
				string text = ((!(name2.Key == "en_US")) ? "[기록] " : "[Saved] ");
				dictionary.Add(name2.Key, text + name2.Value);
			}
			Server server = new Server(name, dictionary);
			if (server.Contexts.Count > 0)
			{
				yield return server;
			}
		}
		yield return new Server("free", new Dictionary<string, string>
		{
			{ "en_US", "Creative Island" },
			{ "ko_KR", "창작섬" }
		});
		yield return new Server("solo", new Dictionary<string, string>
		{
			{ "en_US", "Single Play Mode" },
			{ "ko_KR", "싱글플레이 모드" }
		});
		yield return new Server("multi", new Dictionary<string, string>
		{
			{ "en_US", "Multi Play Mode" },
			{ "ko_KR", "멀티플레이 모드" }
		});
		yield return new Server("online", new Dictionary<string, string>
		{
			{ "en_US", "Online Server (For Test)" },
			{ "ko_KR", "온라인 서버 (테스트)" }
		});
	}
}
