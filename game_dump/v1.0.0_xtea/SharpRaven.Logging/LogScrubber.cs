using System.Collections.Generic;
using SharpRaven.Logging.Filters;

namespace SharpRaven.Logging;

public class LogScrubber : IScrubber
{
	public List<IFilter> Filters { get; private set; }

	public LogScrubber()
	{
		Filters = new List<IFilter>();
		Filters.AddRange(new IFilter[3]
		{
			new CreditCardFilter(),
			new PhoneNumberFilter(),
			new SocialSecurityFilter()
		});
	}

	public string Scrub(string input)
	{
		foreach (IFilter filter in Filters)
		{
			input = filter.Filter(input);
		}
		return input;
	}
}
