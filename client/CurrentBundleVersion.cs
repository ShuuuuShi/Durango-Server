using UnityEngine;

/// <summary>
/// เวอร์ชันของตัวเกม — ส่งไปกับ /knock, โชว์ที่มุมล่างหน้าไตเติ้ล และใช้เทียบว่าต้องบังคับอัปเดตไหม
///
/// [แก้เอง] 31 ส.ค. 2026 — เดิมอ่านจาก TextAsset "client_version" ในตัวเกม (ค่า "5.2.1" ของ NEXON)
/// ซึ่งฝังอยู่ใน resources.assets แก้ไม่ได้ถ้าไม่ประกอบ asset ใหม่
/// เจ้าของสั่งให้เป็นเวอร์ชันของเราเอง และขยับทุกครั้งที่ปล่อยรุ่นใหม่
/// ⇒ คืนค่าของเราตรงนี้แทน (คุมได้จากซอร์ส ไม่ต้องแตะ asset)
///
/// ⚠️ ค่านี้ถูกส่งไปกับ /knock?version=... และเซิร์ฟเอาไปเทียบกับ Client.RequiredVersion
///    ถ้าไม่ตรง เซิร์ฟจะตอบ compatible=false แล้วเกมจะพาผู้เล่นไปหน้าโหลด (ดู TitleMenuGroup.KnockSystem)
///    **ขยับเลขนี้พร้อมกับ dist/manifest.json, version.txt และ Gateway.ServerVersion เสมอ**
/// </summary>
public static class CurrentBundleVersion
{
	/// <summary>เวอร์ชันชุดแจกของเรา — ขยับทุกครั้งที่ปล่อย release</summary>
	public const string CustomVersion = "CustomClient 0.1.6";

	/// <summary>
	/// เวอร์ชันของเกมต้นฉบับ (NEXON) — ยังต้องใช้กับ path ของ assetbundle
	/// อย่าเอา CustomVersion ไปแทนตรงนั้น ไม่งั้นหา bundle ไม่เจอ
	/// </summary>
	public const string BaseGameVersion = "5.2.1";

	private static TextAsset _versionTextAsset;

	private static string _version;

	public static string GetClientVersion()
	{
		return CustomVersion;
	}

	/// <summary>ค่าที่ฝังมากับตัวเกมจริง ๆ (เผื่อมีที่ไหนต้องใช้ของเดิม)</summary>
	public static string GetBundledVersion()
	{
		if (_versionTextAsset != null)
		{
			return _version;
		}
		_versionTextAsset = Resources.Load<TextAsset>("client_version");
		if (_versionTextAsset == null)
		{
			return BaseGameVersion;
		}
		_version = _versionTextAsset.text.Trim();
		if (_version.IndexOf('.') == _version.LastIndexOf('.'))
		{
			_version += ".0";
		}
		return _version;
	}
}
