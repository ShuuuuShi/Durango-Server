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
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class GrowCageGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private GrowCagePetListWidget _petList;

	[SerializeField]
	private GrowCagePetInfoWidget _petInfo;

	private Artifact _cage;

	private bool _selectFirstPet;

	private string _selectedPet;

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
		_petList.Selected += delegate(Messages.Pet pet)
		{
			SelectPet(pet);
		};
		_petList.PetAdded += OnAddPet;
		_petList.SkipTaskCheat += OnSkipTaskCheat;
		_petInfo.TaskStarted += OnStartTask;
		_petInfo.TaskStopped += OnStopTask;
		_petInfo.TaskFinished += OnFinishTask;
		_petInfo.PetTookOut += OnTakeOutPet;
		_petInfo.OnFeed += OnFeed;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Cage, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			Open(targetComponent);
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
		_selectedPet = null;
		_waitRequest = false;
	}

	public void Open(Artifact artifact)
	{
		if (PetUtil.GetGrowCage(artifact).HasValue)
		{
			_selectFirstPet = true;
			base.Open();
			SetArtifact(artifact);
		}
	}

	private void SelectPet(Messages.Pet? pet)
	{
		if (pet.HasValue)
		{
			string id = (_selectedPet = pet.Value.EntityId);
			_petList.Select(id);
			GrowCage? growCage = PetUtil.GetGrowCage(_cage);
			TaskStatus? task = (growCage.HasValue ? growCage.Value.GetTaskStatus(id) : null);
			_petInfo.Set(pet.Value, task);
		}
		else
		{
			_selectedPet = null;
			_petList.Select(null);
			_petInfo.SetEmpty();
		}
	}

	private void OnArtifactStateChange(Artifact artifact)
	{
		if (base.IsOpened && (!(_cage != null) || !(_cage.EntityId != artifact.EntityId)))
		{
			SetArtifact(artifact);
		}
	}

	private void SetArtifact([NotNull] Artifact artifact)
	{
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		_cage = artifact;
		MarkAsDirty();
		OnUpdateCage(growCage, PetUtil.GetGrowCage(_cage));
	}

	private void OnUpdateCage(GrowCage? prev, GrowCage? current)
	{
		if (!prev.HasValue || !current.HasValue)
		{
			return;
		}
		Messages.Pet[] data = prev.Value.Pets.Data;
		Messages.Pet[] data2 = current.Value.Pets.Data;
		for (int i = 0; i < data2.Length; i++)
		{
			Messages.Pet pet = data2[i];
			Messages.Pet[] array = data;
			for (int j = 0; j < array.Length; j++)
			{
				Messages.Pet pet2 = array[j];
				if (pet.EntityId == pet2.EntityId)
				{
					if (pet2.Statistics.Level < pet.Statistics.Level)
					{
						UIManager.SystemMsg(T._("{0:이} {1:이} 되었습니다.", pet.GetPetName(), LocalizeUtil.FormatLevel(pet.Statistics.Level)));
						SoundManager.PlayEvent("ui_animal_levelup");
					}
					break;
				}
			}
		}
	}

	private void MarkAsDirty()
	{
		_dirtyAt = 0f;
	}

	private void Refresh()
	{
		_dirtyAt = null;
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		if (!growCage.HasValue)
		{
			return;
		}
		_titleWidget.Object.SetTitle(_cage.LocalizedName);
		_petList.Set(_cage);
		Messages.Pet[] data = growCage.Value.Pets.Data;
		Messages.Pet? pet = null;
		Messages.Pet? pet2 = null;
		int i = 0;
		for (int size = KUtility.GetSize(data); i < size; i++)
		{
			if (!pet2.HasValue)
			{
				pet2 = data[i];
			}
			if (data[i].EntityId == _selectedPet)
			{
				pet = data[i];
				break;
			}
		}
		if (_selectFirstPet && !pet.HasValue)
		{
			pet = pet2;
		}
		SelectPet(pet);
		_selectFirstPet = false;
		_dirtyAt = CalcDirtyAt();
	}

	private void OnAddPet()
	{
		GrowCage? cage = PetUtil.GetGrowCage(_cage);
		if (!cage.HasValue || _waitRequest)
		{
			return;
		}
		_waitRequest = true;
		PetManager.GetPetList(delegate(PetsInfo? petsInfo)
		{
			_waitRequest = false;
			if (petsInfo.HasValue)
			{
				List<Messages.Pet> list = new List<Messages.Pet>();
				Messages.Pet[] data = petsInfo.Value.Pets.Data;
				for (int i = 0; i < data.Length; i++)
				{
					Messages.Pet item = data[i];
					CageInfo? cageInfo = item.CageInfo;
					if (!cageInfo.HasValue || string.IsNullOrEmpty(item.CageInfo.Value.RegionId))
					{
						list.Add(item);
					}
				}
				if (list.Count == 0)
				{
					UIManager.SystemMsg(T._("축사에 넣을 수 있는 동물이 없습니다."));
				}
				else
				{
					PropKey cageProp = _cage.GetPropKey();
					UIManager.Popup.Tooltip<SelectPetPopup>().SetTitle(T._("축사에 동물을 넣겠습니까?")).SetCapacity(cage.Value.Size - cage.Value.RemainSize, cage.Value.Size)
						.SetList(list)
						.SetOnConfirm(delegate(Messages.Pet pet)
						{
							PetManager.PutInCage(cageProp.EntityId, cageProp.Tile, pet.EntityId, delegate(bool success)
							{
								if (success)
								{
									_selectedPet = pet.EntityId;
								}
							});
						})
						.Show();
				}
			}
		});
	}

	private void OnStartTask(Messages.Pet target, PetTaskType taskType)
	{
		if (_cage == null)
		{
			return;
		}
		SelectPetTaskPopup selectPetTaskPopup = UIManager.Popup.Tooltip<SelectPetTaskPopup>();
		if (taskType != 0)
		{
			if (taskType == PetTaskType.Training)
			{
				selectPetTaskPopup.SetTitle(T._("어떤 훈련을 시키겠습니까?"));
			}
		}
		else
		{
			selectPetTaskPopup.SetTitle(T._("얼마나 채집하시겠습니까?"));
		}
		PropKey prop = _cage.GetPropKey();
		selectPetTaskPopup.SetPet(target).SetCage(_cage).SetOnSelected(delegate(string taskId)
		{
			PetTask petTask = SingletonDict<string, PetTask>.Get(taskId);
			if (petTask == null)
			{
				return false;
			}
			if (target.Stat.Hungry.Get() < petTask.HungryRequired)
			{
				UIManager.SystemMsg(T._("활력이 부족해 시작할 수 없습니다"));
				return false;
			}
			PetManager.StartPetTask(prop, target.EntityId, taskId, null);
			return true;
		})
			.SetFilter((PetTask task) => task.Type == taskType)
			.Show();
	}

	private void OnStopTask(Messages.Pet target)
	{
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		if (!growCage.HasValue)
		{
			return;
		}
		TaskStatus? taskStatus = growCage.Value.GetTaskStatus(target.EntityId);
		if (!taskStatus.HasValue)
		{
			return;
		}
		PetTask petTask = ((!string.IsNullOrEmpty(taskStatus.Value.TaskId)) ? SingletonDict<string, PetTask>.Get(taskStatus.Value.TaskId) : null);
		if (petTask == null)
		{
			return;
		}
		string mainText = null;
		string subText = null;
		switch (petTask.Type)
		{
		case PetTaskType.Production:
			mainText = T._("생산을 중지하시겠습니까?");
			subText = T._("<alert>[icon=icon_make_alert] 생산 결과물과 경험치를 받을 수 없으며 지금까지 소모한 시간과 먹이가 모두 사라집니다.</alert>");
			break;
		case PetTaskType.Training:
			mainText = T._("훈련을 중지하시겠습니까?");
			subText = T._("<alert>[icon=icon_make_alert] 훈련 경험치를 받을 수 없으며 지금까지 소모한 시간과 먹이가 모두 사라집니다.</alert>");
			break;
		}
		PropKey propKey = _cage.GetPropKey();
		UIManager.MessageBox.Show(mainText, subText, delegate(bool ok)
		{
			if (ok)
			{
				PetManager.CancelPetTask(propKey, target.EntityId, null);
			}
		});
	}

	private void OnFinishTask(Messages.Pet target)
	{
		if (!_waitRequest && !(_cage == null))
		{
			PropKey propKey = _cage.GetPropKey();
			_waitRequest = true;
			PetManager.FinishPetTask(propKey, target.EntityId, delegate
			{
				_waitRequest = false;
			});
		}
	}

	private void OnTakeOutPet(Messages.Pet target)
	{
		if (!_waitRequest)
		{
			Artifact cage = _cage;
			_waitRequest = true;
			_selectFirstPet = true;
			PetManager.TakeOutCage(cage.EntityId, cage.WorldTile, target.EntityId, delegate
			{
				_waitRequest = false;
			});
		}
	}

	private void OnFeed(Messages.Pet target)
	{
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		TaskStatus? task = null;
		if (growCage.HasValue)
		{
			task = growCage.Value.GetTaskStatus(target.EntityId);
		}
		PropKey prop = _cage.GetPropKey();
		PetItemInteractionPopup petItemInteractionPopup = UIManager.Popup.Tooltip<PetItemInteractionPopup>();
		petItemInteractionPopup.SetAsFeeding(target, task, delegate(List<ItemData> items)
		{
			if (KUtility.GetSize(items) != 0)
			{
				UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] itemIds)
				{
					PetManager.FeedPet(prop, target.EntityId, itemIds, delegate(bool ok)
					{
						if (ok)
						{
							UIManager.SystemMsg(T._("동물이 먹이를 먹었습니다"));
						}
					});
				});
			}
		});
		petItemInteractionPopup.Show();
	}

	private float? CalcDirtyAt()
	{
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		if (!growCage.HasValue)
		{
			return null;
		}
		Dictionary<string, TaskStatus> tasks = growCage.Value.Tasks;
		if (tasks == null)
		{
			return null;
		}
		double? num = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		foreach (KeyValuePair<string, TaskStatus> item in tasks)
		{
			double num2 = item.Value.Until - predictedServerTime;
			if (!(num2 <= 0.0) && (!num.HasValue || num2 < num.Value))
			{
				num = num2;
			}
		}
		if (num.HasValue)
		{
			return Time.time + (float)num.Value;
		}
		return null;
	}

	private void OnSkipTaskCheat(Messages.Pet pet)
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}
		GrowCage? growCage = PetUtil.GetGrowCage(_cage);
		TaskStatus? taskStatus = (growCage.HasValue ? growCage.Value.GetTaskStatus(pet.EntityId) : null);
		if (taskStatus.HasValue)
		{
			double num = taskStatus.Value.Until - Connections.Frontend.GetPredictedServerTime();
			if (!(num <= 3.0))
			{
				Durango.Utils.Singleton<Commands>.Instance().Cheat($"grow reduce time {_cage.WorldTile.x},{_cage.WorldTile.y} {pet.EntityId} {(int)(num - 3.0)}");
			}
		}
	}
}
