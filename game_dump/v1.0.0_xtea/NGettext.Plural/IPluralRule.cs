namespace NGettext.Plural;

public interface IPluralRule
{
	int NumPlurals { get; }

	int Evaluate(long number);
}
