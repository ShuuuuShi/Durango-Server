namespace BestHTTP.Statistics;

public struct GeneralStatistics
{
	public StatisticsQueryFlags QueryFlags;

	public int Connections;

	public int ActiveConnections;

	public int FreeConnections;

	public int RecycledConnections;

	public int RequestsInQueue;

	public int CacheEntityCount;

	public ulong CacheSize;

	public int CookieCount;

	public uint CookieJarSize;
}
