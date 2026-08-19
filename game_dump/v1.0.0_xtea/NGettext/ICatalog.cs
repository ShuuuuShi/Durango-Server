namespace NGettext;

public interface ICatalog
{
	string GetString(string text);

	string GetString(string text, params object[] args);

	string GetPluralString(string text, string pluralText, long n);

	string GetPluralString(string text, string pluralText, long n, params object[] args);

	string GetParticularString(string context, string text);

	string GetParticularString(string context, string text, params object[] args);

	string GetParticularPluralString(string context, string text, string pluralText, long n);

	string GetParticularPluralString(string context, string text, string pluralText, long n, params object[] args);
}
