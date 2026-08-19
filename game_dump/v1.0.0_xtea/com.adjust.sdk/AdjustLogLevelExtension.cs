namespace com.adjust.sdk;

public static class AdjustLogLevelExtension
{
	public static string lowercaseToString(this AdjustLogLevel AdjustLogLevel)
	{
		return AdjustLogLevel switch
		{
			AdjustLogLevel.Verbose => "verbose", 
			AdjustLogLevel.Debug => "debug", 
			AdjustLogLevel.Info => "info", 
			AdjustLogLevel.Warn => "warn", 
			AdjustLogLevel.Error => "error", 
			AdjustLogLevel.Assert => "assert", 
			AdjustLogLevel.Suppress => "suppress", 
			_ => "unknown", 
		};
	}

	public static string uppercaseToString(this AdjustLogLevel AdjustLogLevel)
	{
		return AdjustLogLevel switch
		{
			AdjustLogLevel.Verbose => "VERBOSE", 
			AdjustLogLevel.Debug => "DEBUG", 
			AdjustLogLevel.Info => "INFO", 
			AdjustLogLevel.Warn => "WARN", 
			AdjustLogLevel.Error => "ERROR", 
			AdjustLogLevel.Assert => "ASSERT", 
			AdjustLogLevel.Suppress => "SUPPRESS", 
			_ => "unknown", 
		};
	}
}
