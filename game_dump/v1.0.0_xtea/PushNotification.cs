using NPA;
using SimpleJSON;

public class PushNotification : INPListenerType, INPRecvNotificationListener
{
	public enum Type
	{
		Unknown,
		Debug,
		ProfessionReward,
		HealthRecovered,
		FatigueRecovered,
		SkillCategoryResearch
	}

	public enum PushMessageType
	{
		None,
		Chat
	}

	private const string senderID = "1083226560478";

	private static readonly NPNotificationTime timeNow = new NPNotificationTime(0, 0, 0, 0, 0, 0);

	private readonly NPAccount account;

	public PushNotification()
	{
		account = NPAccount.Instance;
	}

	public void Initialize()
	{
		account.pushInit(this, "1083226560478");
	}

	public void LocalPushNow(Type type, string message, string meta)
	{
		NPNotificationData data = new NPNotificationData((int)type, message, meta, timeNow, NPAccount.LOCAL_PUSH_TYPE_NOW);
		account.dispatchLocalPush(data);
	}

	public void LocalPushAfter(Type type, string message, string meta, int sec)
	{
		int currentValue = CalculateTimeValue(ref sec, 60);
		int currentValue2 = CalculateTimeValue(ref currentValue, 60);
		int currentValue3 = CalculateTimeValue(ref currentValue2, 24);
		int currentValue4 = CalculateTimeValue(ref currentValue3, 30);
		int year = CalculateTimeValue(ref currentValue4, 12);
		LocalPushAfter(type, message, meta, year, currentValue4, currentValue3, currentValue2, currentValue, sec);
	}

	public void LocalPushAfter(Type type, string message, string meta, int year, int month, int day, int hour, int minute, int sec)
	{
		NPNotificationTime time = new NPNotificationTime(year, month, day, hour, minute, sec);
		NPNotificationData data = new NPNotificationData((int)type, message, meta, time, NPAccount.LOCAL_PUSH_TYPE_AFTER);
		account.dispatchLocalPush(data);
	}

	public void CancelLocalPush(Type type)
	{
		account.cancelLocalPush((int)type);
	}

	public void CancelAllLocalPush()
	{
		account.cancelAllLocalPush();
	}

	public void OnRecvNotification(JSONNode recvNotification)
	{
		if (recvNotification == null)
		{
			return;
		}
		string text = null;
		int num = 0;
		JSONNode jSONNode = null;
		if (recvNotification["aps"] != null)
		{
			text = recvNotification["aps"]["alert"];
			JSONNode jSONNode2 = recvNotification["m"];
			if (jSONNode2 != null)
			{
				num = jSONNode2.AsInt;
			}
			jSONNode = recvNotification["t"];
		}
		else if (recvNotification["message"] != null)
		{
			text = recvNotification["message"];
			JSONNode jSONNode3 = recvNotification["msgType"];
			if (jSONNode3 != null)
			{
				num = jSONNode3.AsInt;
			}
			jSONNode = recvNotification["meta"];
		}
		if (num != 1 && text != null && KSingleton<UIManager>.HasInstance() && jSONNode != null && jSONNode.ToString() != "offline_only")
		{
			UIManager.Popup.Alarm.ShowAlarm(text, "alarm_memo", 4f);
		}
	}

	private static int CalculateTimeValue(ref int currentValue, int unit)
	{
		int result = currentValue / unit;
		currentValue %= unit;
		return result;
	}
}
