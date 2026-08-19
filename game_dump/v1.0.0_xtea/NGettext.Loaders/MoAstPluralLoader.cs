using System.IO;
using NGettext.Plural;

namespace NGettext.Loaders;

public class MoAstPluralLoader : MoLoader
{
	public MoAstPluralLoader(string domain, string localeDir, MoFileParser parser)
		: base(domain, localeDir, new AstPluralRuleGenerator(), parser)
	{
	}

	public MoAstPluralLoader(string filePath, MoFileParser parser)
		: base(filePath, new AstPluralRuleGenerator(), parser)
	{
	}

	public MoAstPluralLoader(Stream moStream, MoFileParser parser)
		: base(moStream, new AstPluralRuleGenerator(), parser)
	{
	}

	public MoAstPluralLoader(string domain, string localeDir)
		: base(domain, localeDir, new AstPluralRuleGenerator())
	{
	}

	public MoAstPluralLoader(string filePath)
		: base(filePath, new AstPluralRuleGenerator())
	{
	}

	public MoAstPluralLoader(Stream moStream)
		: base(moStream, new AstPluralRuleGenerator())
	{
	}
}
