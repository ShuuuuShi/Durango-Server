using System.Globalization;

namespace NGettext.Plural;

public interface IPluralRuleGenerator
{
	IPluralRule CreateRule(CultureInfo cultureInfo);
}
