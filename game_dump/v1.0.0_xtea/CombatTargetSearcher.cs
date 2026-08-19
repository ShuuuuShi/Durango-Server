using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CombatTargetSearcher
{
	private readonly HashSet<GameObject> _searchedTargets = new HashSet<GameObject>();

	private readonly HashSet<GameObject> _holdTargets = new HashSet<GameObject>();

	private readonly List<DamageableEntity> _targetEntities = new List<DamageableEntity>();

	public int Count => _targetEntities.Count + ((AdditionalTargetEntity != null) ? 1 : 0);

	public IList<DamageableEntity> TargetEntities => _targetEntities;

	public DamageableEntity AdditionalTargetEntity { get; private set; }

	public bool Contains(GameObject targetObject)
	{
		for (int i = 0; i < _targetEntities.Count; i++)
		{
			if ((Object)(object)_targetEntities[i].GameObject == (Object)(object)targetObject)
			{
				return true;
			}
		}
		if (AdditionalTargetEntity != null && (Object)(object)AdditionalTargetEntity.GameObject == (Object)(object)targetObject)
		{
			return true;
		}
		return false;
	}

	public void Clear()
	{
		for (int i = 0; i < _targetEntities.Count; i++)
		{
			_targetEntities[i].RemoveLifeGaugeUpdateDelegate();
		}
		_targetEntities.Clear();
		RemoveAdditionalTargetEntity();
	}

	public void SearchTargets(float checkDistance)
	{
		if ((Object)(object)PlayerBehavior.LocalPlayer == (Object)null)
		{
			return;
		}
		InteractionSystem.SearchCombatTargetObjects(_searchedTargets, checkDistance);
		_holdTargets.Clear();
		for (int num = _targetEntities.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _targetEntities[num].GameObject;
			if (_searchedTargets.Contains(gameObject))
			{
				_holdTargets.Add(gameObject);
			}
			else
			{
				_targetEntities[num].RemoveLifeGaugeUpdateDelegate();
				_targetEntities.RemoveAt(num);
			}
		}
		HashSet<GameObject>.Enumerator enumerator = _searchedTargets.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (!_holdTargets.Contains(current))
			{
				DamageableEntity damageableEntity = DamageableEntity.Create(current);
				if (damageableEntity != null)
				{
					damageableEntity.AddLifeGaugeUpdateDelegate();
					_targetEntities.Add(damageableEntity);
				}
			}
		}
	}

	public bool CreateAdditionalTargetEntity([NotNull] GameObject externalTarget)
	{
		if (AdditionalTargetEntity != null && (Object)(object)AdditionalTargetEntity.GameObject == (Object)(object)externalTarget)
		{
			return true;
		}
		RemoveAdditionalTargetEntity();
		DamageableEntity damageableEntity = DamageableEntity.Create(externalTarget);
		if (damageableEntity != null)
		{
			damageableEntity.AddLifeGaugeUpdateDelegate();
			AdditionalTargetEntity = damageableEntity;
			return true;
		}
		return false;
	}

	public void RemoveAdditionalTargetEntity()
	{
		if (AdditionalTargetEntity != null)
		{
			AdditionalTargetEntity.RemoveLifeGaugeUpdateDelegate();
			AdditionalTargetEntity = null;
		}
	}
}
