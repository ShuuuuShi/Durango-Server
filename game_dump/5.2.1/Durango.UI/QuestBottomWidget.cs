using System.Collections.Generic;
using System.Linq;
using Durango.Development;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Quest;
using UnityEngine;

namespace Durango.UI;

public class QuestBottomWidget : MonoBehaviour, IUIInitializable
{
	public const float TweenTime = 1f;

	private const int MaxShowRewardCount = 5;

	private const int MinShowRewardCount = 2;

	[SerializeField]
	private UIWidget _contents;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private Transform _moveTransform;

	[SerializeField]
	private AnimationWidget _loadingIcon;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UILabel _totalScoreLabel;

	[SerializeField]
	private UIProgressBar _progress;

	[SerializeField]
	private QuestBottomRewardWidget _rewardBase;

	private readonly QuestScoreReward _zeroScoreReward = new QuestScoreReward
	{
		QuestScore = 0,
		State = QuestScoreRewardState.Invalid
	};

	private ListObjectPool<QuestBottomRewardWidget> _rewardItemPool = new ListObjectPool<QuestBottomRewardWidget>();

	private readonly List<QuestScoreReward> _scoreRewards = new List<QuestScoreReward>();

	private QuestScoreInfos _scoreInfos;

	private bool _interactionLock;

	private int _currentlyReceivedIndex;

	private QuestGroup _parent;

	public void Init()
	{
		_parent = UIUtility.FindComponentInParent<QuestGroup>(base.gameObject);
		_rewardItemPool.BaseObject = _rewardBase;
		_rewardItemPool.UseBase = false;
		_rewardItemPool.Init(delegate(QuestBottomRewardWidget w)
		{
			w.QuestRewardRequested = QuestRewardRequested;
		});
		_rewardItemPool.Clear();
	}

	private void Start()
	{
		_progress.value = 0f;
	}

	public void BeginLoading()
	{
		_scoreRewards.Clear();
		_loadingIcon.gameObject.SetActive(value: true);
		_loadingIcon.Widget.alpha = 0f;
		_loadingIcon.Delay = 0.2f;
		_loadingIcon.Alpha = 1f;
		_contents.alpha = 0f;
	}

	private void EndLoading()
	{
		_loadingIcon.Delay = 0f;
		_loadingIcon.Alpha = 0f;
		TweenAlpha.Begin(_contents.gameObject, 0.2f, 1f);
		UIManager.Popup.LoadingRing.Hide();
	}

	public void UpdateScoreInfo(QuestScoreInfos questScoreInfos)
	{
		EndLoading();
		bool flag = _scoreRewards.Count == 0;
		QuestScoreInfos scoreInfos = _scoreInfos;
		RefreshScoreRewardData(questScoreInfos);
		SetTotalScoreLabel(questScoreInfos.CurQuestScore);
		if (flag)
		{
			_currentlyReceivedIndex = -1;
		}
		else
		{
			UpdateCheckCurrentlyReceived(scoreInfos);
		}
		PlayScrollAnim(flag);
	}

	private void RefreshScoreRewardData(QuestScoreInfos questScoreInfos)
	{
		_scoreInfos = questScoreInfos;
		_scoreRewards.Clear();
		_scoreRewards.Add(_zeroScoreReward);
		_scoreRewards.AddRange(questScoreInfos.QuestScoreRewards);
	}

	private void UpdateCheckCurrentlyReceived(QuestScoreInfos oldScoreInfos)
	{
		if (oldScoreInfos.QuestScoreRewards == null)
		{
			_currentlyReceivedIndex = -1;
			return;
		}
		QuestScoreReward[] questScoreRewards = oldScoreInfos.QuestScoreRewards;
		for (int i = 0; i < questScoreRewards.Length; i++)
		{
			QuestScoreReward questScoreReward = questScoreRewards[i];
			int num = 0;
			QuestScoreReward[] questScoreRewards2 = _scoreInfos.QuestScoreRewards;
			for (int j = 0; j < questScoreRewards2.Length; j++)
			{
				QuestScoreReward reward = questScoreRewards2[j];
				if (questScoreReward.QuestScore == reward.QuestScore && questScoreReward.State == QuestScoreRewardState.Available && reward.State == QuestScoreRewardState.Taken)
				{
					_currentlyReceivedIndex = num;
					ShowAlarm(reward);
					return;
				}
				num++;
			}
		}
		_currentlyReceivedIndex = -1;
	}

