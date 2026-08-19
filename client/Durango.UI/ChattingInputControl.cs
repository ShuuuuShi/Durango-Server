using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ChattingInputControl : MonoBehaviour, IUIInitializable
{
	private class ChattingAction
	{
		public string Name;

		public Func<bool> Validator;

		public Action Do;
	}

	public Action<string> Submitted;

	[SerializeField]
	private SelectableButton _sendButton;

	[SerializeField]
	private SelectableButton _chatTagButton;

	[SerializeField]
	private UIInput _inputLabel;

	[SerializeField]
	private UISprite _bg;

	private BoxCollider _inputLabelCollider;

	private readonly Observable<bool> _isConnected = new Observable<bool>();

	private readonly Observable<bool> _isEnabled = new Observable<bool>();

	private StringSelector _stringSelector;

	private List<ChattingAction> _chattingActions;

	private readonly List<ChattingAction> _currentChattingActions = new List<ChattingAction>();

	void IUIInitializable.Init()
	{
		_chattingActions = new List<ChattingAction>
		{
			new ChattingAction
			{
				Name = T._("현재 내 위치 공유"),
				Do = delegate
				{
					WorldMapGroup.ShareCurrentPos();
				}
			},
			new ChattingAction
			{
				Name = T._("공유할 위치 선택"),
				Do = delegate
				{
					UIManager.FindScript<WorldMapGroup>().OpenForSharePos();
				}
			},
			new ChattingAction
			{
				Name = T._("부족 초대"),
				Do = delegate
				{
					PlayerBehavior localPlayer2 = PlayerBehavior.LocalPlayer;
					if (localPlayer2 != null && localPlayer2.HasClan)
					{
						Member clan = localPlayer2.Clan;
						GameSystem<SocialSystem>.Instance().SystemSay(new RadioLink
						{
							Text = T._("{0} 부족에 초대합니다.", clan.ClanName),
							Link = $"icon=icon_guild, color=82A7F2, link=ui://Clan/Join/{clan.ClanId}"
						});
					}
				},
				Validator = delegate
				{
					PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
					return localPlayer != null && localPlayer.HasClan;
				}
			},
			new ChattingAction
			{
				Name = T._("악보 공유"),
				Do = delegate
				{
					Singleton<MusicManager>.Instance().GetMusics(delegate(List<KeyValuePair<MusicId, Music>> musics)
					{
						if (musics.Count == 0)
						{
							UIManager.SystemMsg(T._("선택할 수 있는 악보가 없습니다."));
						}
						else
						{
							GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
							genericSelector.ResetArguments();
							genericSelector.SetTitle(T._("공유할 악보를 선택해주세요."));
							foreach (KeyValuePair<MusicId, Music> music in musics)
							{
								genericSelector.AddItem(music.Value.Name);
							}
							KeyValuePair<MusicId, Music>[] array = musics.ToArray();
							genericSelector.SetSelected(delegate(int index)
							{
								if (index >= 0 && index < array.Length)
								{
									KeyValuePair<MusicId, Music> selected = array[index];
									MusicEditorGroup.GetOrMakeSharedMusicSheetId(selected.Key, delegate(string sharedMusicSheetId)
									{
										if (!string.IsNullOrEmpty(sharedMusicSheetId))
										{
											RadioLink radioLink = MusicEditorGroup.MakeMusicExport(sharedMusicSheetId, selected.Value.Name);
											GameSystem<SocialSystem>.Instance().SystemSay(radioLink);
										}
									});
								}
							});
							genericSelector.Show();
						}
					});
				}
			}
		};
		EventDelegate.Add(_inputLabel.onSubmit, OnSubmit);
		_sendButton.Clicked = OnSubmit;
		_chatTagButton.Clicked = ShowChattingActions;
		_inputLabelCollider = _inputLabel.GetComponent<BoxCollider>();
		Observable<bool> isConnected = _isConnected;
		isConnected.Changed = (Action<bool>)Delegate.Combine(isConnected.Changed, (Action<bool>)delegate
		{
			RefreshState();
		});
		Observable<bool> isEnabled = _isEnabled;
		isEnabled.Changed = (Action<bool>)Delegate.Combine(isEnabled.Changed, (Action<bool>)delegate
		{
			RefreshState();
		});
		if (GameManager.ClusterMode != 0)
		{
			_chatTagButton.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		_inputLabel.RemoveFocus();
	}

	private void OnSelectChattingAction(int index)
	{
		ChattingAction chattingAction = _currentChattingActions.Get(index);
		if (chattingAction != null && chattingAction.Do != null)
		{
			chattingAction.Do();
		}
	}

	private void RefreshCurrentChattingActions()
	{
		_currentChattingActions.Clear();
		foreach (ChattingAction chattingAction in _chattingActions)
		{
			if (chattingAction.Validator == null || chattingAction.Validator())
			{
				_currentChattingActions.Add(chattingAction);
			}
		}
	}

	private void ShowChattingActions()
	{
		if (_stringSelector == null)
		{
			RefreshCurrentChattingActions();
			StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
			stringSelector.Set(_currentChattingActions.Select((ChattingAction item) => item.Name), OnSelectChattingAction);
			stringSelector.MinWidth = 300;
			stringSelector.DragLock = true;
			stringSelector.AutoPosition = false;
			stringSelector.HideIgnoreParent = _chatTagButton.transform;
			stringSelector.AddOnFinished(HideChattingActions);
			stringSelector.Show();
			stringSelector.SetPosition(_chatTagButton.Widget, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));
			_stringSelector = stringSelector;
			_chatTagButton.Selected = true;
		}
		else
		{
			HideChattingActions();
		}
	}

	private void HideChattingActions()
	{
		if (_stringSelector != null)
		{
			_stringSelector.Hide();
			_stringSelector = null;
		}
		_chatTagButton.Selected = false;
	}

	private void OnSubmit()
	{
		if (Submitted != null)
		{
			Submitted(_inputLabel.value);
		}
		_inputLabel.value = string.Empty;
	}

	public void FocusInputText(bool hasFocus)
	{
		_inputLabel.isSelected = hasFocus;
	}

	public void SetConnected(bool isConnected)
	{
		_isConnected.Value = isConnected;
	}

	public void SetEnabled(bool isEnabled)
	{
		_isEnabled.Value = isEnabled;
	}

	private void RefreshState()
	{
		Color color;
		bool flag;
		string text;
		bool flag2;
		if (!_isEnabled)
		{
			color = new Color(0.1f, 0.1f, 0.1f, _bg.color.a);
			flag = false;
			text = T._("전송");
			flag2 = false;
		}
		else if (!_isConnected)
		{
			color = new Color(0.2f, 0f, 0f, _bg.color.a);
			flag = false;
			text = T._("재접속");
			_inputLabel.RemoveFocus();
			flag2 = false;
		}
		else
		{
			flag = true;
			text = T._("전송");
			color = new Color(0f, 0f, 0f, _bg.color.a);
			flag2 = true;
		}
		_bg.color = color;
		_sendButton.Text = text;
		_sendButton.Disabled = !flag;
		_chatTagButton.Disabled = !flag;
		_chatTagButton.Selected = false;
		HideChattingActions();
		if (flag2)
		{
			_inputLabelCollider.enabled = true;
			return;
		}
		_inputLabelCollider.enabled = false;
		_inputLabel.RemoveFocus();
	}
}
