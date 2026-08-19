using System;
using System.Collections.Generic;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using Messages;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class ResearchSystem : GameSystem<ResearchSystem>
{
	private AsyncCachedDictionary<string, string[]> _availableClanResearchs;

	private AsyncCachedData<ClanResearchList> _clanResearchList;

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().PostTouched += OnPostTouched;
		_availableClanResearchs = new AsyncCachedDictionary<string, string[]>(RequestAvailabieClanResearchList);
		_clanResearchList = new AsyncCachedData<ClanResearchList>(RequestClanResearchList, 60f);
	}

	private void RequestAvailabieClanResearchList(string key, string[] cachedValue, Action<string, string[]> onResult)
	{
		Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(key);
		if (artifact == null)
		{
			onResult(key, null);
			return;
		}
		Connections.Frontend.Send(new GetAvailableClanResearch
		{
			EntityId = key,
			Tile = artifact.WorldTile
		}).On(delegate(AvailableClanResearch msg, PacketHeader header)
		{
			onResult(key, msg.AvailableResearchIds);
		});
	}

	private void RequestClanResearchList(ClanResearchList cachedValue, Action<ClanResearchList> onResult)
	{
		Connections.Frontend.Send(default(GetClanResearch)).On(delegate(ClanResearchList msg, PacketHeader header)
		{
			onResult(msg);
		});
	}

	public void GetClanResearchList([NotNull] Action<ClanResearchList> result, bool ignoreCache)
	{
		if (!PlayerBehavior.LocalPlayer.HasClan)
		{
			result(default(ClanResearchList));
		}
		else
		{
			_clanResearchList.Request(result, ignoreCache);
		}
	}

	private void OnPostTouched(InteractionMenuList menuList, InteractionObject target)
	{
		int num = menuList.IndexOf(Interaction.ClanResearch);
		if (num == -1)
		{
			return;
		}
		menuList.RemoveAt(num);
		Artifact artifact = target?.GetTargetComponent<Artifact>();
		if (artifact == null)
		{
			return;
		}
		int touchedFrame = Time.frameCount;
		InteractionObject touchedTarget = target;
		_availableClanResearchs.Request(artifact.EntityId, delegate(string[] researchList)
		{
			if (GameSystem<InteractionSystem>.Instance().Target == touchedTarget)
			{
				int size = KUtility.GetSize(researchList);
				if (size != 0)
				{
					for (int i = 0; i < size; i++)
					{
						string text = researchList[i];
						Yaml.ClanResearch clanResearch = SingletonDict<string, Yaml.ClanResearch>.Get(text);
						if (clanResearch != null)
						{
							InteractionMenuData data = new InteractionMenuData(Interaction.ClanResearch)
							{
								Id = text,
								Name = clanResearch.Name
							};
							if (!string.IsNullOrEmpty(clanResearch.Icon))
							{
								data.Icon = clanResearch.Icon;
							}
							data.Duration = (float)clanResearch.Duration;
							menuList.Add(data);
						}
					}
					int researchListFrame = Time.frameCount;
					GetClanResearchList(delegate(ClanResearchList list)
					{
						if (GameSystem<InteractionSystem>.Instance().Target == touchedTarget)
						{
							string entityId = touchedTarget.EntityId;
							string text2 = null;
							double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
							int j = 0;
							for (int size2 = KUtility.GetSize(list.ResearchList); j < size2; j++)
							{
								Messages.ClanResearch clanResearch2 = list.ResearchList[j];
								if (!(clanResearch2.LabEntityId != entityId) && (predictedServerTime < clanResearch2.Until || predictedServerTime < clanResearch2.CooltimeUntil))
								{
									text2 = clanResearch2.ResearchId;
									break;
								}
							}
							if (!string.IsNullOrEmpty(text2))
							{
								for (int num2 = menuList.Count - 1; num2 >= 0; num2--)
								{
									if (menuList[num2].Action == Interaction.ClanResearch && menuList[num2].Id != text2)
									{
										menuList.RemoveAt(num2);
									}
								}
							}
							int k = 0;
							for (int size3 = KUtility.GetSize(list.ResearchList); k < size3; k++)
							{
								Messages.ClanResearch clanResearch3 = list.ResearchList[k];
								int num3 = menuList.IndexOf(Interaction.ClanResearch, clanResearch3.ResearchId);
								if (num3 != -1)
								{
									Yaml.ClanResearch clanResearch4 = SingletonDict<string, Yaml.ClanResearch>.Instance.Get(clanResearch3.ResearchId);
									if (clanResearch4 != null)
									{
										float time = Time.time;
										float num4;
										float since;
										bool disabled;
										if (predictedServerTime < clanResearch3.Until)
										{
											num4 = time + (float)(clanResearch3.Until - predictedServerTime);
											since = num4 - (float)clanResearch4.Duration;
											disabled = false;
										}
										else
										{
											if (!(predictedServerTime < clanResearch3.CooltimeUntil))
											{
												continue;
											}
											since = time + (float)(clanResearch3.Until - predictedServerTime);
											num4 = time + (float)(clanResearch3.CooltimeUntil - predictedServerTime);
											disabled = true;
										}
										InteractionMenuData value = menuList[num3];
										value.Disabled = disabled;
										value.SetTimer(new Durango.Logic.Timer.Timer(since, num4, InterruptCondition.None));
										menuList[num3] = value;
									}
								}
							}
							if (researchListFrame != Time.frameCount)
							{
								if (!string.IsNullOrEmpty(text2))
								{
									menuList.ResetAndDontClear();
								}
								menuList.Apply();
							}
						}
					}, ignoreCache: false);
					if (touchedFrame != researchListFrame)
					{
						menuList.ResetAndDontClear();
						menuList.Apply();
					}
				}
			}
		});
	}

	public void StartClanResearch(string id, Point2 tile, string researchId)
	{
		_clanResearchList.MarkAsDirty();
		Connections.Frontend.Send(new StartClanResearch
		{
			EntityId = id,
			Tile = tile,
			Id = researchId
		});
	}

	public static void GetAvailablePersonalResearch(PropKey prop, [NotNull] Action<AvailablePersonalResearch?> onResult)
	{
		Connections.Frontend.Send(new GetAvailablePersonalResearch
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile
		}).On(delegate(AvailablePersonalResearch msg, PacketHeader header)
		{
			onResult(msg);
		}).Rest(delegate
		{
			onResult(null);
		});
	}

	public static void StartPersonalResearch(PropKey prop, string id, Action<bool> onResult)
	{
		Connections.Frontend.Send(new StartPersonalResearch
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile,
			ResearchId = id
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static string GetCurrentPersonalResearch(ResearchCategory category)
	{
		StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
		foreach (KeyValuePair<string, PersonalResearch> item in SingletonDict<string, PersonalResearch>.Instance)
		{
			PersonalResearch value = item.Value;
			if (value.Category == category && !string.IsNullOrEmpty(value.Effect.StatusEffectId))
			{
				StatusEffect statusEffect = statusEffects.GetStatusEffect(value.Effect.StatusEffectId, value.Effect.Level);
				if (statusEffect != null)
				{
					return item.Key;
				}
			}
		}
		return null;
	}
}
