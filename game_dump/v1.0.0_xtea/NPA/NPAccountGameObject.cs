using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace NPA;

public class NPAccountGameObject : MonoBehaviour
{
	public Hashtable npaListenerTable = new Hashtable();

	public INPRecvNotificationListener pushListener;

	public INPGCMListener gcmListener;

	private void setListener(object[] paramsArray)
	{
		if (npaListenerTable == null)
		{
			npaListenerTable = new Hashtable();
		}
		npaListenerTable.Add((string)paramsArray[1], paramsArray[0]);
	}

	private void setPushListener(INPRecvNotificationListener pushListener)
	{
		this.pushListener = pushListener;
	}

	private void setGCMListener(INPGCMListener gcmListener)
	{
		this.gcmListener = gcmListener;
	}

	public void OnReceiveNotification(string msg)
	{
		NPAccount.ToyDebugLog("OnReceiveNoti " + msg);
		JSONNode recvNotification = JSON.Parse(msg);
		if (pushListener != null)
		{
			pushListener.OnRecvNotification(recvNotification);
		}
	}

	public void OnEndingBannerClick(string msg)
	{
		NPAccount.ToyDebugLog("OnEndingBannerClick " + msg);
		string landInfo = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPEndingBannerListener iNPEndingBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPEndingBannerListener = (INPEndingBannerListener)npaListenerTable[key];
		}
		iNPEndingBannerListener?.OnEndingBannerClick(landInfo);
	}

	public void OnEndingBannerDismiss(string msg)
	{
		NPAccount.ToyDebugLog("OnEndingBannerDismiss " + msg);
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPEndingBannerListener iNPEndingBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPEndingBannerListener = (INPEndingBannerListener)npaListenerTable[key];
		}
		iNPEndingBannerListener?.OnEndingBannerDismiss();
	}

	public void OnEndingBannerFailed(string msg)
	{
		NPAccount.ToyDebugLog("OnEndingBannerFailed " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		JSONNode jSONNode = JSON.Parse(aJSON);
		NPResult nPResult = new NPResult();
		nPResult.resultJson = jSONNode;
		nPResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
		nPResult.errorCode = jSONNode["errorCode"].AsInt;
		INPEndingBannerListener iNPEndingBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPEndingBannerListener = (INPEndingBannerListener)npaListenerTable[key];
		}
		iNPEndingBannerListener?.OnEndingBannerFailed(nPResult);
	}

	public void OnEndingBannerExit(string msg)
	{
		NPAccount.ToyDebugLog("OnEndingBannerExit " + msg);
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPEndingBannerListener iNPEndingBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPEndingBannerListener = (INPEndingBannerListener)npaListenerTable[key];
		}
		iNPEndingBannerListener?.OnEndingBannerExit();
	}

	public void OnBannerClick(string msg)
	{
		NPAccount.ToyDebugLog("OnBannerClick " + msg);
		string landInfo = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPBannerListener iNPBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPBannerListener = (INPBannerListener)npaListenerTable[key];
		}
		iNPBannerListener?.OnBannerClick(landInfo);
	}

	public void OnBannerDismiss(string msg)
	{
		NPAccount.ToyDebugLog("OnBannerDismiss " + msg);
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPBannerListener iNPBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPBannerListener = (INPBannerListener)npaListenerTable[key];
		}
		iNPBannerListener?.OnBannerDismiss();
	}

	public void OnBannerFailed(string msg)
	{
		NPAccount.ToyDebugLog("OnBannerFailed " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		JSONNode jSONNode = JSON.Parse(aJSON);
		NPResult nPResult = new NPResult();
		nPResult.resultJson = jSONNode;
		nPResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
		nPResult.errorCode = jSONNode["errorCode"].AsInt;
		INPBannerListener iNPBannerListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPBannerListener = (INPBannerListener)npaListenerTable[key];
		}
		iNPBannerListener?.OnBannerFailed(nPResult);
	}

	public void OnClose(string msg)
	{
		NPAccount.ToyDebugLog("OnClose " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		JSONNode jSONNode = JSON.Parse(aJSON);
		NPCloseResult nPCloseResult = new NPCloseResult();
		nPCloseResult.resultJson = jSONNode;
		nPCloseResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
		nPCloseResult.errorCode = jSONNode["errorCode"].AsInt;
		nPCloseResult.screenName = jSONNode["screenName"].Value;
		if (nPCloseResult.screenName.CompareTo("notice") == 0)
		{
			INPOnCloseListener iNPOnCloseListener = null;
			if (npaListenerTable.ContainsKey(key))
			{
				iNPOnCloseListener = (INPOnCloseListener)npaListenerTable[key];
			}
			iNPOnCloseListener?.OnClose(nPCloseResult);
		}
	}

	public void OnGameServiceClose(string msg)
	{
		NPAccount.ToyDebugLog("OnGameServiceClose " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		JSONNode jSONNode = JSON.Parse(aJSON);
		NPResult nPResult = new NPResult();
		nPResult.resultJson = jSONNode;
		nPResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
		nPResult.errorCode = jSONNode["errorCode"].AsInt;
		INPListener iNPListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPListener = (INPListener)npaListenerTable[key];
		}
		iNPListener?.OnResult(nPResult);
	}

	public void OnResult(string msg)
	{
		NPAccount.ToyDebugLog("OnResult " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		JSONNode jSONNode = JSON.Parse(aJSON);
		NPResult nPResult = new NPResult();
		nPResult.resultJson = jSONNode;
		nPResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
		nPResult.errorCode = jSONNode["errorCode"].AsInt;
		INPListener iNPListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPListener = (INPListener)npaListenerTable[key];
		}
		iNPListener?.OnResult(nPResult);
	}

	public void OnActionPerformedResult(string msg)
	{
		NPAccount.ToyDebugLog("OnActionPerformedResult " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPPlateListener iNPPlateListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPPlateListener = (INPPlateListener)npaListenerTable[key];
		}
		if (iNPPlateListener != null)
		{
			JSONNode jSONNode = JSON.Parse(aJSON);
			NPResult nPResult = new NPResult();
			nPResult.resultJson = jSONNode;
			nPResult.requestTag = (NPRequestTypeTag)jSONNode["requestTag"].AsInt;
			nPResult.errorCode = jSONNode["errorCode"].AsInt;
			iNPPlateListener.OnActionPerformedResult(nPResult);
		}
	}

	public void OnGCMResult(string msg)
	{
		NPAccount.ToyDebugLog("OnGCMResult " + msg);
		if (gcmListener != null)
		{
			gcmListener.OnGCMResult(int.Parse(msg));
		}
	}

	public void OnRequestPermissionsResult(string msg)
	{
		NPAccount.ToyDebugLog("OnRequestPermissionsResult " + msg);
		string aJSON = msg.Substring(0, msg.LastIndexOf("$"));
		string key = msg.Substring(msg.LastIndexOf("$") + 1);
		INPRuntimePermissionListener iNPRuntimePermissionListener = null;
		if (npaListenerTable.ContainsKey(key))
		{
			iNPRuntimePermissionListener = (INPRuntimePermissionListener)npaListenerTable[key];
		}
		if (iNPRuntimePermissionListener != null)
		{
			JSONNode jSONNode = JSON.Parse(aJSON);
			int asInt = jSONNode["requestCode"].AsInt;
			JSONArray asArray = jSONNode["permissions"].AsArray;
			string[] array = new string[asArray.Count];
			for (int i = 0; i < asArray.Count; i++)
			{
				array[i] = asArray[i];
			}
			JSONArray asArray2 = jSONNode["grantResults"].AsArray;
			int[] array2 = new int[asArray2.Count];
			for (int j = 0; j < asArray2.Count; j++)
			{
				array2[j] = asArray2[j].AsInt;
			}
			iNPRuntimePermissionListener.OnRequestPermissionsResult(asInt, array, array2);
		}
	}
}
