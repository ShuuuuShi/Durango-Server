using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SharpRaven.Data;

public class SentryStacktrace
{
	[JsonProperty(PropertyName = "frames")]
	public List<ExceptionFrame> Frames;

	public SentryStacktrace(Exception e)
	{
		StackTrace stackTrace = new StackTrace(e, fNeedFileInfo: true);
		Frames = (stackTrace.GetFrames() ?? new StackFrame[0]).Reverse().Select(delegate(StackFrame frame)
		{
			int num = frame.GetFileLineNumber();
			if (num == 0)
			{
				num = frame.GetILOffset();
			}
			MethodBase method = frame.GetMethod();
			return new ExceptionFrame
			{
				Filename = frame.GetFileName(),
				Module = (((object)method.DeclaringType == null) ? null : method.DeclaringType.FullName),
				Function = method.Name,
				Source = method.ToString(),
				LineNumber = num,
				ColumnNumber = frame.GetFileColumnNumber()
			};
		}).ToList();
	}
}
