namespace NPA;

public interface INPRuntimePermissionListener : INPListenerType
{
	void OnRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults);
}
