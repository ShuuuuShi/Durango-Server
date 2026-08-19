namespace com.adjust.sdk;

public static class AdjustEnvironmentExtension
{
	public static string lowercaseToString(this AdjustEnvironment adjustEnvironment)
	{
		return adjustEnvironment switch
		{
			AdjustEnvironment.Sandbox => "sandbox", 
			AdjustEnvironment.Production => "production", 
			_ => "unknown", 
		};
	}
}
