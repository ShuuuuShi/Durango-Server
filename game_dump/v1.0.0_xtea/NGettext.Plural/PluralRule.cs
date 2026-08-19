using System;

namespace NGettext.Plural;

public class PluralRule : IPluralRule
{
	protected PluralRuleEvaluatorDelegate EvaluatorDelegate;

	public static readonly PluralRule Default = new PluralRule(2, (long number) => (number != 1) ? 1 : 0);

	public int NumPlurals { get; protected set; }

	public PluralRule(int numPlurals, PluralRuleEvaluatorDelegate evaluatorDelegate)
	{
		if (numPlurals <= 0)
		{
			throw new ArgumentOutOfRangeException("numPlurals");
		}
		if (evaluatorDelegate == null)
		{
			throw new ArgumentNullException("evaluatorDelegate");
		}
		NumPlurals = numPlurals;
		EvaluatorDelegate = evaluatorDelegate;
	}

	public virtual int Evaluate(long number)
	{
		return EvaluatorDelegate(number);
	}
}