	public void PlayScrollAnim(bool immediate = false)
	{
		GetItemList(out var firstIndex, out var outLastIndex);
		int questScore = _scoreRewards[firstIndex].QuestScore;
		int questScore2 = _scoreRewards[outLastIndex].QuestScore;
		_rewardItemPool.Set(_scoreRewards.Count);
		float num = _progress.backgroundWidget.width;
		float num2 = questScore2 - questScore;
		float progress = GetRatio(questScore, questScore2, _scoreInfos.CurQuestScore);
		float num3 = ((!(num2 > 0f)) ? 0f : (num / num2));
		List<Vector3> poses = new List<Vector3>();
		Vector3 localPosition = _rewardBase.transform.localPosition;
		localPosition.x = _progress.transform.localPosition.x;
		for (int i = 0; i < _rewardItemPool.Count; i++)
		{
			QuestBottomRewardWidget questBottomRewardWidget = _rewardItemPool[i];
			if (i < firstIndex || i > outLastIndex)
			{
				questBottomRewardWidget.Widget.alpha = 0f;
			}
			else
			{
				questBottomRewardWidget.Widget.alpha = 1f;
				bool flag = firstIndex == i;
				questBottomRewardWidget.SetData(_parent.SelectedCategory, _scoreRewards[i], i == _scoreRewards.Count - 1, flag && _currentlyReceivedIndex != i - 1, immediate);
				questBottomRewardWidget.PlayAnim(flag, immediate);
			}
			if (i > 0)
			{
				localPosition.x += (float)(_scoreRewards[i].QuestScore - _scoreRewards[i - 1].QuestScore) * num3;
			}
			poses.Add(localPosition);
		}
		TweenTick.Begin(base.gameObject, (_currentlyReceivedIndex == -1) ? 0.0001f : 1f, delegate(float factor, bool isFinished)
		{
			if (isFinished)
			{
				for (int j = 0; j < poses.Count; j++)
				{
					Vector3 vector = poses[j];
					if (immediate)
					{
						_rewardItemPool[j].transform.localPosition = vector;
					}
					else
					{
						TweenPosition tweenPosition = TweenPosition.Begin(_rewardItemPool[j].gameObject, 1f, vector);
						tweenPosition.method = UITweener.Method.EaseInOut;
						tweenPosition.SetOnFinished(UnlockInteraction);
						tweenPosition.PlayForward();
					}
				}
				Vector3 localPosition2 = _scrollView.transform.localPosition;
				localPosition2.x = 0f - poses[firstIndex].x + _progress.transform.localPosition.x - 3f;
				if (immediate)
				{
					_moveTransform.localPosition = localPosition2;
					_progress.value = progress;
					UnlockInteraction();
				}
				else
				{
					TweenPosition tweenPosition2 = TweenPosition.Begin(_moveTransform.gameObject, 1f, localPosition2);
					tweenPosition2.method = UITweener.Method.EaseInOut;
					tweenPosition2.PlayForward();
					PlayProgressAnim(progress);
				}
			}
		});
	}

	private void PlayProgressAnim(float value)
	{
		float currentValue = _progress.value;
		TweenTick tweenTick = TweenTick.Begin(_progress.gameObject, 1f, delegate(float factor, bool isFinished)
		{
			_progress.value = Mathf.Lerp(currentValue, value, factor);
		});
		tweenTick.method = UITweener.Method.EaseInOut;
		tweenTick.PlayForward();
	}

