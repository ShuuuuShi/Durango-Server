using System.Text.RegularExpressions;

namespace SharpRaven.Logging.Filters;

public class PhoneNumberFilter : IFilter
{
	public string Filter(string input)
	{
		Regex regex = new Regex("1?\\W*([2-9][0-8][0-9])\\W*([2-9][0-9]{2})\\W*([0-9]{4})(\\se?x?t?(\\d*))?");
		regex.Replace(input, (Match m) => "##-PHONE-TRUNC-##");
		return input;
	}
}
