using System;
using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ChapterGroup : UIBase
{
	[SerializeField]
	private ChapterEffect[] _chapterEffects;

	[SerializeField]
	private QuestFinishedEffect _questFinishedEffect;

	private readonly TimeSequencePlayer _timeSequencePlayer = new TimeSequencePlayer();

	private void Start()
	{
		ChapterEffect[] chapterEffects = _chapterEffects;
		foreach (ChapterEffect player in chapterEffects)
		{
			_timeSequencePlayer.AddPlayer(player);
		}
		_timeSequencePlayer.AddPlayer(_questFinishedEffect);
		SetChildrenActive(activated: false);
		base.VisibleController.Changed += OnVisibleChanged;
		GameSystem<PlayGuideSystem>.Instance().EventChanged += PlayGuideSystem_EventChanged;
		GameSystem<QuestSystem>.Instance().ChapterStarted += OnChapterStarted;
		SetVisible(visible: false, "Loading");
		UIManager.OnLoadingCurtainHidden(delegate
		{
			SetVisible(visible: true, "Loading");
		});
		NGUITools.SetLayer(base.gameObject, LayerHelper.UIOverLayer);
	}

	private void Update()
	{
		ProcessQueue();
	}

	private void PlayGuideSystem_EventChanged(GuideEvent prev, GuideEvent cur)
	{
		if (cur != null)
		{
			int size = KUtility.GetSize(cur.Chapter);
			if (size != 0)
			{
				Show(T._(cur.Chapter[0]), (size <= 1) ? string.Empty : T._(cur.Chapter[1]));
			}
		}
	}

	private void OnVisibleChanged(bool visible)
	{
		if (_timeSequencePlayer.IsPlaying())
		{
			_timeSequencePlayer.Stop();
		}
	}

	private void ProcessQueue()
	{
		if (base.Visible)
		{
			_timeSequencePlayer.Update();
		}
	}

	private void OnChapterStarted(string questId)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(questId);
		if (questYml != null)
		{
			Show(questYml.ChapterSubject, questYml.Description);
		}
	}

	public void Show(string title, string subtitle, int index = 0, Action finished = null)
	{
		index = Mathf.Clamp(index, 0, _chapterEffects.Length);
		_chapterEffects[index].Set(title, subtitle, finished);
	}

	public void Show(QuestRewardResults results)
	{
		_questFinishedEffect.Set(results, null);
	}
}
