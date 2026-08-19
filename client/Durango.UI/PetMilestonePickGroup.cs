using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Item;
using Shared.Pet;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("PetGrowth")]
public class PetMilestonePickGroup : UIBase
{
	private enum RollState
	{
		Ready,
		Rolling,
		Result
	}

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private CurrencyWidgetBase _currencyWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIModelViewer _previewViewer;

	[SerializeField]
	private GameObject _rollPointSprite;

	[SerializeField]
	private GameObject _rollTouchedEffect;

	[SerializeField]
	private NestedPrefabLinker _rollWidgetLinker;

	[SerializeField]
	private PetMilestoneSelectedInfoWidget _infoWidget;

	[SerializeField]
	private GameObject _itemRewardedObject;

	[SerializeField]
	private RollDecorationSprite[] _rollDecorationSprites;

	[SerializeField]
	private UILabel _touchPlzLabel;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private PetMilestoneResultWidget _resultWidget;

	[SerializeField]
	private RectLayoutComponent _buttonsWidget;

	[SerializeField]
	private SelectableButton _okButton;

	[SerializeField]
	private SelectableButton _rerollButton;

	[SerializeField]
	private SelectableButton _exitButton;

	[SerializeField]
	private PetMilestoneCelebrationWidget _celebrationWidget;

	private PetMilestoneRollWidget _rollWidget;

	private RollState? _rollState;

	private Messages.Pet _pet;

	private int? _milestoneId;

	private Messages.PetActiveSkill? _petCurrentSkill;

	private Action _picked;

	private bool _isChangedPet;

	private int _onHideFrame;

	private bool _hideConfirm;

	private bool _forceSetResult;

	private string _selectedTagId = string.Empty;

	private uint _sequence;

	private MilestoneResult? _lastMilestoneResult;

	private DrawSkillResult? _lastDrawSkillResult;

	private void Awake()
	{
		_rollWidget = _rollWidgetLinker.Object.GetComponent<PetMilestoneRollWidget>();
		_confirmButton.Text = T._("돌리기");
		_okButton.Text = T._("속성 확정");
		_exitButton.Text = T._("나가기");
		SelectableButton okButton = _okButton;
		okButton.Clicked = (Action)Delegate.Combine(okButton.Clicked, (Action)delegate
		{
			UIManager.MessageBox.Show(T._("정말 확정하시겠습니까?"), T._("<alert>[icon=icon_make_alert] 확정 후에는 속성 변경이 불가능합니다.</alert>"), delegate(bool ok)
			{
				if (ok)
				{
					_hideConfirm = true;
					_isChangedPet = true;
					PetManager.AcceptMilestone(_pet.EntityId, delegate
					{
						Close();
					});
				}
			});
		});
		SelectableButton rerollButton = _rerollButton;
		rerollButton.Clicked = (Action)Delegate.Combine(rerollButton.Clicked, new Action(OnReroll));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
		SelectableButton exitButton = _exitButton;
		exitButton.Clicked = (Action)Delegate.Combine(exitButton.Clicked, (Action)delegate
		{
			Close();
		});
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		base.OnOpenSucceed += OnOpened;
		base.OnCloseSucceed += OnClosed;
		Observable<float> rollSpeed = _rollWidget.RollSpeed;
		rollSpeed.Changed = (Action<float>)Delegate.Combine(rollSpeed.Changed, (Action<float>)delegate(float speed)
		{
			SetDecorationSpriteRotateSpeed(10f + speed);
		});
		_rollWidget.TagFocused += delegate(string tagId)
		{
			_infoWidget.Set(tagId);
		};
		_rollWidget.SkillFocused += delegate(Messages.PetActiveSkill skill)
		{
			_infoWidget.Set(skill);
		};
		_rollWidget.EmptyFocused += delegate
		{
			_infoWidget.SetEmpty();
		};
		_rollWidget.Unfocused += delegate
		{
			MilestoneResult? lastMilestoneResult2 = _lastMilestoneResult;
			if (lastMilestoneResult2.HasValue)
			{
				_infoWidget.Set(_lastMilestoneResult.Value.SelectedTagId);
			}
			else
			{
				DrawSkillResult? lastDrawSkillResult2 = _lastDrawSkillResult;
				if (lastDrawSkillResult2.HasValue)
				{
					_infoWidget.Set(_lastDrawSkillResult.Value.Skill);
				}
				else
				{
					_infoWidget.SetUnknown();
				}
			}
		};
		_rollWidget.MilestoneRollFinished += OnMilestoneRollFinish;
		_rollWidget.DrawSkillRollFinished += delegate(DrawSkillResult result)
		{
			SetResult(result);
		};
		_rollWidget.RollFailFinished += delegate
		{
			_hideConfirm = true;
			Close();
		};
		_rollWidget.RollAnimationFinished += delegate
		{
			DrawSkillResult? lastDrawSkillResult = _lastDrawSkillResult;
			if (!lastDrawSkillResult.HasValue)
			{
				MilestoneResult? lastMilestoneResult = _lastMilestoneResult;
				if (!lastMilestoneResult.HasValue)
				{
					return;
				}
			}
			Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
			if (petCurrentSkill.HasValue)
			{
				RequestRedrawActiveSkill();
			}
			else
			{
				RequestPickMilestoneAgain();
			}
			_forceSetResult = false;
		};
		_currencyWidget.HideExtraButton(hide: true);
		SetChildrenActive(activated: false);
	}

