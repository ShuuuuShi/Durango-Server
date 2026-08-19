using System;
using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitlePlayerSelectionGroupBase : MonoBehaviour
{
	[SerializeField]
	protected SelectableButton _confirmButton;

	[SerializeField]
	private KGridScrollView _scroll;

	[SerializeField]
	private UILabel _serverLabel;

	[SerializeField]
	private UILabel _playerCountLabel;

	[SerializeField]
	private UIWidget _userCountHolder;

	[SerializeField]
	private TweenAlpha _slotExceededAlram;

	[SerializeField]
	private UISprite _playerCountExceededIcon;

	[SerializeField]
	private SelectionMarker _selectedWidgetMarker;

	[SerializeField]
	private SelectableWidget _backButton;

	private Action _backFunc;

	private void Awake()
	{
		_selectedWidgetMarker.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		TitleUIRootResizer.AddOnScreenResized(OnScreenResized);
		_slotExceededAlram.Sample(0f, isFinished: true);
		GameSystem<InputSystem>.Instance().On(InputCommand.SelectCurrentCell, OnReceiveSelectCurrentCellMessage);
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, OnReceiveBackMessage);
	}

	private void OnDestroy()
	{
		GameSystem<InputSystem>.Instance().Off(InputCommand.SelectCurrentCell, OnReceiveSelectCurrentCellMessage);
		GameSystem<InputSystem>.Instance().Off(InputCommand.Back, OnReceiveBackMessage);
	}

	protected virtual void OnScreenResized()
	{
		_scroll.UpdateLayout();
	}

	public void Show(Account account, string serverName, int availableSlotCount, int maxSlotCount, [NotNull] Action<string, int> startWithExistingId, [NotNull] Action<int> startPrlogue, Action<PlayerInfo> deleteClicked)
	{
		base.gameObject.SetActive(value: true);
		Pair<string, int> recommendedPlayer = account.GetRecommendedPlayer();
		List<PlayerInfo> list = ((account.Players == null) ? new List<PlayerInfo>() : account.Players);
		_serverLabel.text = serverName;
		bool flag = list.Count > availableSlotCount;
		_playerCountExceededIcon.gameObject.SetActive(flag);
		_playerCountLabel.text = string.Format("{0}/{1}", flag ? list.Count.ToString().ToEncodedColor(NGUIText.ParseColor("BA2E2DFF")) : list.Count.ToString(), availableSlotCount.ToString().ToEncodedColor(new Color(1f, 1f, 1f, 0.5f)));
		if (flag)
		{
			UIEventListener.Get(_userCountHolder.gameObject).onClick = delegate
			{
				_slotExceededAlram.tweenFactor = 0f;
				_slotExceededAlram.PlayForward();
			};
		}
		else
		{
			UIEventListener.Get(_userCountHolder.gameObject).onClick = null;
		}
		_scroll.Nodes.BeginLoad();
		bool flag2 = false;
		int i = 0;
		for (int num = Mathf.Max(list.Count, availableSlotCount, maxSlotCount); i < num; i++)
		{
			if (i < list.Count)
			{
				bool wantClicked = list[i].PlayerEntityId == recommendedPlayer.Item1;
				CreateExistingCharacterButton(_scroll.Nodes, list[i], i, startWithExistingId, deleteClicked, wantClicked);
			}
			else if (i < availableSlotCount)
			{
				bool wantClicked2 = !flag2 && string.IsNullOrEmpty(recommendedPlayer.Item1);
				flag2 = true;
				CreateNewCharacterButton(_scroll.Nodes, list.Count, startPrlogue, wantClicked2);
			}
			else
			{
				CreateLockedSlotButton(_scroll.Nodes);
			}
		}
		_scroll.Nodes.EndLoad();
		_scroll.UpdateLayout();
	}

	private void CreateExistingCharacterButton(ListObjectPool nodes, PlayerInfo player, int idx, [NotNull] Action<string, int> startWithExistingId, Action<PlayerInfo> deleteClicked, bool wantClicked)
	{
		GameObject next = nodes.GetNext();
		TitlePlayerSelectionNode comp = next.GetComponent<TitlePlayerSelectionNode>();
		comp.Set(player, delegate(PlayerInfo selectedPlayer)
		{
			_confirmButton.Clicked = delegate
			{
				startWithExistingId(selectedPlayer.PlayerEntityId, idx);
				base.gameObject.SetActive(value: false);
			};
			_confirmButton.Text = T._("캐릭터 선택");
			_selectedWidgetMarker.Set(comp);
		}, DoubleClickNode, deleteClicked);
		if (wantClicked)
		{
			comp.Clicked();
		}
	}

	private void CreateNewCharacterButton(ListObjectPool nodes, int idx, [NotNull] Action<int> startPrlogue, bool wantClicked)
	{
		GameObject next = nodes.GetNext();
		TitlePlayerSelectionNode comp = next.GetComponent<TitlePlayerSelectionNode>();
		comp.Set(null, delegate
		{
			_confirmButton.Clicked = delegate
			{
				int obj = idx;
				startPrlogue(obj);
				base.gameObject.SetActive(value: false);
			};
			_confirmButton.Text = T._("캐릭터 생성");
			_selectedWidgetMarker.Set(comp);
		}, DoubleClickNode);
		if (wantClicked)
		{
			comp.Clicked();
		}
	}

	private static void CreateLockedSlotButton(ListObjectPool nodes)
	{
		nodes.GetNext().GetComponent<TitlePlayerSelectionNode>().SetLocked();
	}

	public void SetBackButtonEvent(Action func)
	{
		_backFunc = func;
		_backButton.Clicked = delegate
		{
			OnReceiveBackMessage(null);
		};
	}

	public void DoubleClickNode(PlayerInfo playerInfo)
	{
		if (_confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}

	public void OnReceiveBackMessage(InputCommandMessage message)
	{
		base.gameObject.SetActive(value: false);
		if (_backFunc != null)
		{
			_backFunc();
		}
	}

	public void OnReceiveSelectCurrentCellMessage(InputCommandMessage message)
	{
		if (_confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}
}
