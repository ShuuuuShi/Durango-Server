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

	private GameObject _bottomDeleteButton;

	private PlayerInfo _selectedPlayer;

	private Action<PlayerInfo> _deleteClicked;

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
		PositionBottomDeleteButton();
	}

	public void Show(Account account, string serverName, int availableSlotCount, int maxSlotCount, [NotNull] Action<string, int> startWithExistingId, [NotNull] Action<int> startPrlogue, Action<PlayerInfo> deleteClicked)
	{
		base.gameObject.SetActive(value: true);
		_selectedPlayer = null;
		_deleteClicked = deleteClicked;
		_confirmButton.Clicked = null;
		_selectedWidgetMarker.gameObject.SetActive(value: false);
		if (_bottomDeleteButton != null)
		{
			_bottomDeleteButton.SetActive(value: false);
		}
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
				CreateNewCharacterButton(_scroll.Nodes, i, startPrlogue, wantClicked2);
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
			_selectedPlayer = selectedPlayer;
			EnsureBottomDeleteButton(comp, deleteClicked);
			_confirmButton.Clicked = delegate
			{
				startWithExistingId(selectedPlayer.PlayerEntityId, idx);
				base.gameObject.SetActive(value: false);
			};
			_confirmButton.Text = T._("캐릭터 선택");
			_selectedWidgetMarker.Set(comp);
		}, delegate(PlayerInfo doubleClickedPlayer)
		{
			if (doubleClickedPlayer != null && doubleClickedPlayer.PlayerEntityId == player.PlayerEntityId)
			{
				startWithExistingId(player.PlayerEntityId, idx);
				base.gameObject.SetActive(value: false);
			}
		});
		if (wantClicked)
		{
			comp.Clicked();
		}
	}

	private void EnsureBottomDeleteButton(TitlePlayerSelectionNode source, Action<PlayerInfo> deleteClicked)
	{
		if (deleteClicked == null || source == null)
		{
			return;
		}
		if (_bottomDeleteButton == null)
		{
			_bottomDeleteButton = source.CreateDeleteButton(_confirmButton.transform.parent);
			if (_bottomDeleteButton != null)
			{
				UIEventListener.Get(_bottomDeleteButton).onClick = delegate
				{
					if (_selectedPlayer != null && _deleteClicked != null)
					{
						_deleteClicked(_selectedPlayer);
					}
				};
			}
		}
		_deleteClicked = deleteClicked;
		if (_bottomDeleteButton != null)
		{
			_bottomDeleteButton.SetActive(value: true);
			PositionBottomDeleteButton();
		}
	}

	private void PositionBottomDeleteButton()
	{
		if (_bottomDeleteButton == null || _confirmButton == null || _confirmButton.Widget == null)
		{
			return;
		}
		UIWidget widget = _bottomDeleteButton.GetComponent<UIWidget>();
		if (widget == null)
		{
			return;
		}
		Vector3 localPosition = _confirmButton.transform.localPosition;
		if (TitleUIRootResizer.IsPortrait)
		{
			localPosition.y -= (_confirmButton.Widget.height + widget.height) * 0.5f + 24f;
		}
		else
		{
			localPosition.x += (_confirmButton.Widget.width + widget.width) * 0.5f + 24f;
		}
		_bottomDeleteButton.transform.localPosition = localPosition;
	}

	private void CreateNewCharacterButton(ListObjectPool nodes, int idx, [NotNull] Action<int> startPrlogue, bool wantClicked)
	{
		GameObject next = nodes.GetNext();
		TitlePlayerSelectionNode comp = next.GetComponent<TitlePlayerSelectionNode>();
		comp.Set(null, delegate
		{
			_selectedPlayer = null;
			if (_bottomDeleteButton != null)
			{
				_bottomDeleteButton.SetActive(value: false);
			}
			_confirmButton.Clicked = delegate
			{
				int obj = idx;
				startPrlogue(obj);
				base.gameObject.SetActive(value: false);
			};
			_confirmButton.Text = T._("캐릭터 생성");
			_selectedWidgetMarker.Set(comp);
		}, delegate
		{
			startPrlogue(idx);
			base.gameObject.SetActive(value: false);
		});
		if (wantClicked)
		{
			comp.Clicked();
		}
	}

	private static void CreateLockedSlotButton(ListObjectPool nodes)
	{
		GameObject next = nodes.GetNext();
		TitlePlayerSelectionNode component = next.GetComponent<TitlePlayerSelectionNode>();
		component.SetLocked();
	}

	public void SetBackButtonEvent(Action func)
	{
		_backFunc = func;
		_backButton.Clicked = delegate
		{
			OnReceiveBackMessage(null);
		};
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
