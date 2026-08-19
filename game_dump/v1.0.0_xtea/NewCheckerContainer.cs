using System;
using System.Collections.Generic;

public class NewCheckerContainer : NewChecker
{
	private List<NewChecker> _childs;

	public override string Key
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override List<NewChecker> Childs
	{
		get
		{
			if (_childs == null)
			{
				_childs = new List<NewChecker>();
			}
			return _childs;
		}
	}

	public override int Count
	{
		get
		{
			if (base.Enable)
			{
				int num = 0;
				int i = 0;
				for (int count = Childs.Count; i < count; i++)
				{
					num += Childs[i].Count;
				}
				return num;
			}
			return 0;
		}
		set
		{
			throw new NotSupportedException();
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
			throw new NotSupportedException();
		}
	}

	public override void AddChild(NewChecker obj)
	{
		if (!Childs.Contains(obj))
		{
			Childs.Add(obj);
		}
		if (!obj.Parents.Contains(this))
		{
			obj.Parents.Add(this);
		}
		if (obj.IsNew)
		{
			ExecuteOnChange();
		}
	}
}
