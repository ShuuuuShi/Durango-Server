using Durango.Network;
using Messages;

public static class EstateLicenseExtension
{
	public static bool IsProtected(this EstateLicense licenses)
	{
		if (licenses.ProtectedUntil.HasValue)
		{
			return licenses.ProtectedUntil.Value >= Connections.Frontend.GetPredictedServerTime();
		}
		return false;
	}
}
