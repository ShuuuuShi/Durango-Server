using System;

namespace Durango.System;

public class Platform_Android : Platform
{
	public override void RequestPermission(string permission, Action<bool> callback)
	{
		AndroidPermissionsManager.RequestPermission("android.permission." + permission, new AndroidPermissionCallback(delegate
		{
			callback(obj: true);
		}, delegate
		{
			callback(obj: false);
		}, delegate
		{
			callback(obj: false);
		}));
	}
}