	private void GetItemList(out int outFirstIndex, out int outLastIndex)
	{
		int num = -1;
		for (int i = 0; i < _scoreRewards.Count; i++)
		{
			if (_scoreRewards[i].State != QuestScoreRewardState.Taken && _scoreRewards[i].State != QuestScoreRewardState.Invalid)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			outFirstIndex = Mathf.Max(_scoreRewards.Count - 2, _scoreRewards.Count - 1);
			outLastIndex = _scoreRewards.Count - 1;
			return;
		}
		int questScore = _scoreRewards[num - 1].QuestScore;
		int questScore2 = _scoreRewards[num].QuestScore;
		int num2 = Mathf.Min(num + 5 - 2, _scoreRewards.Count - 1);
		float availableItemRatioInProgress = GetAvailableItemRatioInProgress();
		while (num2 > num + 2 - 2)
		{
			int questScore3 = _scoreRewards[num2].QuestScore;
			if (GetRatio(questScore, questScore3, questScore2) > availableItemRatioInProgress)
			{
				break;
			}
			num2--;
		}
		outFirstIndex = num - 1;
		outLastIndex = num2;
	}

	private float GetAvailableItemRatioInProgress()
	{
		return GetRatio(0f, _progress.backgroundWidget.width, _descriptionLabel.printedSize.x) + 0.15f;
	}

	private float GetRatio(float start, float end, float value)
	{
		if (start >= end)
		{
			return 1f;
		}
		return 1f - (end - value) / (end - start);
	}

	private void SetTotalScoreLabel(int score)
	{
		_totalScoreLabel.text = $"[icon=crown:0.8] {score}";
	}

	private void QuestRewardRequested(GameObject rewardWidget, string category, int score)
	{
		if (!_interactionLock)
		{
			LockInteraction();
			UIManager.Popup.LoadingRing.AttachToWidget(rewardWidget);
			GameSystem<QuestSystem>.Instance().RequestQuestScoreReward(category, score);
		}
	}

	private void LockInteraction()
	{
		_interactionLock = true;
	}

	private void UnlockInteraction()
	{
		_interactionLock = false;
	}

	private void ShowAlarm(QuestScoreReward reward)
	{
		AlarmGroup alarmGroup = UIManager.FindScript<AlarmGroup>();
		if (!(alarmGroup == null))
		{
			AlarmRewardQueue.Args args = default(AlarmRewardQueue.Args);
			List<ReceiveRewardsPopup.ItemArgument> list = new List<ReceiveRewardsPopup.ItemArgument>();
			ReceiveRewardsPopup.AddRewardedItems(list, reward.Reward, isBonus: false);
			if (list.Count != 0)
			{
				ReceiveRewardsPopup.ItemArgument itemArgument = list.First();
				ItemIcon itemIcon = default(ItemIcon);
				itemIcon.Main = itemArgument.Icon;
				ItemIcon icon = itemIcon;
				icon.Colors = ((!itemArgument.IconColor.HasValue) ? ItemIconTex.MakeFromTableKey(itemArgument.IconRTable, itemArgument.IconGTable, itemArgument.IconBTable) : itemArgument.IconColor);
				args.Icon = icon;
				args.Main = itemArgument.Title + " " + itemArgument.GetSubText();
				args.Sub = T._("보상 획득");
				alarmGroup.RewardAlarm(args, AlarmGroup.RewardEffectType.QuestScoreReward);
			}
		}
	}

	private void OnClick()
	{
		if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftControl))
		{
			string cheat = "aqs " + _scoreInfos.Category + " 10";
			Singleton<Commands>.Instance().Cheat(cheat);
			Connections.Frontend.Send(new GetQuestScoreInfos
			{
				Category = _scoreInfos.Category
			}).On(delegate(QuestScoreInfos msg, PacketHeader header)
			{
				UpdateScoreInfo(msg);
			});
		}
	}
}
