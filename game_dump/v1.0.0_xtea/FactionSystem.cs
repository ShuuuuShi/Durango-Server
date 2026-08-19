using System;
using System.Collections.Generic;
using K1Network;
using L10N;
using Messages;
using Shared.Faction;
using Shared.Inspect;
using Shared.System;
using TimerData;
using UnityEngine;

public class FactionSystem : GameSystem<FactionSystem>
{
	private class FactionSorter
	{
		private static readonly FactionType[] FactionTypes = new FactionType[5]
		{
			FactionType.Lama,
			FactionType.TheFirm,
			FactionType.ChlorophylForum,
			FactionType.ChamberOfPioneer,
			FactionType.TheCommittee
		};

		private readonly Dictionary<FactionType, Faction> _factionDictionary = new Dictionary<FactionType, Faction>();

		private readonly List<Faction?> _factionList = new List<Faction?>();

		public IList<Faction?> GetSortedFactions(Faction[] factions)
		{
			_factionDictionary.Clear();
			for (int i = 0; i < factions.Length; i++)
			{
				Faction value = factions[i];
				_factionDictionary[value.Type] = value;
			}
			_factionList.Clear();
			for (int j = 0; j < FactionTypes.Length; j++)
			{
				if (_factionDictionary.TryGetValue(FactionTypes[j], out var value2))
				{
					_factionList.Add(value2);
				}
				else
				{
					_factionList.Add(null);
				}
			}
			return _factionList;
		}
	}

	private readonly Dictionary<AnimalHealthStatus, string[]> _animalStatusFeedback = new Dictionary<AnimalHealthStatus, string[]>
	{
		{
			AnimalHealthStatus.Disease,
			new string[6]
			{
				T.N_("병들었다. 병균이 감염성이 있으니 도태시켜야 한다."),
				T.N_("병들었다. 더 큰 피해를 막기 위해 개체를 살처분하자."),
				T.N_("병들었다. 수역에는 선제적 균형 조치가 필요하다."),
				T.N_("병들었다. 방치하면 역병이 될 수 있으니 처리하자."),
				T.N_("병들었다. 전염성이 강하니 일찌감치 대응하자."),
				T.N_("병들었다. 섬의 보건을 위해 죽여야 한다.")
			}
		},
		{
			AnimalHealthStatus.Hunger,
			new string[6]
			{
				T.N_("굶었다. 직접 먹이를 찾을 기운만 회복하게 먹이자."),
				T.N_("굶었다. 개체 수 감소가 우려되니 먹이를 주자."),
				T.N_("굶었다. 길들지 않게 소량의 먹이만 주자."),
				T.N_("굶었다. 개체 수 유지를 위해 최소한의 먹이만 주자."),
				T.N_("굶었다. 먹이를 주는 건 어디까지나 균형을 위해서다."),
				T.N_("굶었다. 스스로 움직일 수 있을 정도만 먹이자.")
			}
		},
		{
			AnimalHealthStatus.Hurt,
			new string[6]
			{
				T.N_("아파한다. 무리 유지에 필수적이니 치료하자."),
				T.N_("아파한다. 개체 수 감소가 걱정되니 보살펴주자."),
				T.N_("아파한다. 주변 생태계 균형을 위해 고쳐주자."),
				T.N_("아파한다. 스스로 버텨야 하지만 이번만 치료하자."),
				T.N_("아파한다. 임시로 치료해주도록 하자."),
				T.N_("아파한다. 자력으로 회복할 때까지만 보살피자.")
			}
		}
	};

