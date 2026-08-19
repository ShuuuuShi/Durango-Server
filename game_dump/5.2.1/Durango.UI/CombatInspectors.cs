using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class CombatInspectors : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private CombatInspector _inspectorBase;

	private CombatInspector _playerInspector;

	private ListObjectPool<CombatInspector> _enemyInspectors;

	private ListObjectPool<CombatInspector> _allyInspectors;

	private int _allyInspectorCount;

	private int _enemyInspectorCount;

	[SerializeField]
	[EnumList(typeof(CombatInspector.Type), false, 0, -1)]
	private List<CombatInspector.TypeLayout> _layouts;

	public List<CombatInspector.TypeLayout> Layouts => _layouts;

	void IUIInitializable.Init()
	{
		_enemyInspectors = new ListObjectPool<CombatInspector>();
		_allyInspectors = new ListObjectPool<CombatInspector>();
		_enemyInspectors.BaseObject = _inspectorBase;
		_allyInspectors.BaseObject = _inspectorBase;
		_playerInspector = UnityEngine.Object.Instantiate(_inspectorBase, _inspectorBase.transform.parent);
	}

	private void Start()
	{
		Observable<DamageableEntity> target = GameSystem<CombatSystem>.Instance().Target;
		target.Changed = (Action<DamageableEntity>)Delegate.Combine(target.Changed, new Action<DamageableEntity>(TargetChaged));
	}

	private void LateUpdate()
	{
		_playerInspector.Refresh();
		for (int i = 0; i < _allyInspectors.Count; i++)
		{
			if (_allyInspectors[i].Target == null)
			{
				int num = _allyInspectors.Count - 1;
				_allyInspectors.Swap(i, num);
				_allyInspectors.Set(num);
				i--;
			}
			else
			{
				_allyInspectors[i].Refresh();
			}
		}
		for (int j = 0; j < _enemyInspectors.Count; j++)
		{
			if (_enemyInspectors[j].Target == null)
			{
				int num2 = _enemyInspectors.Count - 1;
				_enemyInspectors.Swap(j, num2);
				_enemyInspectors.Set(num2);
				j--;
			}
			else
			{
				_enemyInspectors[j].Refresh();
			}
		}
	}

	public void Hide()
	{
		_enemyInspectors.Clear();
		_allyInspectors.Clear();
		base.gameObject.SetActive(value: false);
	}

	private void TargetChaged(DamageableEntity target)
	{
		string text = ((!(target == null)) ? target.GetEntityId() : null);
		for (int i = 0; i < _enemyInspectors.Count; i++)
		{
			bool select = _enemyInspectors[i].Target.GetEntityId() == text;
			_enemyInspectors[i].SetSelect(select);
		}
	}

	public void BeginSetting()
	{
		_allyInspectorCount = 0;
		_enemyInspectorCount = 0;
	}

	public void EndSetting()
	{
		_allyInspectors.Set(_allyInspectorCount);
		_enemyInspectors.Set(_enemyInspectorCount);
		_playerInspector.Set(GameSystem<CombatSystem>.Instance().DamageableEntities.Get(GameManager.PlayerId), isEnemy: false);
		_playerInspector.SetSelect(isSelect: false);
		for (int i = 0; i < _allyInspectors.Count; i++)
		{
			_allyInspectors[i].SetSelect(isSelect: false);
		}
		TargetChaged(GameSystem<CombatSystem>.Instance().Target.Value);
		base.gameObject.SetActive(value: true);
	}

	public void AddAlly(DamageableEntity entity)
	{
		AddInspector(entity, _allyInspectors, isEmeny: false, ref _allyInspectorCount);
	}

	public void AddEnemey(DamageableEntity entity)
	{
		AddInspector(entity, _enemyInspectors, isEmeny: true, ref _enemyInspectorCount);
	}

	private static void AddInspector(DamageableEntity entity, ListObjectPool<CombatInspector> inspectors, bool isEmeny, ref int count)
	{
		if (entity == null)
		{
			return;
		}
		int num = -1;
		string entityId = entity.GetEntityId();
		for (int i = 0; i < inspectors.Count; i++)
		{
			if (inspectors[i].Target != null && inspectors[i].Target.GetEntityId() == entityId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = inspectors.Count;
			inspectors.Add().Set(entity, isEmeny);
		}
		inspectors.Swap(num, count);
		count++;
	}
}
