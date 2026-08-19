using UnityEngine;

public static class CurrentBundleVersion
{
	private static TextAsset _versionTextAsset;

	private static string _version;

	public static string GetClientVersion()
	{
		if (_versionTextAsset != null)
		{
			return _version;
		}
		_versionTextAsset = Resources.Load<TextAsset>("client_version");
		if (_versionTextAsset == null)
		{
			return string.Empty;
		}
		_version = _versionTextAsset.text.Trim();
		if (_version.IndexOf('.') == _version.LastIndexOf('.'))
		{
			_version += ".0";
		}
		return _version;
	}
}
