using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.Logic.Combat;

public class DamageableEntities
{
	private const float SearchPeriod = 1f;

	public Action Updated;

	private bool _enabled;

	private float _combatObjectUpdateAt;

	private readonly List<GameObject> _combatObjects = new List<GameObject>();

	private readonly Dictionary<string, DamageableEntity> _buffer = new Dictionary<string, DamageableEntity>();

	private readonly Dictionary<string, DamageableEntity> _enemies = new Dictionary<string, DamageableEntity>();

	private readonly Dictionary<string, DamageableEntity> _allies = new Dictionary<string, DamageableEntity>();

	private DamageableEntity _player;

	public void Update()
	{
		if (_enabled && _combatObjectUpdateAt < Time.time)
		{
			Refresh();
		}
	}

	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		if (!_enabled)
		{
			_combatObjects.Clear();
			_buffer.Clear();
			_enemies.Clear();
			_allies.Clear();
		}
	}

	[CanBeNull]
	public DamageableEntity Find(string id)
	{
		bool isAlly;
		return Get(id, out isAlly, make: false);
	}

	[CanBeNull]
	public DamageableEntity Get(string id)
	{
		bool isAlly;
		return Get(id, out isAlly, make: true);
	}

	[CanBeNull]
	public DamageableEntity Find(string id, out bool isAlly)
	{
		return Get(id, out isAlly, make: false);
	}

	[CanBeNull]
	public DamageableEntity Get(string id, out bool isAlly)
	{
		return Get(id, out isAlly, make: true);
	}

	[CanBeNull]
	private DamageableEntity Get(string id, out bool isAlly, bool make)
	{
		if (id == null)
		{
			isAlly = false;
			return null;
		}
		if (id == GameManager.PlayerId)
		{
			if (make && _player == null)
			{
				_player = Create(PlayerBehavior.LocalPlayer.gameObject);
				_player.AddGaugeUpdateDelegate();
			}
			isAlly = true;
			return _player;
		}
		isAlly = false;
		if (!_enemies.TryGetValue(id, out var value))
		{
			if (_allies.TryGetValue(id, out value))
			{
				isAlly = true;
			}
			else if (make)
			{
				GameObject gameObject = Singleton<ObjectManager>.Instance().FindObject(id);
				if (gameObject != null)
				{
					value = Create(gameObject);
					((!ObjectIdentifier.IsAlly(gameObject)) ? _enemies : _allies).Add(id, value);
				}
			}
		}
		return value;
	}

	public IEnumerable<DamageableEntity> GetEnemies()
	{
		return _enemies.Values;
	}

	public IEnumerable<DamageableEntity> GetAllies()
	{
		return _allies.Values;
	}

	public bool HasEnemies()
	{
		return _enemies.Count > 0;
	}

	public bool HasAllies()
	{
		return _allies.Count > 0;
	}

	public void TargetChange(TargetChanged changed)
	{
		DamageableEntity damageableEntity = Find(changed.EntityId);
		if (damageableEntity != null)
		{
			damageableEntity.TargetId = changed.TargetId;
		}
	}

	public void ClearTargets()
	{
		foreach (DamageableEntity value in _enemies.Values)
		{
			if (value != null)
			{
				value.TargetId = null;
			}
		}
		foreach (DamageableEntity value2 in _allies.Values)
		{
			if (value2 != null)
			{
				value2.TargetId = null;
			}
		}
	}

	private void Refresh()
	{
		_combatObjectUpdateAt = Time.time + 1f;
		_combatObjects.Clear();
		InteractionSystem.SearchCombatTargetObjects(_combatObjects);
		_buffer.Clear();
		_buffer.AddRange(_enemies);
		_buffer.AddRange(_allies);
		_enemies.Clear();
		_allies.Clear();
		for (int i = 0; i < _combatObjects.Count; i++)
		{
			GameObject obj = _combatObjects[i];
			string entityId = ObjectIdentifier.GetEntityId(obj);
			Add((!ObjectIdentifier.IsAlly(obj)) ? _enemies : _allies, entityId, obj);
		}
		foreach (DamageableEntity value in _buffer.Values)
		{
			if (value != null)
			{
				value.RemoveGaugeUpdateDelegate();
			}
		}
		_buffer.Clear();
		_combatObjects.Clear();
		if (Updated != null)
		{
			Updated();
		}
	}

	private void Add(Dictionary<string, DamageableEntity> container, string id, GameObject obj)
	{
		if (id == null)
		{
			return;
		}
		if (_buffer.TryGetValue(id, out var value))
		{
			_buffer.Remove(id);
		}
		if (!container.ContainsKey(id))
		{
			if (value == null)
			{
				value = Create(obj);
				value.AddGaugeUpdateDelegate();
			}
			container.Add(id, value);
		}
	}

	private static DamageableEntity Create(GameObject obj)
	{
		CharacterBehavior component = obj.GetComponent<CharacterBehavior>();
		if (component != null)
		{
			PetAI component2 = obj.GetComponent<PetAI>();
			if (component2 != null)
			{
				return new PetDamageableEntity(component, component2);
			}
			return new CharacterDamageableEntity(component);
		}
		Artifact component3 = obj.GetComponent<Artifact>();
		if (component3 != null)
		{
			return new ArtifactDamageableEntity(component3);
		}
		return null;
	}
}
