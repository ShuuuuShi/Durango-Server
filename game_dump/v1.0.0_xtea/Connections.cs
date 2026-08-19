using K1Network;

public class Connections
{
	private static Connection _frontend;

	private static Connection _radiotower;

	public static Connection Frontend
	{
		get
		{
			if (_frontend == null)
			{
				_frontend = new Connection(timeSynchronize: true);
			}
			return _frontend;
		}
	}

	public static Connection Radiotower
	{
		get
		{
			if (_radiotower == null)
			{
				_radiotower = new Connection();
			}
			return _radiotower;
		}
	}

	public static void Reset()
	{
		_frontend = null;
		_radiotower = null;
	}
}