	private void Start()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnDestroy()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnTouch(GameObject obj, bool press)
	{
		if (base.IsOpened && _rollWidget.StopRoll())
		{
			_touchPlzLabel.text = T._("<weak>선택 중..</weak>");
			_rollTouchedEffect.gameObject.SetActive(value: true);
			SoundManager.PlayEvent((!_milestoneId.HasValue) ? "ui_animal_specialability_stop" : "ui_animal_ability_stop");
		}
	}

	protected override bool TryClose()
	{
		if (_isChangedPet && _onHideFrame != Time.frameCount && !_hideConfirm)
		{
			_onHideFrame = Time.frameCount;
			Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
			if (!petCurrentSkill.HasValue)
			{
				MessageBox messageBox = UIManager.MessageBox;
				messageBox.Show(T._("<em>속성을 확정하지 않고</em> 나가시겠습니까?"), string.Format("{0}\n{1}", T._("<alert_icon/> 속성을 확정하지 않으면 다음 속성을 얻을 수 없습니다."), T._("<alert_icon/> 속성을 확정하기 전까지는 계속해서 속성을 변경할 수 있습니다.")), delegate(bool ok)
				{
					if (ok)
					{
						_hideConfirm = true;
						Close();
					}
				});
			}
			else
			{
				_hideConfirm = true;
				Close();
			}
			return false;
		}
		return base.TryClose();
	}

	private void OnOpened()
	{
		_onHideFrame = 0;
		_hideConfirm = false;
		_forceSetResult = false;
		_petCurrentSkill = null;
		_rollTouchedEffect.gameObject.SetActive(value: false);
	}

	private void OnClosed()
	{
		_rollState = null;
		_celebrationWidget.gameObject.SetActive(value: false);
		_lastMilestoneResult = null;
		_lastDrawSkillResult = null;
		_petCurrentSkill = null;
		_selectedTagId = string.Empty;
		StopAllCoroutines();
		if (_isChangedPet && _picked != null)
		{
			_picked();
			_picked = null;
		}
	}