	private readonly Dictionary<NaturalHealthStatus, string[]> _naturalStatusFeedback = new Dictionary<NaturalHealthStatus, string[]>
	{
		{
			NaturalHealthStatus.PlantDry,
			new string[6]
			{
				T.N_("말랐다. 식생이 무성해야 하니 물을 주자."),
				T.N_("말랐다. 지력을 돋워야 하니 수분을 공급하자."),
				T.N_("말랐다. 사막화를 막으려면 물길이 필요하다."),
				T.N_("말랐다. 주변에 악영향이 있으니 물을 줘야 한다."),
				T.N_("말랐다. 섬 생태계의 핵심 역할이니 해갈해주자."),
				T.N_("말랐다. 섬의 생태계는 화분과 같으니 물을 주자.")
			}
		},
		{
			NaturalHealthStatus.PlantDisease,
			new string[6]
			{
				T.N_("병들었다. 영양을 주어 치료하자."),
				T.N_("병들었다. 주변에 퍼지지 않게 치료하자."),
				T.N_("병들었다. 병이 퍼지지 않게 대처하자."),
				T.N_("병들었다. 스스로 버틸 수 있게 돋우자."),
				T.N_("병들었다. 병의 원인을 밝혀 치료하자."),
				T.N_("병들었다. 식물이 자가회복하게 돕자.")
			}
		},
		{
			NaturalHealthStatus.PlantRot,
			new string[6]
			{
				T.N_("고사했다. 근처에 병을 옮길 수 있으니 제거하자."),
				T.N_("고사했다. 스스로 회복이 어려울듯 하니 베어내자."),
				T.N_("고사했다. 예측이 곤란하니 선제적으로 제거하자."),
				T.N_("고사했다. 주변에 감염될 우려가 있으니 베자."),
				T.N_("고사했다. 회생이 불가능하니 잘라내자."),
				T.N_("고사했다. 예방 차원에서 처리하도록 하자.")
			}
		},
		{
			NaturalHealthStatus.MineralPoisonous,
			new string[6]
			{
				T.N_("공기가 유독하다. 주변을 정화하자."),
				T.N_("공기가 유독하다. 바람이 빠지게 하자."),
				T.N_("공기가 유독하다. 해로운 공기를 빼내자."),
				T.N_("공기가 유독하다. 충분한 제독 과정을 거치자."),
				T.N_("공기가 유독하다. 위험이 분산되도록 하자."),
				T.N_("공기가 유독하다. 신선한 공기로 솎아내자.")
			}
		},
		{
			NaturalHealthStatus.MineralWastewater,
			new string[6]
			{
				T.N_("폐수가 흐른다. 주변을 오염시키니 처리하자."),
				T.N_("폐수가 흐른다. 정수하도록 하자."),
				T.N_("폐수가 흐른다. 약품을 뿌려 놓자."),
				T.N_("폐수가 흐른다. 퍼지지 않게 제어하자."),
				T.N_("폐수가 흐른다. 토양에 스미지 않게 대처하자."),
				T.N_("폐수가 흐른다. 깨끗이 바꿔 놓자.")
			}
		}
	};

	private readonly Dictionary<FactionType, string[]> _disabledConfactFactionFeedbacks = new Dictionary<FactionType, string[]>
	{
		{
			FactionType.ChamberOfPioneer,
			new string[10]
			{
				T.N_("미안. 애들 때문에 지금 연락 못 받아."),
				T.N_("지금 대표가 사무실에 찾아와서. 이따가 연락 줄래?"),
				T.N_("당신아. 나 지금 속이 너무 안 좋아. 다음에 무전해줘…"),
				T.N_("근처 전파가 안 좋아서 당신 무전이 잘 안 들린다. 잠시 후에 다시 해줄래?"),
				T.N_("밤새서 일해서 너무 피곤해. 눈 좀 붙이게 좀 뒤에 무전 보내줘."),
				T.N_("처리할 서류가 많아서 잠깐만 있다가 연락 주면 안 될까?"),
				T.N_("내가 사랑하는 거 알지? 나 눈 좀 붙이게 이따 연락줘."),
				T.N_("사무실에 콤프소가 들어와서 난리통이야. 나중에 다시 연락해."),
				T.N_("식사 중이야. 좀 있다가 다시 무전해줘."),
				T.N_("나 자고 있었어… 잠 좀 잘게. 나중에…")
			}
		},
		{
			FactionType.ChlorophylForum,
			new string[10]
			{
				T.N_("회의 중입니다. 이따가 연락주세요."),
				T.N_("몸 상태가 좋지 않아 휴식 중입니다. 다음에 연락 부탁드립니다."),
				T.N_("잠시 후에 보고가 있어서 연락 받기가 어렵습니다."),
				T.N_("작성할 문서가 있어 응답이 어렵습니다."),
				T.N_("외부 시찰 중이라 지금은 연락 받기가 어려울 것 같습니다."),
				T.N_("주변 정화 봉사활동 중이라 무전은 이따가 받겠습니다."),
				T.N_("무전 상태가 좋지 않습니다. 나중에 다시 연락주세요."),
				T.N_("회의가 길어지고 있어서, 잠시 무전이 어렵습니다."),
				T.N_("피곤해서 졸았습니다. 얼마 있다가 연락 주시면 안 될까요?"),
				T.N_("철야를 했더니… 제정신이 아니라 지금은 연락 못 받겠습니다.")
			}
		}
	};

