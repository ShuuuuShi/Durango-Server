using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayGuide;

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

		public bool ignore_already_owned;

		public string id;

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

	public bool FromAutoGuide { get; set; }

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

	protected string[] Split(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new string[0];
		}
		string[] array = text.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}
}
