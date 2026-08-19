using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.Logic.PlayGuide;

public abstract class ToDoBase
{
	public class ToDoJson
	{
		public string type;

		public string name;

		public string message;

		public string tooltip;

		public string tag;

		public Dictionary<string, Dictionary<string, string>> filters;

		public string id;

		public int index;

		public int level;

		public float ratio;

		public int count;

		public float time;

		public int time_begin;

		public int time_end;

		public Vector2 pos;

		public float radius;
	}

	public string Key { get; set; }

	public int TargetProgress { get; set; }

	public int CurrentProgress { get; set; }

	public bool IsCompleted { get; set; }

	public string LocalText { get; set; }

	public string Tooltip { get; set; }

	public virtual bool IsVisibleProgress
	{
		get
		{
			if (!IsCompleted)
			{
				return TargetProgress > 1;
			}
			return false;
		}
	}

	public event Action Completed;

	public virtual void OnAddItem()
	{
	}

	public virtual void OnRemoveItem()
	{
	}

	public virtual void Process()
	{
	}

	public virtual bool OnClicked()
	{
		return false;
	}

	public void CallComplete()
	{
		if (!IsCompleted)
		{
			IsCompleted = true;
			GameSystem<ToDoListSystem>.Instance().SetUpdated(this);
			if (this.Completed != null)
			{
				this.Completed();
			}
		}
	}

	protected void CallProgressChange(int progress)
	{
		if (IsCompleted)
		{
			return;
		}
		int num = 1;
		if (TargetProgress > 0)
		{
			progress = Mathf.Min(progress, TargetProgress);
			if (progress != CurrentProgress)
			{
				CurrentProgress = progress;
				GameSystem<ToDoListSystem>.Instance().SetUpdated(this);
			}
			num = TargetProgress;
		}
		if (progress >= num)
		{
			CallComplete();
		}
	}
}