	private void SetMilestoneTitle(Messages.Pet pet)
	{
		int? milestoneId = _milestoneId;
		if (!milestoneId.HasValue || pet.Statistics.MilestonesInformation == null)
		{
			return;
		}
		int num = -1;
		MilestoneInfo[] milestonesInformation = pet.Statistics.MilestonesInformation;
		int i = 0;
		for (int size = KUtility.GetSize(milestonesInformation); i < size; i++)
		{
			if (milestonesInformation[i].MilestoneTableId == _milestoneId.Value)
			{
				num = i;
				break;
			}
		}
		string text = ((_pet.Stat.LastMilestoneAccepted && !(_selectedTagId != string.Empty)) ? T._("발견") : T._("변경"));
		_titleLabel.text = ((num + 1 >= KUtility.GetSize(milestonesInformation)) ? T._("<em>{0}</em>의 마지막 속성 {1}!", pet.GetPetName(), text) : T._("<em>{0}</em>의 <em>{1}번째</em> 속성 {2}!", pet.GetPetName(), num + 1, text));
	}

	private void SetActiveSkillTitle()
	{
		Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
		string text = ((!petCurrentSkill.HasValue) ? T._("발견") : T._("변경"));
		_titleLabel.text = T._("<em>{0}</em>의 특수 행동 {1}!", _pet.GetPetName(), text);
	}

	public void ShowPetMilestonePick(Messages.Pet pet, int milestoneId, Action picked)
	{
		Open();
		_pet = pet;
		_milestoneId = milestoneId;
		_picked = picked;
		_isChangedPet = false;
		_currencyWidget.SetCurrencyType(Yaml.Util.Singleton<CostsYaml>.Instance.PetRevertMilestone.Currency);
		bool flag = _pet.Stat.LastMilestoneAccepted || !PetUtil.HasAcquiredMilestone(pet);
		string title = ((!flag) ? T._("현재 적용 중인 속성") : T._("새로운 속성"));
		_infoWidget.SetTitle(title);
		SetMilestoneTitle(pet);
		if (flag)
		{
			SetState(RollState.Ready);
		}
		else
		{
			MilestoneInfo? currentPetMilestoneInfo = PetUtil.GetCurrentPetMilestoneInfo(_pet);
			Money? retryCost = _pet.Stat.RetryCost;
			if (retryCost.HasValue && currentPetMilestoneInfo.HasValue)
			{
				MilestoneResult result = default(MilestoneResult);
				result.RetryCost = _pet.Stat.RetryCost.Value;
				result.SelectedTagId = currentPetMilestoneInfo.Value.TagId;
				_forceSetResult = true;
				SetResult(result, effect: false);
			}
		}
		SetPreviewModel(pet);
		uint seq = ++_sequence;
		PetManager.GetMilestoneCandidate(pet.EntityId, milestoneId, delegate(MilestoneCandidates? candidates)
		{
			if (candidates.HasValue && seq == _sequence)
			{
				_rollWidget.Show(candidates.Value);
			}
		});
	}

	public void ShowPetActiveSkillPick(Messages.Pet pet, Action picked)
	{
		Open();
		_pet = pet;
		_milestoneId = null;
		_picked = picked;
		_isChangedPet = false;
		_currencyWidget.SetCurrencyType(Yaml.Util.Singleton<CostsYaml>.Instance.PetRevertActiveSkill.Currency);
		if (PetUtil.HasPetActiveSkill(_pet))
		{
			_petCurrentSkill = _pet.Statistics.AvailableActiveSkill[0];
		}
		Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
		string title = ((!petCurrentSkill.HasValue) ? T._("특수 행동") : T._("현재 적용 중인 특수 행동"));
		_infoWidget.SetTitle(title);
		SetActiveSkillTitle();
		Messages.PetActiveSkill? petCurrentSkill2 = _petCurrentSkill;
		if (!petCurrentSkill2.HasValue)
		{
			SetState(RollState.Ready);
		}
		else
		{
			Money? retryCost = _pet.Stat.RetryCost;
			if (retryCost.HasValue)
			{
				DrawSkillResult result = default(DrawSkillResult);
				result.Skill = _petCurrentSkill.Value;
				result.RetryCost = _pet.Stat.RetryCost.Value;
				_forceSetResult = true;
				SetResult(result, effect: false);
			}
		}
		SetPreviewModel(pet);
		List<Pair<Messages.PetActiveSkill, float>> activeSkillCandidates = PetUtil.GetActiveSkillCandidates(pet);
		_rollWidget.Show(activeSkillCandidates);
	}

