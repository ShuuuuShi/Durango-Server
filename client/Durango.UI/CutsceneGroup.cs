using System;
using Durango.Cutscene;
using UnityEngine;

namespace Durango.UI;

public class CutsceneGroup : UIBase
{
	[EnumList(typeof(Durango.Cutscene.Type), false, 0, -1)]
	[SerializeField]
	private CutsceneUIBase[] _cutsceneUIList;

	public CutsceneUIBase CurrentCutsceneUI { get; private set; }

	private void Start()
	{
		SetChildrenActive(activated: false);
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	protected override bool TryOpen()
	{
		return true;
	}

	public override bool Close()
	{
		return false;
	}

	public void Open(Durango.Cutscene.Type cutsceneType, Action callback)
	{
		CurrentCutsceneUI = _cutsceneUIList[(int)cutsceneType];
		if (CurrentCutsceneUI == null)
		{
			if (callback != null)
			{
				callback();
			}
			return;
		}
		CurrentCutsceneUI.Open(delegate
		{
			base.Open();
			if (callback != null)
			{
				callback();
			}
		});
	}

	public void Close(Action callback)
	{
		if (CurrentCutsceneUI == null)
		{
			base.Close();
			if (callback != null)
			{
				callback();
			}
			return;
		}
		CurrentCutsceneUI.Close(delegate
		{
			base.Close();
			if (callback != null)
			{
				callback();
			}
		});
		CurrentCutsceneUI = null;
	}
}
