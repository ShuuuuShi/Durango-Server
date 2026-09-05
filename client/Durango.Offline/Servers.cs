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
		// [แก้เอง] 1 ก.ย. 2026 — เซิร์ฟชุมชนต้องมาเป็นตัวแรก = ตัวที่ถูกเลือกเป็นค่าเริ่มต้น
		// 🐛 ก่อนหน้านี้ "Creative Island" (โหมดออฟไลน์ในเครื่อง) อยู่บนสุด ⇒ ผู้เล่นใหม่กด "เริ่ม"
		//    ทันทีจะเข้าโลกออฟไลน์ ไม่ใช่เซิร์ฟเรา และเซฟออฟไลน์ในชุดแจกโหลดไม่ขึ้น
		//    ⇒ เด้งกลับหน้าไตเติ้ลพร้อม "ไม่สามารถโหลดข้อมูลผู้เล่นได้" (ยืนยันจากการเทสจริง 1 ก.ย.)
		yield return new Server("online", new Dictionary<string, string>
		{
            // [แก้เอง] 31 ส.ค. 2026 — ห้ามใส่ NGUI BBCode สี "[C2185B]...[-]" ตรงนี้
            // label บางตัว (เช่นปุ่ม Back มุมซ้ายบนหน้า Select Character) ไม่ได้เปิด supportEncoding
            // จะโชว์แท็กดิบ ๆ ให้ผู้เล่นเห็น ⇒ ใช้ข้อความล้วนเท่านั้น
            { "en_US", "DurangoTH Community Server" },
            { "ko_KR", "DurangoTH Community Server" }
		});
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
	}
}