	private void SetState(RollState state)
	{
		if (_rollState.HasValue && _rollState.Value == state)
		{
			return;
		}
		_rollState = state;
		_infoWidget.SetUnknown();
		_infoWidget.GetComponent<TweenerPlayer>().ResetToLast();
		_celebrationWidget.gameObject.SetActive(value: false);
		_itemRewardedObject.gameObject.SetActive(value: false);
		bool active = false;
		bool active2 = false;
		bool active3 = false;
		bool active4 = false;
		bool active5 = false;
		bool active6 = false;
		bool active7 = false;
		switch (state)
		{
		case RollState.Ready:
		{
			active = true;
			active2 = true;
			active5 = true;
			MilestoneInfo? currentPetMilestoneInfo = PetUtil.GetCurrentPetMilestoneInfo(_pet);
			if (currentPetMilestoneInfo.HasValue && !PetUtil.PetReadyToActiveSkill(_pet))
			{
				_infoWidget.Set(currentPetMilestoneInfo.Value.TagId);
			}
			else
			{
				Messages.PetActiveSkill? petCurrentSkill2 = _petCurrentSkill;
				if (petCurrentSkill2.HasValue)
				{
					_infoWidget.Set(_petCurrentSkill.Value);
				}
			}
			_rollWidget.GetComponent<UIWidget>().alpha = 0f;
			break;
		}
		case RollState.Rolling:
		{
			SoundManager.PlayEvent((!_milestoneId.HasValue) ? "ui_animal_specialability_start" : "ui_animal_ability_start");
			_touchPlzLabel.text = ((!Platform.Instance.UsePCUI) ? T._("화면을 터치하세요!") : T._("화면을 클릭하세요!"));
			active = true;
			active4 = true;
			Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
			if (petCurrentSkill.HasValue || PetUtil.PetReadyToDrawActiveSkill(_pet))
			{
				_infoWidget.SetTitle(T._("특수 행동"));
				SetActiveSkillTitle();
			}
			else
			{
				SetMilestoneTitle(_pet);
			}
			break;
		}
		case RollState.Result:
			active3 = !_forceSetResult;
			active6 = true;
			active7 = true;
			active = false;
			_rerollButton.Disabled = false;
			_rollWidget.GetComponent<UIWidget>().alpha = 0f;
			break;
		}
		_rollWidget.gameObject.SetActive(active);
		_rollPointSprite.gameObject.SetActive(active);
		_confirmButton.gameObject.SetActive(active2);
		_resultWidget.gameObject.SetActive(active3);
		_touchPlzLabel.gameObject.SetActive(active4);
		_closeButton.gameObject.SetActive(active5);
		_buttonsWidget.gameObject.SetActive(active6);
		_exitButton.gameObject.SetActive(active6);
		_currencyWidget.gameObject.SetActive(active7);
		GameObject obj = _okButton.gameObject;
		Messages.PetActiveSkill? petCurrentSkill3 = _petCurrentSkill;
		obj.SetActive(!petCurrentSkill3.HasValue);
	}

