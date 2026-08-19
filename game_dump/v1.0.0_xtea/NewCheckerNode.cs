using System;
using System.Collections.Generic;

public class NewCheckerNode : NewChecker
{
	private string _key;

	private bool _isNew;

	public override string Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	public override bool IsNew
	{
		get
		{
			return base.Enable && _isNew;
		}
		set
		{
			if (_isNew != value)
			{
				_isNew = value;
				NewCheckUtil.Save(this);
				ExecuteOnChange();
			}
		}
	}

	public override int Count
	{
		get
		{
			return IsNew ? 1 : 0;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override List<NewChecker> Childs => null;

	public override void AddChild(NewChecker obj)
	{
		throw new NotSupportedException();
	}
}
