using System;
using Durango.UI;
using UnityEngine;

namespace Durango.System;

public class Platform_PC : Platform
{
	private const string LocalAccountIdKey = "durango_local_account_id";

	private static string _localAccountId;

	public override string NPSN
	{
		get
		{
			if (string.IsNullOrEmpty(_localAccountId))
			{
				_localAccountId = PlayerPrefs.GetString(LocalAccountIdKey, string.Empty);
				if (string.IsNullOrEmpty(_localAccountId))
				{
					_localAccountId = Guid.NewGuid().ToString("N");
					PlayerPrefs.SetString(LocalAccountIdKey, _localAccountId);
					PlayerPrefs.Save();
				}
			}
			return _localAccountId;
		}
	}

	public override bool IsPCStore => true;

	public override string AppBundleId => "com.nexon.durango.wildlands";

	public override bool IsLoginTypeGuest => false;

	public override bool IsConnectFacebook => false;

	public override bool IsConnectGooglePlus => false;

	public override bool IsAvailableOfferwall => false;

	// [แก้เอง] ค่าเริ่มต้น = UI มือถือ · สลับได้ที่ตั้งค่าในเกม (option:ui_mode)
	// env DURANGO_FORCE_PCUI=1 ยังบังคับ PC ได้ถ้าต้องการเทียบ
	public override bool UsePCUI
	{
		get
		{
			string env = global::System.Environment.GetEnvironmentVariable("DURANGO_FORCE_PCUI");
			if (env == "1" || string.Equals(env, "true", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			string mode = PlayerPrefs.GetString("option:ui_mode", string.Empty);
			if (string.Equals(mode, "pc", StringComparison.OrdinalIgnoreCase)
			    || string.Equals(mode, "PC", StringComparison.Ordinal))
			{
				return true;
			}
			if (string.Equals(mode, "mobile", StringComparison.OrdinalIgnoreCase)
			    || mode == "มือถือ")
			{
				return false;
			}
			return false;
		}
	}

	public override int DefaultUISize => 1280;

	public override bool UsePCRenderer => true;

	public override bool SupportPortrait => false;

	public override int DefaultRenderTargetSize => 1024;

	public override string PrologueMovieUrl => "http://db.kyllox.pe.kr/durango/movies/standard/prologue_movie.mp4";

	public static string PrologueMovieUrl_PC => "http://db.kyllox.pe.kr/durango/movies/standard/prologue_movie.mp4";

	public override bool GetScreenResolution(bool isPortrait, out int width, out int height)
	{
		Point2 screenResolution = GetScreenResolution();
		width = screenResolution.x;
		height = screenResolution.y;
		return true;
	}

	public static Point2 GetScreenResolution()
	{
		Point2 point = default(Point2);
		point.x = (int)((float)Screen.width * 96f / Screen.dpi);
		point.y = (int)((float)Screen.height * 96f / Screen.dpi);
		Point2 result = point;
		int num = Mathf.Max((int)((float)(result.x * UIManager.UISize) / 1280f), 1600);
		int y = Mathf.RoundToInt((float)num * UIAnchorPolicy.DefaultAspectRatio);
		result.x = num;
		result.y = y;
		return result;
	}
}