	private void SetPreviewModel(Messages.Pet pet)
	{
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		Animal animal = ((pet2 != null) ? SingletonDict<int, Animal>.Get(pet2.VehicleEntityType) : null);
		if (animal == null)
		{
			_previewViewer.gameObject.SetActive(value: false);
			return;
		}
		string prefabPath = animal.PrefabPath;
		bool flag = pet.Stat.Life == null || pet.Stat.Life.Ratio() <= 0f;
		bool isOld = pet.Stat.IsOld;
		Action<GameObject> action = null;
		if (pet2.IsRidable)
		{
			action = (Action<GameObject>)Delegate.Combine(action, _previewViewer.SetupSaddle());
		}
		Action<GameObject> a = action;
		Action<GameObject> b;
		if (flag)
		{
			b = _previewViewer.DefaultDeadAnimalPlay(isOld);
		}
		else
		{
			UIModelViewer previewViewer = _previewViewer;
			bool isOld2 = isOld;
			b = previewViewer.DefaultAnimalPlay("stand", isOld2);
		}
		action = (Action<GameObject>)Delegate.Combine(a, b);
		_previewViewer.SetPlainModel(prefabPath, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = action
		});
	}

	private void SetDecorationSpriteRotateSpeed(float speed)
	{
		for (int i = 0; i < _rollDecorationSprites.Length; i++)
		{
			_rollDecorationSprites[i].SetRotateSpeed(speed * (float)((i % 2 == 0) ? 1 : (-1)));
		}
	}

	private void OnConfirm()
	{
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.Show(T._("정말 진행하시겠습니까?"), T._("<alert>[icon=icon_make_alert] 재접속, 전투, 상점 등으로 화면을 전환하면 의도와 다른 속성 및 특수 행동으로 결정될 수 있습니다. 안전한 장소에서 진행하는 것을 추천합니다.</alert>"), delegate(bool ok)
		{
			if (ok)
			{
				Confirm();
			}
		});
	}

	private void Confirm()
	{
		SetState(RollState.Rolling);
		if (_milestoneId.HasValue)
		{
			_infoWidget.SetTitle(T._("새로운 속성"));
			MilestoneInfo? currentPetMilestoneInfo = PetUtil.GetCurrentPetMilestoneInfo(_pet);
			if (!currentPetMilestoneInfo.HasValue)
			{
				return;
			}
			if (!currentPetMilestoneInfo.Value.Acquired)
			{
				_rollWidget.PlayRoll(delegate(Action<object> onResult)
				{
					_isChangedPet = true;
					PetManager.PickMilestone(_pet.EntityId, delegate(MilestoneResult? result)
					{
						if (result.HasValue)
						{
							onResult(result.Value);
						}
						else
						{
							onResult(null);
						}
					});
				});
			}
			else
			{
				OnReroll();
			}
			return;
		}
		Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
		if (!petCurrentSkill.HasValue)
		{
			_rollWidget.PlayRoll(delegate(Action<object> onResult)
			{
				_isChangedPet = true;
				PetManager.DrawActiveSkill(_pet.EntityId, delegate(DrawSkillResult? result)
				{
					if (result.HasValue)
					{
						_petCurrentSkill = result.Value.Skill;
						onResult(result.Value);
					}
					else
					{
						onResult(null);
					}
				});
			});
		}
		else
		{
			OnReroll();
		}
	}

	private void OnReroll()
	{
		_rerollButton.Disabled = true;
		if (_milestoneId.HasValue)
		{
			StartRollAnimation();
			return;
		}
		Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
		if (petCurrentSkill.HasValue)
		{
			StartRollAnimation();
		}
	}

	private void StartRollAnimation()
	{
		_rollWidget.gameObject.SetActive(value: true);
		_closeButton.gameObject.SetActive(value: false);
		_buttonsWidget.gameObject.SetActive(value: false);
		_exitButton.gameObject.SetActive(value: false);
		_rollWidget.StartRollAnimationCoroutine();
	}

	private void RequestRedrawActiveSkill()
	{
		Messages.PetActiveSkill? petCurrentSkill = _petCurrentSkill;
		if (!petCurrentSkill.HasValue)
		{
			return;
		}
		bool byVoucher = false;
		DrawSkillResult? lastDrawSkillResult = _lastDrawSkillResult;
		bool byCurrency;
		if (lastDrawSkillResult.HasValue)
		{
			Yaml.Cost revertActiveSkillCost = GetRevertActiveSkillCost(_lastDrawSkillResult.Value);
			revertActiveSkillCost.Payable(InventorySystem.Wallet, out byVoucher, out byCurrency);
		}
		else
		{
			Money? retryCost = _pet.Stat.RetryCost;
			if (retryCost.HasValue)
			{
				Yaml.Cost revertActiveSkillCost2 = GetRevertActiveSkillCost(_pet.Stat.RetryCost.Value);
				revertActiveSkillCost2.Payable(InventorySystem.Wallet, out byVoucher, out byCurrency);
			}
		}
		_isChangedPet = true;
		PetManager.RedrawActiveSkill(_pet.EntityId, _petCurrentSkill.Value, byVoucher, delegate(DrawSkillResult? result)
		{
			_rerollButton.Disabled = false;
			if (!result.HasValue)
			{
				_exitButton.gameObject.SetActive(value: true);
			}
			else
			{
				_petCurrentSkill = result.Value.Skill;
				SetState(RollState.Rolling);
				_rollWidget.PlayRoll(delegate(Action<object> onResult)
				{
					onResult(result.Value);
				});
			}
		});
	}

	private void RequestPickMilestoneAgain()
	{
		bool byVoucher = false;
		_isChangedPet = true;
		MilestoneResult? lastMilestoneResult = _lastMilestoneResult;
		bool byCurrency;
		if (lastMilestoneResult.HasValue)
		{
			Yaml.Cost revertMilestoneCost = GetRevertMilestoneCost(_lastMilestoneResult.Value);
			revertMilestoneCost.Payable(InventorySystem.Wallet, out byVoucher, out byCurrency);
		}
		else
		{
			Money? retryCost = _pet.Stat.RetryCost;
			if (retryCost.HasValue)
			{
				Yaml.Cost revertMilestoneCost2 = GetRevertMilestoneCost(_pet.Stat.RetryCost.Value);
				revertMilestoneCost2.Payable(InventorySystem.Wallet, out byVoucher, out byCurrency);
			}
		}
		PetManager.PickMilestoneAgain(_pet.EntityId, byVoucher, delegate(MilestoneResult? result)
		{
			_rerollButton.Disabled = false;
			if (!result.HasValue)
			{
				_exitButton.gameObject.SetActive(value: true);
			}
			else
			{
				SetState(RollState.Rolling);
				_rollWidget.PlayRoll(delegate(Action<object> onResult)
				{
					onResult(result.Value);
				});
			}
		});
	}

	private void OnMilestoneRollFinish(MilestoneResult result)
	{
		if (string.IsNullOrEmpty(result.RewardItemId))
		{
			SetResult(result);
			return;
		}
		GameSystem<InventorySystem>.Instance().AddOnItemEvent(result.RewardItemId, delegate
		{
			SetResult(result);
		});
	}

	private void SetResult(MilestoneResult result, bool effect = true)
	{
		_lastMilestoneResult = result;
		_resultWidget.gameObject.SetActive(!_forceSetResult);
		_resultWidget.Set(result);
		SetState(RollState.Result);
		if (string.IsNullOrEmpty(result.SelectedTagId))
		{
			_infoWidget.SetEmpty();
		}
		else
		{
			_infoWidget.Set(result.SelectedTagId);
		}
		ItemData itemData = ((!string.IsNullOrEmpty(result.RewardItemId)) ? GameSystem<InventorySystem>.Instance().FindItem(result.RewardItemId) : null);
		if (itemData == null)
		{
			_itemRewardedObject.gameObject.SetActive(value: false);
		}
		else
		{
			_itemRewardedObject.gameObject.SetActive(value: true);
			_itemRewardedObject.transform.Find("Icon").GetComponent<ItemIconTex>().SetIcon(itemData);
			_itemRewardedObject.transform.Find("Text").GetComponent<UILabel>().text = itemData.Name;
			UIManager.SystemMsg(T._("<em>{0}</em>{0:-을} 받았습니다.", itemData.Name));
		}
		_infoWidget.GetComponent<TweenerPlayer>().Play(0.6f);
		Yaml.Cost revertMilestoneCost = GetRevertMilestoneCost(result);
		_rerollButton.Text = T._("다시 돌리기 {0}", revertMilestoneCost.CostToEmphasisString(InventorySystem.Wallet));
		Point2 preferredSize = _rerollButton.GetPreferredSize();
		_rerollButton.SetDimensions(preferredSize.x + 40, _rerollButton.Widget.height);
		_buttonsWidget.UpdateLayout();
		Yaml.Tag tag = ((result.SelectedTagId != null) ? SingletonDict<string, Yaml.Tag>.Get(result.SelectedTagId) : null);
		_selectedTagId = ((result.SelectedTagId != null) ? result.SelectedTagId : string.Empty);
		if (tag != null && tag.Grade > TagGrade.Negative && effect)
		{
			SoundManager.PlayEvent("ui_animal_ability_result_success");
			_celebrationWidget.gameObject.SetActive(value: true);
		}
		else
		{
			SoundManager.PlayEvent("ui_animal_ability_result_fail");
			_celebrationWidget.gameObject.SetActive(value: false);
		}
		_previewViewer.SetAnimalAnimation("idle", "stand");
	}

	private void SetResult(DrawSkillResult result, bool effect = true)
	{
		_lastDrawSkillResult = result;
		SetState(RollState.Result);
		_resultWidget.gameObject.SetActive(value: false);
		if (string.IsNullOrEmpty(result.Skill.SkillId))
		{
			_infoWidget.SetEmpty();
		}
		else
		{
			_infoWidget.Set(result.Skill);
		}
		_infoWidget.GetComponent<TweenerPlayer>().Play();
		Yaml.Cost revertActiveSkillCost = GetRevertActiveSkillCost(result);
		_rerollButton.Text = T._("다시 돌리기 {0}", revertActiveSkillCost.CostToEmphasisString(InventorySystem.Wallet));
		Point2 preferredSize = _rerollButton.GetPreferredSize();
		_rerollButton.SetDimensions(preferredSize.x + 40, _rerollButton.Widget.height);
		_buttonsWidget.UpdateLayout();
		if (effect)
		{
			_celebrationWidget.gameObject.SetActive(value: true);
			if (result.Skill.Rank == SkillRank.S)
			{
				SoundManager.PlayEvent("ui_animal_specialability_result_srank");
			}
			else
			{
				SoundManager.PlayEvent("ui_animal_specialability_result");
			}
		}
		_previewViewer.SetAnimalAnimation("idle", "stand");
	}

	private static Yaml.Cost GetRevertMilestoneCost(MilestoneResult drawMilestoneResult)
	{
		return GetRevertMilestoneCost(drawMilestoneResult.RetryCost);
	}

	private static Yaml.Cost GetRevertMilestoneCost(Money money)
	{
		Yaml.Cost petRevertMilestone = Yaml.Util.Singleton<CostsYaml>.Instance.PetRevertMilestone;
		return Yaml.Cost.ConvertToYamlCost(money, petRevertMilestone.VoucherId, 1);
	}

	private static Yaml.Cost GetRevertActiveSkillCost(DrawSkillResult drawSkillResult)
	{
		return GetRevertActiveSkillCost(drawSkillResult.RetryCost);
	}

	private static Yaml.Cost GetRevertActiveSkillCost(Money money)
	{
		Yaml.Cost petRevertActiveSkill = Yaml.Util.Singleton<CostsYaml>.Instance.PetRevertActiveSkill;
		return Yaml.Cost.ConvertToYamlCost(money, petRevertActiveSkill.VoucherId, 1);
	}
}
