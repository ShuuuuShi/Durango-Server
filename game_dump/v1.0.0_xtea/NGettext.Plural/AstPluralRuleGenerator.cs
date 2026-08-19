using System;
using System.Globalization;
using System.Text.RegularExpressions;
using NGettext.Plural.Ast;

namespace NGettext.Plural;

public class AstPluralRuleGenerator : DefaultPluralRuleGenerator, IPluralRuleGenerator, IPluralRuleTextParser
{
	private static readonly Regex NPluralsRegex = new Regex("(nplurals=(?<nplurals>\\d+))", RegexOptions.IgnoreCase);

	private static readonly Regex PluralRegex = new Regex("(plural=(?<plural>[^;\\n]+))", RegexOptions.IgnoreCase);

	protected string PluralRuleText { get; private set; }

	public AstTokenParser Parser { get; protected set; }

	public AstPluralRuleGenerator()
		: this(new AstTokenParser())
	{
	}

	public AstPluralRuleGenerator(AstTokenParser parser)
	{
		Parser = parser;
	}

	public AstPluralRuleGenerator(string pluralRuleText)
		: this()
	{
		SetPluralRuleText(pluralRuleText);
	}

	public AstPluralRuleGenerator(string pluralRuleText, AstTokenParser parser)
		: this(parser)
	{
		SetPluralRuleText(pluralRuleText);
	}

	public void SetPluralRuleText(string pluralRuleText)
	{
		PluralRuleText = pluralRuleText;
	}

	public override IPluralRule CreateRule(CultureInfo cultureInfo)
	{
		if (PluralRuleText != null)
		{
			int numPlurals = ParseNumPlurals(PluralRuleText);
			string input = ParsePluralFormulaText(PluralRuleText);
			Token astRoot = Parser.Parse(input);
			return new AstPluralRule(numPlurals, astRoot);
		}
		return base.CreateRule(cultureInfo);
	}

	public int ParseNumPlurals(string input)
	{
		Match match = NPluralsRegex.Match(input);
		if (!match.Success)
		{
			throw new FormatException("Failed to parse 'nplurals' parameter from the plural rule text: invalid format");
		}
		return int.Parse(match.Groups["nplurals"].Value);
	}

	public string ParsePluralFormulaText(string input)
	{
		Match match = PluralRegex.Match(input);
		if (!match.Success)
		{
			throw new FormatException("Failed to parse 'plural' parameter from the plural rule text: invalid format");
		}
		return match.Groups["plural"].Value;
	}
}
