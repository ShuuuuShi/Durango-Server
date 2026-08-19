using System;
using Durango.UI.Control;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class StoryChapterDetailNode : MonoBehaviour
{
	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private GameObject _play;

	[SerializeField]
	private UIWidget _circle;

	[SerializeField]
	private GameObject _current;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private ToDoProgressGauge _progress;

	[SerializeField]
	private GameObject _reward;

	[SerializeField]
	private UIWidget _rewardSp;

	[SerializeField]
	private UILabel _rewardCount;

	[SerializeField]
	private GameObject _rewardChecked;

	private string _questId;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_play);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(_questId);
			if (questYml != null)
			{
				if (KUtility.GetSize(questYml.QuestEndMessages) > 0)
				{
					DialogueGroupBase dialogueGroupBase = UIManager.FindScript<DialogueGroupBase>();
					dialogueGroupBase.AddQuestMessages(_questId, questYml.QuestEndMessages, addFront: true);
				}
				if (KUtility.GetSize(questYml.QuestStartMessages) > 0)
				{
					DialogueGroupBase dialogueGroupBase2 = UIManager.FindScript<DialogueGroupBase>();
					dialogueGroupBase2.AddQuestMessages(_questId, questYml.QuestStartMessages, addFront: true);
				}
			}
		});
	}

	public void Set(QuestToDo quest, bool isLocked)
	{
		_questId = quest.Id;
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(_questId);
		if (isLocked || questYml == null)
		{
			_title.text = "?";
			_title.color = PresetColor.UIWhite;
			_description.gameObject.SetActive(value: false);
		}
		else
		{
			_title.text = ((!quest.Finished) ? ((string)questYml.Subject) : $"[s]{questYml.Subject}[/s]");
			_title.color = ((!quest.Finished) ? PresetColor.UIYellow : PresetColor.UIDarkGray);
			_description.gameObject.SetActive(!quest.Finished);
			_description.text = questYml.Description;
		}
		if (quest.Reward.HasValue && quest.Reward.Value.SkillPoints.HasValue)
		{
			_reward.gameObject.SetActive(value: true);
			_rewardCount.text = quest.Reward.Value.SkillPoints.ToString();
			_rewardChecked.SetActive(quest.Finished);
			_rewardSp.alpha = ((!quest.Finished) ? 1f : 0.2f);
		}
		else
		{
			_reward.gameObject.SetActive(value: false);
		}
		if (isLocked || quest.GoalCount <= 1 || quest.Finished)
		{
			_progress.gameObject.SetActive(value: false);
		}
		else
		{
			_progress.gameObject.SetActive(value: true);
			_progress.Set(quest.Progress, quest.GoalCount);
		}
		bool flag = !isLocked && !quest.Finished;
		_current.SetActive(flag);
		bool flag2 = questYml != null && (KUtility.GetSize(questYml.QuestStartMessages) > 0 || KUtility.GetSize(questYml.QuestEndMessages) > 0);
		bool flag3 = !isLocked && quest.Finished && flag2;
		_play.SetActive(flag3);
		_circle.gameObject.SetActive(!flag3 && !flag);
		_circle.color = ((!isLocked) ? PresetColor.UIYellow : PresetColor.UIDarkGray);
		_layout.UpdateLayout();
	}
}
