namespace NPA;

public class NPNotificationData
{
	public int notificationID { get; set; }

	public string message { get; set; }

	public string meta { get; set; }

	public NPNotificationTime time { get; set; }

	public int pushType { get; set; }

	public int badgeNumber { get; set; }

	public NPNotificationData(int notificationID, string message, string meta, NPNotificationTime time, int pushType)
	{
		this.notificationID = notificationID;
		this.message = message;
		this.meta = meta;
		this.time = time;
		this.pushType = pushType;
		badgeNumber = 0;
	}

	public NPNotificationData(int notificationID, string message, string meta, NPNotificationTime time, int pushType, int badgeNumber)
	{
		this.notificationID = notificationID;
		this.message = message;
		this.meta = meta;
		this.time = time;
		this.pushType = pushType;
		this.badgeNumber = badgeNumber;
	}
}
