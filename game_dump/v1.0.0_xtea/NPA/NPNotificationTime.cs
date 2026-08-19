namespace NPA;

public class NPNotificationTime
{
	public int year { get; set; }

	public int month { get; set; }

	public int day { get; set; }

	public int hour { get; set; }

	public int minute { get; set; }

	public int sec { get; set; }

	public NPNotificationTime(int year, int month, int day, int hour, int minute, int sec)
	{
		this.year = year;
		this.month = month;
		this.day = day;
		this.hour = hour;
		this.minute = minute;
		this.sec = sec;
	}
}
