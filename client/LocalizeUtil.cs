using System;
using L10N;

public static class LocalizeUtil
{
	public static string Get(Enum e)
	{
		return LocalizeSystem.Get(GetKey(e));
	}

	public static string GetKey(Enum e)
	{
		return $"#{e.GetType().FullName}.{e}";
	}

	public static string FormatLevel(int lv)
	{
		return T._("{0:lv:}", lv);
	}

	public static string GetNameRoleHelpText()
	{
		return T._("한글 10자, 영문 20자, 띄어쓰기, 특수문자 <em>-</em>및 <em>.</em> 사용 가능");
	}

	public static string GetNameRoleDescription()
	{
		return T._("<em>띄어쓰기와 특수문자 제한</em>\n처음과 마지막 글자가 될 수 없습니다.\n띄어쓰기는 한 번만 사용할 수 있습니다.\n특수문자는 연이어 쓸 수 없습니다.");
	}

	public static string GetProbabilityLink()
	{
		return LocalizeSystem.Locale switch
		{
			"ko_KR" => "https://m.nexon.com/terms/271", 
			"th_TH" => "https://m.nexon.com/terms/492", 
			"zh_TW" => "https://m.nexon.com/terms/493", 
			_ => "https://m.nexon.com/terms/484", 
		};
	}
}
