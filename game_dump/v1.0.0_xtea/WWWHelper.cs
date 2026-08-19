using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class WWWHelper : MonoBehaviour
{
	public delegate void HttpRequestDelegate(int id, WWW www);

	private int requestId;

	private static WWWHelper instace;

	private static GameObject container;

	public static WWWHelper Instance
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if ((Object)(object)instace == (Object)null)
			{
				container = new GameObject();
				((Object)container).name = "WWWHelper";
				instace = container.AddComponent(typeof(WWWHelper)) as WWWHelper;
			}
			return instace;
		}
	}

	public event HttpRequestDelegate OnHttpRequest;

	public void get(int id, string url)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		WWW www = new WWW(url);
		((MonoBehaviour)this).StartCoroutine(WaitForRequest(id, www));
	}

	public void post(int id, string url, Dictionary<string, string> data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		WWWForm val = new WWWForm();
		foreach (KeyValuePair<string, string> datum in data)
		{
			val.AddField(datum.Key, datum.Value);
		}
		WWW www = new WWW(url, val);
		((MonoBehaviour)this).StartCoroutine(WaitForRequest(id, www));
	}

	private IEnumerator WaitForRequest(int id, WWW www)
	{
		yield return www;
		if (this.OnHttpRequest != null)
		{
			this.OnHttpRequest(id, www);
		}
		www.Dispose();
	}

	private JSONClass dicToJSON(Dictionary<string, string> dic)
	{
		JSONClass jSONClass = new JSONClass();
		foreach (KeyValuePair<string, string> item in dic)
		{
			if (item.Value == null)
			{
				jSONClass[item.Key] = string.Empty;
			}
			else
			{
				jSONClass[item.Key] = item.Value;
			}
		}
		return jSONClass;
	}
}
