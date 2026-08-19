using UnityEngine;

public static class UIExtension
{
	public static string ToEncodedColor(this string text, Color c)
	{
		return "[" + NGUIText.EncodeColor32(c) + "]" + text + "[-]";
	}

	public static string ToEncodedColor(this string text, string colorCode)
	{
		return "[" + colorCode + "]" + text + "[-]";
	}

	public static string ToEncodedIcon(this string text)
	{
		return "[icon=" + text + "]";
	}

	public static bool SetActiveAnd(this GameObject obj, bool activate)
	{
		obj.gameObject.SetActive(activate);
		return activate;
	}

	public static T FindComponent<T>(this GameObject obj, string childName) where T : MonoBehaviour
	{
		Transform transform = obj.transform.Find(childName);
		if (transform != null)
		{
			return transform.GetComponent<T>();
		}
		return null;
	}
}
