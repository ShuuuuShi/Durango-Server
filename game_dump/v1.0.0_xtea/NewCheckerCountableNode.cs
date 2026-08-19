using System;
using System.Collections.Generic;

public class NewCheckerCountableNode : NewChecker
{
	private int _count;

	public override string Key
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override List<NewChecker> Childs => null;

	public override int Count
	{
		get
		{
			return base.Enable ? _count : 0;
		}
		set
		{
			if (_count != value)
			{
				_count = value;
				ExecuteOnChange();
			}
		}
	}

	public override bool IsNew
	{
		get
		{
			return Count > 0;
		}
		set
		{
		}
	}

	public override void AddChild(NewChecker obj)
	{
		throw new NotSupportedException();
	}
}
