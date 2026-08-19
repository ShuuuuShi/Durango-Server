using System.Reflection;

namespace SharpRaven.Utilities;

public static class SystemUtil
{
	public static Module[] GetModules()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		return executingAssembly.GetModules();
	}
}