	private Dictionary<FactionType, double> _latestReceivedFactionCooltimes = new Dictionary<FactionType, double>();

	private FactionSorter _factionSorter = new FactionSorter();

	private FactionRadioDisplay _radioDisplay;

	private FactionTodoUpdater _factionTodoUpdater;

	private Dictionary<FactionType, List<FactionRadioRecord>> _radioRecordsDictionary;

	public event Action<IList<Faction?>> FactionsUpdated;

	public event Action<FactionType> FactionRecordUpdated;

	private void Awake()
	{
		_radioDisplay = new FactionRadioDisplay();
		_factionTodoUpdater = new FactionTodoUpdater();
		Connections.Frontend.On(delegate(FactionEvents msg, PacketHeader _)
		{
			_factionTodoUpdater.FactionTodoUpdated(msg);
			RequestFactions();
		});
		Connections.Frontend.On(delegate(FactionRadio msg, PacketHeader _)
		{
			if (AddNewFactionRadioRecord(msg.Faction, msg.Messages, Connections.Frontend.GetPredictedServerTime()) && this.FactionRecordUpdated != null)
			{
				this.FactionRecordUpdated(msg.Faction);
			}
			_radioDisplay.FactionRadioReceived(msg, _);
		});
		Connections.Frontend.On<StrangeRadio>(_radioDisplay.StrangeRadioReceived);
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			_factionTodoUpdater.LoadTodos();
			RequestFactions();
		};
		AddInteractionHandlers();
	}

	private void AddInteractionHandlers()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AnimalInspection, delegate(InteractionObject target)
		{
			AnimalBehavior targetComponent = target.GetTargetComponent<AnimalBehavior>();
			if (!((Object)(object)targetComponent == (Object)null))
			{
				Connections.Frontend.Send(new TryInspectAnimal
				{
					EntityId = target.EntityId
				}).On(delegate(Messages.Timer timerMsg, PacketHeader _)
				{
					DisplayGaugeAndPlayMotion(timerMsg.Duration, "Faction_Watch");
				}).On(delegate(Error msg, PacketHeader _)
				{
					if (msg.TypeName == "TooFarAway")
					{
						UIManager.SystemMsg(T._("대상이 멀리 있습니다."));
					}
				})
					.On(delegate(FailedInspect msg, PacketHeader _)
					{
						UIManager.SystemMsg(T._("관찰에 실패했다.") + msg.Reason);
					})
					.On(delegate(AnimalHealthStatusChanged msg, PacketHeader _)
					{
						string[] value4;
						if (msg.Status == AnimalHealthStatus.Healthy)
						{
							UIManager.SystemMsg(T._("관찰 결과 특별한 이상은 없다."));
						}
						else if (_animalStatusFeedback.TryGetValue(msg.Status, out value4))
						{
							int num3 = Random.Range(0, value4.Length - 1);
							UIManager.SystemMsg(T._(value4[num3]));
						}
					});
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.NaturalInspection, delegate(InteractionObject target)
		{
			Connections.Frontend.Send(new TryInspectNatural
			{
				EntityId = target.EntityId
			}).On(delegate(Messages.Timer timerMsg, PacketHeader _)
			{
				DisplayGaugeAndPlayMotion(timerMsg.Duration, "Faction_Watch");
			}).On(delegate(Error msg, PacketHeader _)
			{
				if (msg.TypeName == "TooFarAway")
				{
					UIManager.SystemMsg(T._("대상이 멀리 있습니다."));
				}
			})
				.On(delegate(FailedInspect msg, PacketHeader _)
				{
					UIManager.SystemMsg(T._("관찰에 실패했다.") + msg.Reason);
				})
				.On(delegate(NaturalHealthStatusChanged msg, PacketHeader _)
				{
					string[] value3;
					if (msg.Status == NaturalHealthStatus.Healthy)
					{
						UIManager.SystemMsg(T._("관찰 결과 특별한 이상은 없다."));
					}
					else if (_naturalStatusFeedback.TryGetValue(msg.Status, out value3))
					{
						int num2 = Random.Range(0, value3.Length - 1);
						UIManager.SystemMsg(T._(value3[num2]));
					}
				});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AnimalFeed, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "barehand_gather_low");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AnimalHeal, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "barehand_gather_low");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.NaturalWater, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "Farming_Water");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.NaturalCure, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "Barehand_Gather_Low");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.NaturalPoisonPurify, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "Cook_Barbecue_Sit");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.NaturalWaterPurify, delegate(InteractionObject target)
		{
			InspectFollowUpAct(target.EntityId, "Ride_Saddle");
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ContactFaction, delegate(InteractionObject target)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (!GameSystem<MapSystem>.Instance().IsExploredPoi(new Point2(target.Tile)))
			{
				GameSystem<MapSystem>.Instance().SearchNearPOIProp();
			}
			Connections.Frontend.Send(new ContactFaction
			{
				EntityId = target.EntityId
			}).On(delegate(Error msg, PacketHeader _)
			{
				//IL_0081: Unknown result type (might be due to invalid IL or missing references)
				if (msg.TypeName == "NotActivated")
				{
					UIManager.SystemMsg(T._("크레이터와 관련된 단체를 아직 만나지 못 했습니다."));
				}
				else if (msg.TypeName == "AlreadyCompletedCrator")
				{
					UIManager.SystemMsg(T._("이미 연락을 마친 크레이터입니다."));
				}
				else if (msg.TypeName == "NoAvailableEvent")
				{
					FactionType factionType = GameSystem<MapSystem>.Instance().FindFactionFromCraterTile(new Point2(target.Tile));
					if (_latestReceivedFactionCooltimes.TryGetValue(factionType, out var value))
					{
						double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
						string text = TimerSystem.TimeToString(value - predictedServerTime);
						UIManager.SystemMsg(T._("관련 임무는 {0} 뒤에 계속할 수 있습니다.", text));
					}
					else
					{
						UIManager.SystemMsg(T._("관련 임무는 잠시 뒤에 계속할 수 있습니다."));
					}
					if (_disabledConfactFactionFeedbacks.TryGetValue(factionType, out var value2))
					{
						int num = Random.Range(0, value2.Length - 1);
						_radioDisplay.ShowFactionRadioMessage(factionType, new string[1] { T._(value2[num]) });
					}
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RecontactFaction, delegate(InteractionObject target)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (!GameSystem<MapSystem>.Instance().IsExploredPoi(new Point2(target.Tile)))
			{
				GameSystem<MapSystem>.Instance().SearchNearPOIProp();
			}
			FactionType faction2 = GameSystem<MapSystem>.Instance().FindFactionFromCraterTile(new Point2(target.Tile));
			if (_factionTodoUpdater.IsFactionPlaying(faction2))
			{
				UIManager.MessageBox.Show(T._("해당 단체와 진행 중인 일이 있습니다. 포기하고 새로운 일을 하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						Connections.Frontend.Send(new CancelFactionEvents
						{
							Faction = faction2
						}).On<OK>(delegate
						{
							Connections.Frontend.Send(new ContactFaction
							{
								EntityId = target.EntityId
							});
						});
					}
				});
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.CancelFactionEvent, delegate(InteractionObject target)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			FactionType faction = GameSystem<MapSystem>.Instance().FindFactionFromCraterTile(new Point2(target.Tile));
			if (faction != FactionType.Invalid)
			{
				UIManager.MessageBox.Show(T._("포기하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						Connections.Frontend.Send(new CancelFactionEvents
						{
							Faction = faction
						});
					}
				});
			}
		});
	}

	public void RequestFactions()
	{
		Connections.Frontend.Send(default(GetFactions)).On(delegate(Factions msg, PacketHeader _)
		{
			int num = msg._Factions.Length;
			for (int i = 0; i < num; i++)
			{
				_latestReceivedFactionCooltimes[msg._Factions[i].Type] = msg._Factions[i].AvailableAt;
			}
			if (this.FactionsUpdated != null)
			{
				this.FactionsUpdated(_factionSorter.GetSortedFactions(msg._Factions));
			}
		});
	}

	public IList<FactionRadioRecord> GetFactionRecords(FactionType type)
	{
		InitializeRadioRecordsDictionary(type);
		return GetOrCreateFactionRecords(type);
	}

	private void InitializeRadioRecordsDictionary(FactionType requestedType)
	{
		if (_radioRecordsDictionary != null)
		{
			return;
		}
		_radioRecordsDictionary = new Dictionary<FactionType, List<FactionRadioRecord>>();
		Connections.Frontend.Send(default(GetFactionRadioHistory)).On(delegate(FactionRadioHistories msg, PacketHeader _)
		{
			for (int i = 0; i < msg.Histories.Length; i++)
			{
				FactionRadioHistory factionRadioHistory = msg.Histories[i];
				SetFactionRadioRecordsHistory(factionRadioHistory.Faction, factionRadioHistory.Messages);
			}
			if (this.FactionRecordUpdated != null)
			{
				this.FactionRecordUpdated(requestedType);
			}
		});
	}

	private List<FactionRadioRecord> GetOrCreateFactionRecords(FactionType type)
	{
		List<FactionRadioRecord> value = null;
		if (_radioRecordsDictionary != null && !_radioRecordsDictionary.TryGetValue(type, out value))
		{
			value = new List<FactionRadioRecord>();
			_radioRecordsDictionary[type] = value;
		}
		return value;
	}

	private void SetFactionRadioRecordsHistory(FactionType type, FactionRadioRecord[] messages)
	{
		List<FactionRadioRecord> orCreateFactionRecords = GetOrCreateFactionRecords(type);
		if (orCreateFactionRecords != null)
		{
			orCreateFactionRecords.Clear();
			for (int i = 0; i < messages.Length; i++)
			{
				orCreateFactionRecords.Add(messages[i]);
			}
		}
	}

	private bool AddNewFactionRadioRecord(FactionType type, string[] messages, double receivedAt)
	{
		List<FactionRadioRecord> orCreateFactionRecords = GetOrCreateFactionRecords(type);
		if (orCreateFactionRecords != null)
		{
			orCreateFactionRecords.Add(new FactionRadioRecord
			{
				Messages = messages,
				ReceivedAt = receivedAt
			});
			return true;
		}
		return false;
	}

	private void DisplayGaugeAndPlayMotion(float duration, string motionName)
	{
		TimerData.Timer timer = new TimerData.Timer("faction", duration);
		TimerData.Timer.Play<DefaultProgressGauge>(timer);
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().Motion(motionName);
		}
	}

	private void InspectFollowUpAct(ulong entityId, string motionName)
	{
		Connections.Frontend.Send(new InspectFollowUpAct
		{
			EntityId = entityId
		}).On(delegate(Messages.Timer timerMsg, PacketHeader _)
		{
			DisplayGaugeAndPlayMotion(timerMsg.Duration, motionName);
		});
	}
}
