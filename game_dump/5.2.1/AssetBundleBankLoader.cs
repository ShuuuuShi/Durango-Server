using System;

public class AssetBundleBankLoader : BankLoader
{
	private MemorySoundBank _bank;

	private Action _callback;

	public override bool IsLoaded
	{
		get
		{
			if (_bank != null)
			{
				return _bank.IsValid;
			}
			return false;
		}
	}

	public AssetBundleBankLoader(string bankPath)
		: base(bankPath)
	{
	}

	public override void AddCallback(Action callback)
	{
		if (callback == null)
		{
			return;
		}
		if (_bank != null)
		{
			if (_bank.IsValid)
			{
				callback();
			}
		}
		else
		{
			_callback = (Action)Delegate.Combine(_callback, callback);
		}
	}

	public override bool Load(byte[] binaryData)
	{
		_bank = new MemorySoundBank(binaryData);
		if (_bank.IsValid)
		{
			if (_callback != null)
			{
				_callback();
			}
			return true;
		}
		_callback = null;
		return false;
	}

	public override void Unload()
	{
		if (_bank != null)
		{
			_bank.Unload();
			_bank = null;
		}
	}
}
