using Durango.Network;
using Messages;

public static class EstateLicenseExtension
{
	public static bool IsProtected(this EstateLicense licenses)
	{
		return licenses.ProtectedUntil.HasValue && licenses.ProtectedUntil.Value >= Connections.Frontend.GetPredictedServerTime();
	}
}
