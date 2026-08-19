using System;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

public class CustomPluralRuleProvider : IFormatProvider
{
	private readonly PluralRules.PluralRuleDelegate pluralRule;

	public CustomPluralRuleProvider(PluralRules.PluralRuleDelegate pluralRule)
	{
		this.pluralRule = pluralRule;
	}

	public object GetFormat(Type formatType)
	{
		return ((object)formatType != typeof(CustomPluralRuleProvider)) ? null : this;
	}

	public PluralRules.PluralRuleDelegate GetPluralRule()
	{
		return pluralRule;
	}
}
