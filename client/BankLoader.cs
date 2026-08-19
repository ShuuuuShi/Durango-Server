using System;

public abstract class BankLoader
{
	public string BankPath { get; private set; }

	public abstract bool IsLoaded { get; }

	public BankLoader(string bankPath)
	{
		BankPath = bankPath;
	}

	public abstract void AddCallback(Action callback);

	public virtual bool Load(byte[] binaryData)
	{
		return true;
	}

	public virtual void Unload()
	{
	}
}
