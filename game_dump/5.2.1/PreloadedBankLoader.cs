using System;

public class PreloadedBankLoader : BankLoader
{
	public override bool IsLoaded => true;

	public PreloadedBankLoader(string bankPath)
		: base(bankPath)
	{
	}

	public override void AddCallback(Action callback)
	{
		callback?.Invoke();
	}
}
