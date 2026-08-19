using System.Collections.Generic;

public abstract class NewChecker
{
	public List<EventDelegate> OnChangeList = new List<EventDelegate>();

	private List<NewChecker> _parents;

	private bool _enable = true;

	public abstract string Key { get; set; }

	public abstract List<NewChecker> Childs { get; }

	public abstract int Count { get; set; }

	public abstract bool IsNew { get; set; }

	public List<NewChecker> Parents
	{
		get
		{
			if (_parents == null)
			{
				_parents = new List<NewChecker>();
			}
			return _parents;
		}
	}

	public bool Enable
	{
		get
		{
			return _enable;
		}
		set
		{
			if (_enable != value)
			{
				_enable = value;
				ExecuteOnChange();
			}
		}
	}

	public bool OnChangeLock { get; set; }

	public abstract void AddChild(NewChecker obj);

	public void RegisterCallback(EventDelegate.Callback callback)
	{
		EventDelegate.Add(OnChangeList, callback);
	}

	public void ClearCallback()
	{
		OnChangeList.Clear();
		int i = 0;
		for (int count = Parents.Count; i < count; i++)
		{
			Parents[i].ClearCallback();
		}
	}

	public void ExecuteOnChange()
	{
		if (!OnChangeLock)
		{
			EventDelegate.Execute(OnChangeList);
			int i = 0;
			for (int count = Parents.Count; i < count; i++)
			{
				Parents[i].ExecuteOnChange();
			}
		}
	}

	public void AddChild(INewCheckerable obj)
	{
		AddChild(obj.NewChecker);
	}

	public void ClearChild()
	{
		if (Childs != null)
		{
			int i = 0;
			for (int count = Childs.Count; i < count; i++)
			{
				Childs[i].Parents.Remove(this);
			}
			Childs.Clear();
		}
	}
}
