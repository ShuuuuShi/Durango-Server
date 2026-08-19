using System;
using Durango.Logic;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PvpIslandResultGroup : UIBase
{
	private const string HideKey = "PvpResult";

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _resultLabel;

	[SerializeField]
	private GameObject _highlight;

	[SerializeField]
	private SelectableButton _exitButton;

	private int _ranking;

	private int _killCount;

	private float _survivalTime;

	private CombatGroup _combatGroup;

	private void Start()
	{
		_combatGroup = UIManager.FindScript<CombatGroup>();
		_exitButton.Text = T._("나가기");
		SelectableButton exitButton = _exitButton;
		exitButton.Clicked = (Action)Delegate.Combine(exitButton.Clicked, new Action(ExitButtonClicked));
		GameSystem<PvpIslandSystem>.Instance().PlayerDied += PvpIslandSystem_PlayerDied;
		GameSystem<PvpIslandSystem>.Instance().Win += PvpIslandSystem_Win;
		GameSystem<PvpIslandSystem>.Instance().GameStarted += delegate
		{
			_combatGroup.BattleViewChanged += LockNormalMode;
			_combatGroup.SetBattleView(CombatGroup.BattleViewMode.Normal);
		};
		GameSystem<PvpIslandSystem>.Instance().BattleStarted += delegate
		{
			_combatGroup.BattleViewChanged -= LockNormalMode;
			_combatGroup.BattleViewChanged += LockBattleMode;
			_combatGroup.SetBattleView(CombatGroup.BattleViewMode.Battle);
			_combatGroup.BattleModeLock.Value = true;
		};
		TryClose();
	}

	private void LockNormalMode(CombatGroup.BattleViewMode mode)
	{
		if (mode != 0)
		{
			_combatGroup.SetBattleView(CombatGroup.BattleViewMode.Normal);
		}
	}

	private void LockBattleMode(CombatGroup.BattleViewMode mode)
	{
		if (mode != CombatGroup.BattleViewMode.Battle)
		{
			_combatGroup.SetBattleView(CombatGroup.BattleViewMode.Battle);
		}
	}

	private void ExitButtonClicked()
	{
		Connections.Frontend.Send(default(S02Leave)).On<Error>(delegate
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(_exitButton.gameObject);
			_exitButton.Disabled = false;
		});
		UIManager.Popup.LoadingRing.AttachToWidget(_exitButton.gameObject);
		_exitButton.Disabled = true;
		StopAllCoroutines();
	}

	private void ResetUI()
	{
		_combatGroup.BattleViewChanged -= LockBattleMode;
		_combatGroup.SetBattleView(CombatGroup.BattleViewMode.Normal);
		UIBase.CloseAllUI();
	}

	private void PvpIslandSystem_PlayerDied(S02PVPDead msg)
	{
		ResetUI();
		_ranking = msg.VictimRank;
		_killCount = msg.VictimKillCount;
		_survivalTime = msg.VictimSurvivedTime;
		string text = null;
		if (!string.IsNullOrEmpty(msg.KillerName))
		{
			string text2 = "<alert>" + msg.KillerName + "</alert>";
			string[] weaponTags = msg.WeaponTags;
			for (int i = 0; i < weaponTags.Length; i++)
			{
				switch (weaponTags[i])
				{
				case "bow":
					text = T._("\t{0}님의 활이 당신을 고이 모셨습니다.", text2);
					break;
				case "sword_onehand":
					text = T._("{0}님의 칼이 당신의 떠나는 길을 배웅했습니다.", text2);
					break;
				case "lance_twohand":
					text = T._("{0}님의 창이 당신의 숨결을 흘려 보냅니다.", text2);
					break;
				case "sword_twohand":
					text = T._("{0}님의 양손 칼이 당신의 떠나는 길을 배웅했습니다.", text2);
					break;
				case "blunt_twohand":
					text = T._("{0}님의 양손 망치가 당신의 최후를 맞이합니다.", text2);
					break;
				case "axe_twohand":
					text = T._("{0}님의 양손 도끼가 당신의 마지막 순간을 함께 했습니다.", text2);
					break;
				case "crossbow":
					text = T._("{0}님의 석궁이 당신을 고이 모셨습니다.", text2);
					break;
				case "blunt_onehand":
					text = T._("{0}님의 망치가 당신의 최후를 맞이합니다.", text2);
					break;
				case "axe_onehand":
					text = T._("{0}님의 도끼가 당신의 마지막 순간을 함께 했습니다.", text2);
					break;
				}
				if (text != null)
				{
					break;
				}
			}
		}
		if (text == null)
		{
			text = T._("사망했습니다.");
		}
		UIManager.SystemMsg(text);
		KUtility.DelayedCall(this, ShowResult, 6f);
	}

	private void PvpIslandSystem_Win(S02PVPFinish msg)
	{
		ResetUI();
		_ranking = 1;
		_killCount = msg.WinnerKillCount;
		_survivalTime = msg.WinnerSurvivedTime;
		ChapterGroup chapterGroup = UIManager.FindScript<ChapterGroup>();
		string subtitle = T._("{0}님이 모든 스파이를 처치하였습니다!", PlayerBehavior.LocalPlayer.PlayerName);
		chapterGroup.Show(T._("임무 종료"), subtitle, 2, ShowResult);
	}

	private void ShowResult()
	{
		bool flag = _ranking == 1;
		_exitButton.Disabled = false;
		_titleLabel.text = ((!flag) ? T._("임무 종료") : T._("난투 승리"));
		_highlight.SetActive(flag);
		string text = string.Format("[FFFFFFA0]{0}[-]<br>8</br>[size=64]{1}[/size][/c]<br>12</br>[preset=separator?line_s02]<br>2</br><kv>key=[FFFFFFA0]{2}[-],value=[c]{3}</kv><br>4</br><kv>key=[FFFFFFA0]{4}[-],value=[c][icon=icon_skill_time] {5}", T._("순위"), T._("{0}위", _ranking), T._("처치한 스파이"), _killCount, T._("생존시간"), TimedeltaFormatter.Format(_survivalTime));
		_resultLabel.text = text;
		Open();
		base.VisibleController.HideExceptForMe(hide: true, "PvpResult");
	}

	[ExposedInEditor(null)]
	private void TestResult()
	{
		S02PVPFinish s02PVPFinish = default(S02PVPFinish);
		s02PVPFinish.WinnerKillCount = 20;
		s02PVPFinish.WinnerSurvivedTime = 97f;
		S02PVPFinish msg = s02PVPFinish;
		PvpIslandSystem_Win(msg);
	}
}
