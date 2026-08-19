using System;
using System.Collections.Generic;
using Durango.Development;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class DomesticCageGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private DomesticCagePetListWidget _petList;

	[SerializeField]
	private DomesticCagePetInfoWidget _petInfo;

	private Artifact _cage;

	private bool _selectFirstRein;

	private string _selectedRein;

	private float? _dirtyAt;

	private bool _waitRequest;

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	private void Start()
	{
		SetChildrenActive(activated: false);
		base.OnOpenSucceed += Opened;
		base.OnCloseSucceed += Closed;
		_petList.Selected += delegate(DomesticationInfo info)
		{
			SoundManager.PlayEvent("ui_button_tame_list");
			SelectRein(info);
		};
		_petList.ReinAdded += OnAddRein;
		_petList.SkipProgressCheat += OnSkipProgressCheat;
		_petInfo.Released += OnReleaseRein;
		_petInfo.DomesticateStarted += OnStartDomestication;
		_petInfo.DomesticateStoped += OnStopDomestication;
		_petInfo.DomesticateFinished += OnFinishDomestication;
		_petInfo.ReinTookOut += OnTakeOutRein;
		_petInfo.OnFeed += OnFeed;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.OpenDomesticCage, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				Open(targetComponent);
			}
		});
	}

	private void Update()
	{
		if (!base.IsOpened)
		{
			return;
		}
		float? dirtyAt = _dirtyAt;
		if (dirtyAt.HasValue)
		{
			float time = Time.time;
			if (_dirtyAt.Value < time)
			{
				Refresh();
			}
		}
	}

	private void Opened()
	{
		Artifact.ArtifactStateChanged += OnArtifactStateChange;
	}

	private void Closed()
	{
		Artifact.ArtifactStateChanged -= OnArtifactStateChange;
		_selectedRein = null;
		_waitRequest = false;
	}

	public void Open([NotNull] Artifact artifact)
	{
		if (artifact.ArtifactState.DomesticCage.HasValue)
		{
			base.Open();
			_cage = artifact;
			_selectFirstRein = true;
			Refresh();
		}
	}

	private void MarkAsDirty()
	{
		_dirtyAt = 0f;
	}

	private void SelectRein(DomesticationInfo? info)
	{
		if (info.HasValue)
		{
			string id = (_selectedRein = info.Value.ItemId);
			_petList.Select(id);
			_petInfo.Set(info.Value);
		}
		else
		{
			_selectedRein = null;
			_petList.Select(null);
			_petInfo.SetEmpty();
		}
	}

	private void OnArtifactStateChange(Artifact artifact)
	{
		if (base.IsOpened && (!(_cage != null) || !(_cage.EntityId != artifact.EntityId)))
		{
			_cage = artifact;
			MarkAsDirty();
		}
	}

	private void Refresh()
	{
		_dirtyAt = null;
		Artifact cage = _cage;
		if (cage == null || !cage.ArtifactState.DomesticCage.HasValue)
		{
			return;
		}
		DomesticationInfo[] reins = cage.ArtifactState.DomesticCage.Value.Reins;
		_titleWidget.Object.SetTitle(cage.LocalizedName);
		_petList.Set(cage);
		DomesticationInfo? info = null;
		DomesticationInfo? domesticationInfo = null;
		int i = 0;
		for (int size = KUtility.GetSize(reins); i < size; i++)
		{
			if (!domesticationInfo.HasValue)
			{
				domesticationInfo = reins[i];
			}
			if (reins[i].ItemId == _selectedRein)
			{
				info = reins[i];
				break;
			}
		}
		if (_selectFirstRein && !info.HasValue)
		{
			info = domesticationInfo;
		}
		SelectRein(info);
		_selectFirstRein = false;
		_dirtyAt = CalcDirtyAt();
	}

	private void OnAddRein()
	{
		if (_cage == null || !_cage.ArtifactState.DomesticCage.HasValue)
		{
			return;
		}
		DomesticCage value = _cage.ArtifactState.DomesticCage.Value;
		Artifact cage = _cage;
		PetItemInteractionPopup petItemInteractionPopup = UIManager.Popup.Tooltip<PetItemInteractionPopup>();
		petItemInteractionPopup.SetAsReinSelection(value, delegate(ItemData selectedItem)
		{
			if (selectedItem != null)
			{
				UIManager.MessageBox.ShowLockConfirm(selectedItem, delegate
				{
					PetManager.PutInReinsToCage(cage.EntityId, cage.WorldTile, selectedItem.Id, delegate(bool success)
					{
						if (success)
						{
							_selectedRein = selectedItem.Id;
						}
					});
				});
			}
		});
		petItemInteractionPopup.Show();
	}

	private void OnReleaseRein(DomesticationInfo target)
	{
		Artifact cage = _cage;
		UIManager.MessageBox.Show(T._("동물을 야생에 풀어주시겠습니까?"), T._("<alert>[icon=icon_make_alert] 풀어준 동물은 야생으로 돌아가 사라집니다.</alert>"), delegate(int index)
		{
			if (index == 0)
			{
				_selectFirstRein = true;
				PetManager.ReleaseReinFromCage(target.ItemId, new PropKey
				{
					EntityId = cage.EntityId,
					Tile = cage.WorldTile
				}, delegate
				{
					UIManager.SystemMsg(T._("동물을 풀어주었습니다."));
				});
			}
		}, new MessageBox.Button
		{
			Text = T._("풀어주기"),
			Style = PresetButton.Style.Solid,
			Sound = "ui_button_release"
		}, new MessageBox.Button
		{
			Text = T._("취소"),
			Style = PresetButton.Style.Border
		});
	}

	private void OnStartDomestication(DomesticationInfo target)
	{
		if (!_waitRequest)
		{
			Artifact cage = _cage;
			_waitRequest = true;
			PetManager.StartDomestication(cage.EntityId, cage.WorldTile, target.ItemId, delegate
			{
				_waitRequest = false;
			});
		}
	}

	private void OnStopDomestication(DomesticationInfo target)
	{
		Artifact cage = _cage;
		UIManager.MessageBox.Show(T._("길들이기를 취소하시겠습니까?"), T._("<alert>[icon=icon_make_alert] 지금까지의 시간과 먹이는 보존되지 않습니다.</alert>"), delegate(bool ok)
		{
			if (ok)
			{
				PetManager.CancelDomestication(cage.EntityId, cage.WorldTile, target.ItemId, null);
			}
		});
	}

	private void OnFinishDomestication(DomesticationInfo target)
	{
		if (_waitRequest)
		{
			return;
		}
		Artifact cage = _cage;
		_waitRequest = true;
		PetManager.FinishDomestication(cage.EntityId, cage.WorldTile, target.ItemId, delegate(DomesticationResult? result)
		{
			_waitRequest = false;
			if (result.HasValue && cage.ArtifactState.DomesticCage.HasValue)
			{
				if (!result.Value.Domesticated)
				{
					SoundManager.PlayEvent("ui_tame_result_fail");
					UIManager.SystemMsg(T._("길들이기에 실패하였습니다."));
					_petList.Select(null);
					_petInfo.SetEscaped();
				}
				else
				{
					UIManager.Popup.Tooltip<DomesticationRewardPopup>().SetLevel(target.Level).SetType(target.EntityType)
						.SetCancelText(T._("축사에 보관"))
						.SetConfirm(T._("가방에 넣기"), delegate
						{
							_selectFirstRein = true;
							PetManager.TakeOutReinFromCage(cage.EntityId, cage.WorldTile, target.ItemId, delegate(bool isSuccess)
							{
								if (isSuccess)
								{
									UIManager.SystemMsg(T._("동물을 가방으로 옮겼습니다."));
								}
							});
						})
						.SetResult(result.Value)
						.Show();
				}
			}
		});
	}

	private void OnTakeOutRein(DomesticationInfo target)
	{
		if (_waitRequest)
		{
			return;
		}
		Artifact cage = _cage;
		_waitRequest = true;
		_selectFirstRein = true;
		PetManager.TakeOutReinFromCage(cage.EntityId, cage.WorldTile, target.ItemId, delegate(bool isSuccess)
		{
			_waitRequest = false;
			if (isSuccess)
			{
				UIManager.SystemMsg(T._("동물을 가방으로 옮겼습니다."));
			}
		});
	}

	private void OnFeed(DomesticationInfo target)
	{
		if (Yaml.Util.Singleton<Constants>.Instance.Pet.IsDomesticationTimeFullyModified(target))
		{
			UIManager.SystemMsg(T._("더 이상 효과를 볼 수 없습니다."));
			return;
		}
		Artifact cage = _cage;
		PetItemInteractionPopup petItemInteractionPopup = UIManager.Popup.Tooltip<PetItemInteractionPopup>();
		petItemInteractionPopup.SetAsFeeding(target, delegate(List<ItemData> selectedItems)
		{
			if (KUtility.GetSize(selectedItems) != 0)
			{
				UIManager.MessageBox.ShowLockConfirm(selectedItems, delegate(string[] ids)
				{
					PlayYammyAnimation(target.ItemId);
					PetManager.PutItemsForDomestication(cage.EntityId, cage.WorldTile, target.ItemId, ids);
				});
			}
		});
		petItemInteractionPopup.Show();
	}

	private float? CalcDirtyAt()
	{
		if (_cage == null || !_cage.ArtifactState.DomesticCage.HasValue)
		{
			return null;
		}
		DomesticationInfo[] reins = _cage.ArtifactState.DomesticCage.Value.Reins;
		double? num = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		int i = 0;
		for (int size = KUtility.GetSize(reins); i < size; i++)
		{
			if (reins[i].DomesticationInProgress)
			{
				double num2 = reins[i].DomesticateUntil - predictedServerTime;
				if (!(num2 <= 0.0) && (!num.HasValue || num2 < num.Value))
				{
					num = num2;
				}
			}
		}
		if (num.HasValue)
		{
			return Time.time + (float)num.Value;
		}
		return null;
	}

	private void PlayYammyAnimation(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			_petList.PlayYammyAnimation(id);
			_petInfo.PlayYammyAnimation();
		}
	}

	private void OnSkipProgressCheat(DomesticationInfo info)
	{
		if (Debug.isDebugBuild && info.DomesticationInProgress)
		{
			double num = info.DomesticateUntil - Connections.Frontend.GetPredictedServerTime();
			if (!(num <= 3.0))
			{
				Durango.Utils.Singleton<Commands>.Instance().Cheat($"domestic reduce time {_cage.WorldTile.x},{_cage.WorldTile.y} {info.ItemId} {(int)(num - 3.0)}");
			}
		}
	}
}
