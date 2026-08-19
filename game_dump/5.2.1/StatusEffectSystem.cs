using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Shop;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

public class StatusEffectSystem : GameSystem<StatusEffectSystem>
{
	private readonly Dictionary<string, Durango.Logic.StatusEffects> _statusEffects = new Dictionary<string, Durango.Logic.StatusEffects>();

	private readonly List<KeyValuePair<float, Messages.StatusEffects>> _statusEffectsQueue = new List<KeyValuePair<float, Messages.StatusEffects>>();

	public event Action<Durango.Logic.StatusEffects> StatusEffectsUpdated;

	public event Action<string, string> StatusEffectAdded;

	public event Action<string, string> StatusEffectRemoved;

	private void Awake()
	{
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetStatusEffects));
		});
		Connections.Frontend.On<Messages.StatusEffects>(OnStatusEffects);
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Singleton<PetManager>.Instance().PetDisappeared += delegate(AnimalBehavior animal)
			{
				RemoveEffects(animal.EntityId);
			};
			Singleton<ArtifactManager>.Instance().Removed += delegate(Artifact artifact)
			{
				RemoveEffects(artifact.EntityId);
			};
			Singleton<AnimalManager>.Instance().AnimalDisappeared += delegate(AnimalBehavior animal)
			{
				RemoveEffects(animal.EntityId);
			};
			Singleton<PlayerManager>.Instance().PlayerDisappeared += delegate(PlayerBehavior player)
			{
				RemoveEffects(player.EntityId);
			};
		};
	}

	private void Update()
	{
		float time = Time.time;
		for (int num = _statusEffectsQueue.Count - 1; num >= 0; num--)
		{
			if (_statusEffectsQueue[num].Key < time)
			{
				SetStatusEffects(_statusEffectsQueue[num].Value);
				_statusEffectsQueue.RemoveAt(num);
			}
		}
	}

	private void RemoveEffects(string entityid)
	{
		_statusEffects.Remove(entityid);
		for (int num = _statusEffectsQueue.Count - 1; num >= 0; num--)
		{
			if (_statusEffectsQueue[num].Value.EntityId == entityid)
			{
				_statusEffectsQueue.RemoveAt(num);
			}
		}
	}

	[NotNull]
	private Durango.Logic.StatusEffects MakeOrGet(string entityId)
	{
		if (!_statusEffects.TryGetValue(entityId, out var value))
		{
			value = new Durango.Logic.StatusEffects(entityId);
			_statusEffects[entityId] = value;
			value.StatusEffectsUpdated += OnUpdateStatusEffects;
			value.StatusEffectAdded += OnAddStatusEffect;
			value.StatusEffectRemoved += OnRemoveStatusEffect;
		}
		return value;
	}

	private void OnUpdateStatusEffects(Durango.Logic.StatusEffects effects)
	{
		if (this.StatusEffectsUpdated != null)
		{
			this.StatusEffectsUpdated(effects);
		}
	}

	private void OnAddStatusEffect(string entityId, string id)
	{
		if (this.StatusEffectAdded != null)
		{
			this.StatusEffectAdded(entityId, id);
		}
	}

	private void OnRemoveStatusEffect(string entityId, string id)
	{
		if (this.StatusEffectRemoved != null)
		{
			this.StatusEffectRemoved(entityId, id);
		}
	}

	private void OnStatusEffects(Messages.StatusEffects msg, PacketHeader header)
	{
		if (PlayerBehavior.LocalPlayer.EntityId == msg.EntityId)
		{
			SetStatusEffects(msg);
			return;
		}
		float num = (float)(header.Time - Connections.Frontend.GetBufferedServerTime());
		_statusEffectsQueue.Add(new KeyValuePair<float, Messages.StatusEffects>(Time.time + num, msg));
	}

	private void SetStatusEffects(Messages.StatusEffects msg)
	{
		MakeOrGet(msg.EntityId).SetStatusEffects(msg);
	}

	[NotNull]
	public Durango.Logic.StatusEffects GetStatusEffects()
	{
		return MakeOrGet(GameManager.PlayerId);
	}

	[CanBeNull]
	public Durango.Logic.StatusEffects GetStatusEffects(string entityId)
	{
		if (string.IsNullOrEmpty(entityId))
		{
			return null;
		}
		return _statusEffects.Get(entityId);
	}

	public Durango.Logic.StatusEffect GetStatusEffect(string id, int? level = null)
	{
		return GetStatusEffects().GetStatusEffect(id, level);
	}

	public Durango.Logic.StatusEffect GetStatusEffect(string entityId, string id, int? level = null)
	{
		return GetStatusEffects(entityId)?.GetStatusEffect(id, level);
	}

	[CanBeNull]
	public Durango.Logic.StatusEffect GetStatusEffectFromCommodity(string commodityId)
	{
		Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(commodityId);
		if (commodity == null)
		{
			return null;
		}
		if (KUtility.GetSize(commodity.Contents.StatusEffects) > 0)
		{
			return GetStatusEffect(commodity.Contents.StatusEffects[0].status_effects_id);
		}
		return null;
	}
}
