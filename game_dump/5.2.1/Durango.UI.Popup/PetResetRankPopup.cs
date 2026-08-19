using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class PetResetRankPopup : TooltipBase
{
	private enum Page
	{
		Ready,
		Result
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _helpLabel;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private PetResetRankInfoWidget _rankWidgetBase;

	[SerializeField]
	private UISprite _lineSprite;

	private ListObjectPool<PetResetRankInfoWidget> _rankWidgets;

	private Messages.Pet _pet;

	private RevertPetRankCandidate? _candidate;

	private Action _onConfirmed;

	private bool _isWait;

	private int _onHideFrame;

	private bool _hideConfirm;

	private Page _page;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_rankWidgets = new ListObjectPool<PetResetRankInfoWidget>();
		_rankWidgets.BaseObject = _rankWidgetBase;
		_rankWidgets.Clear();
		_titleLabel.text = T._("등급 초기화");
		_helpLabel.text = T._("등급 초기화시 속성 발견으로 얻은 속성과 길들일 때 받았던 <em>보너스 속성</em>이 <em>초기화</em>됩니다.");
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(Hide));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
	}

	private void OnConfirm()
	{
		if (_isWait)
		{
			return;
		}
		RevertPetRankCandidate? candidate2 = _candidate;
		if (!candidate2.HasValue)
		{
			_isWait = true;
			UIManager.MessageBox.Show(T._("정말 진행하시겠습니까?"), T._("<alert>[icon=icon_make_alert] 재접속, 전투, 상점 등으로 화면이 전환되면 등급 초기화가 완료되지 않을 수 있습니다. 안전한 장소에서 진행하시는 것을 추천드립니다.</alert>"), delegate(bool ok)
			{
				if (!ok)
				{
					_isWait = false;
				}
				else
				{
					PetManager.RevertPetRank(_pet.EntityId, delegate(RevertPetRankCandidate? candidate)
					{
						_isWait = false;
						_candidate = candidate;
						MarkAsChanged();
					});
				}
			});
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.AddKeyValueInfo(T._("새로운 등급"), "[size=36]" + ((_candidate.Value.Rank != PetRank.S) ? _candidate.Value.Rank.ToString() : ("<em>" + _candidate.Value.Rank.ToString() + "</em>")) + "[/size]");
		messageBox.Show(T._("<em>새로운 등급</em>으로 결정하시겠습니까?"), T._("[icon=icon_make_alert] 확정 후에는 이전 등급으로 돌아갈 수 없습니다."), delegate(bool ok)
		{
			if (ok)
			{
				_isWait = true;
				PetManager.AcceptPetRank(_pet.EntityId, delegate(bool success)
				{
					_isWait = false;
					if (success)
					{
						if (_onConfirmed != null)
						{
							_onConfirmed();
						}
						_hideConfirm = true;
						Hide();
					}
				});
			}
		}, T._("등급 확정"));
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelButton;
	}

	public void Set(Messages.Pet pet, Action onConfirm)
	{
		_pet = pet;
		_onConfirmed = onConfirm;
	}

	public override void Hide()
	{
		RevertPetRankCandidate? candidate = _candidate;
		if (candidate.HasValue && _onHideFrame != Time.frameCount && !_hideConfirm)
		{
			_onHideFrame = Time.frameCount;
			PetRank rank = _pet.Rank;
			MessageBox messageBox = UIManager.MessageBox;
			messageBox.AddKeyValueInfo(T._("기존 등급"), "[size=36]" + ((_candidate.Value.Rank != PetRank.S) ? rank.ToString() : ("<em>" + rank.ToString() + "</em>")) + "[/size]");
			messageBox.Show(T._("등급 변경을 <em>취소</em>하고 기존 등급을 유지하시겠습니까?"), T._("[icon=icon_make_alert] 새로 받은 등급 정보는 사라지며 등급 초기화 권이 소비됩니다."), delegate(bool ok)
			{
				if (ok)
				{
					_hideConfirm = true;
					Hide();
				}
			}, T._("기존 등급 유지"));
		}
		else
		{
			base.Hide();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		_isWait = false;
		_hideConfirm = false;
		_onHideFrame = 0;
	}

	protected override void OnHide()
	{
		base.OnHide();
		_candidate = null;
		_onConfirmed = null;
	}

	private void ShowReadyPage()
	{
		_page = Page.Ready;
		_lineSprite.gameObject.SetActive(value: false);
		Vector3 localPosition = _rankWidgetBase.transform.localPosition;
		localPosition.x = 0f;
		_rankWidgets[0].transform.localPosition = localPosition;
	}

	private void ShowResultPage(bool instant)
	{
		_page = Page.Result;
		Vector3 localPosition = _rankWidgetBase.transform.localPosition;
		localPosition.x = 0f;
		Vector3 pos;
		Vector3 pos2 = (pos = localPosition);
		UIWidget component = _rankWidgetBase.transform.parent.GetComponent<UIWidget>();
		pos2.x -= (float)component.width * 0.25f;
		pos.x += (float)component.width * 0.25f;
		_lineSprite.gameObject.SetActive(value: true);
		if (instant)
		{
			_rankWidgets[0].SetPosition(pos2, useTween: false);
			_rankWidgets[1].SetPosition(pos, useTween: false);
			_rankWidgets[1].SetAlpha(1f, useTween: false);
			_lineSprite.alpha = 1f;
			_lineSprite.SetEnable<TweenAlpha>(enable: false);
			return;
		}
		if (_candidate.HasValue && _candidate.Value.Rank == PetRank.S)
		{
			SoundManager.PlayEvent("ui_revert_srank");
		}
		else
		{
			SoundManager.PlayEvent("ui_revert_rank");
		}
		_rankWidgets[0].Duration = 0.3f;
		_rankWidgets[0].transform.localPosition = localPosition;
		_rankWidgets[0].SetPosition(pos2);
		_rankWidgets[1].SetPosition(pos, useTween: false);
		_rankWidgets[1].PlayEffect(0.5f);
		_lineSprite.alpha = 0f;
		TweenAlpha.Begin(_lineSprite.gameObject, 0.2f, 1f).delay = 0.3f;
	}

	protected override void FillData()
	{
		_rankWidgets.BeginLoad();
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		List<string> value = reusable.Value;
		if (_pet.Stat.Tags != null)
		{
			MilestoneInfo[] milestonesInformation = _pet.Statistics.MilestonesInformation;
			foreach (KeyValuePair<string, int> tag in _pet.Stat.Tags)
			{
				bool flag = false;
				if (milestonesInformation != null)
				{
					for (int i = 0; i < milestonesInformation.Length; i++)
					{
						if (milestonesInformation[i].TagId == tag.Key)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					value.Add(tag.Key);
				}
			}
		}
		_rankWidgets.GetNext().Set(T._("현재 등급"), _pet.Rank, value, effectOn: false);
		RevertPetRankCandidate? candidate = _candidate;
		if (!candidate.HasValue)
		{
			Yaml.Cost petRevertRank = Yaml.Util.Singleton<CostsYaml>.Instance.PetRevertRank;
			_cancelButton.Text = T._("취소");
			_confirmButton.Text = string.Format("{0} {1}", T._("등급 초기화"), petRevertRank.CostToEmphasisString(InventorySystem.Wallet));
		}
		else
		{
			_cancelButton.Text = T._("취소");
			_confirmButton.Text = T._("등급 확정");
			RevertPetRankCandidate value2 = _candidate.Value;
			_rankWidgets.GetNext().Set(string.Format("<em>{0}</em>", T._("새로운 등급")), value2.Rank, string.IsNullOrEmpty(value2.Tag) ? null : new string[1] { value2.Tag }, effectOn: true);
		}
		_rankWidgets.EndLoad();
	}

	protected override void UpdateLayout()
	{
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		RevertPetRankCandidate? candidate = _candidate;
		if (!candidate.HasValue)
		{
			ShowReadyPage();
		}
		else
		{
			ShowResultPage(_page == Page.Result);
		}
	}
}
