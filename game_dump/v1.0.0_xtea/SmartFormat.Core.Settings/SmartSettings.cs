using System;
using System.Collections.Generic;
using System.Reflection;

namespace SmartFormat.Core.Settings;

public class SmartSettings
{
	public CaseSensitivityType CaseSensitivity { get; set; }

	internal SmartSettings()
	{
		CaseSensitivity = CaseSensitivityType.CaseSensitive;
	}

	internal IEqualityComparer<string> GetCaseSensitivityComparer()
	{
		return CaseSensitivity switch
		{
			CaseSensitivityType.CaseSensitive => StringComparer.CurrentCulture, 
			CaseSensitivityType.CaseInsensitive => StringComparer.CurrentCultureIgnoreCase, 
			_ => throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown."), 
		};
	}

	internal StringComparison GetCaseSensitivityComparison()
	{
		return CaseSensitivity switch
		{
			CaseSensitivityType.CaseSensitive => StringComparison.CurrentCulture, 
			CaseSensitivityType.CaseInsensitive => StringComparison.CurrentCultureIgnoreCase, 
			_ => throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown."), 
		};
	}

	internal BindingFlags GetCaseSensitivityBindingFlag()
	{
		return CaseSensitivity switch
		{
			CaseSensitivityType.CaseSensitive => BindingFlags.Default, 
			CaseSensitivityType.CaseInsensitive => BindingFlags.IgnoreCase, 
			_ => throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown."), 
		};
	}
}
