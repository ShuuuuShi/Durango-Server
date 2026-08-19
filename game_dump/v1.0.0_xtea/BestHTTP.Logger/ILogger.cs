using System;

namespace BestHTTP.Logger;

public interface ILogger
{
	Loglevels Level { get; set; }

	string FormatVerbose { get; set; }

	string FormatInfo { get; set; }

	string FormatWarn { get; set; }

	string FormatErr { get; set; }

	string FormatEx { get; set; }

	void Verbose(string division, string verb);

	void Information(string division, string info);

	void Warning(string division, string warn);

	void Error(string division, string err);

	void Exception(string division, string msg, Exception ex);
}
