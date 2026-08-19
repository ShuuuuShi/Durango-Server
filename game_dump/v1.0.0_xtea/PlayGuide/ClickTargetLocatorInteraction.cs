using System;
using System.Collections.Generic;
using Shared.System;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorInteraction : ClickTargetLocator
{
	private static List<GameObject> _searchBuffer = new List<GameObject>();

	private readonly InteractionGroup _interactionGroup;

	private readonly Predicate<GameObject> _filter;

	private ulong _entityId;

	private int[] _entityTypes;

	private Interaction _action;

	private string _argument;

	private float _nextSearchTime;

	public ClickTargetLocatorInteraction(Predicate<GameObject> filter = null)
	{
		_filter = filter;
		_interactionGroup = UIManager.FindScript<InteractionGroup>();
	}

	protected override void OnInitialized()
	{
		ClickTargetData clickTargetData = ClickTargetDict.Get("interaction");
		if (clickTargetData != null)
		{
			_entityId = (ulong)clickTargetData.id.ToInt();
			if (!string.IsNullOrEmpty(clickTargetData.param))
			{
				string[] array = clickTargetData.param.Split('|');
				_entityTypes = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					_entityTypes[i] = array[i].ToInt();
				}
			}
		}
		ClickTargetData clickTargetData2 = ClickTargetDict.Get("interaction_button");
		if (clickTargetData2 != null)
		{
			_action = clickTargetData2.id.ToEnum(Interaction.None);
			_argument = clickTargetData2.param;
		}
	}

	protected override string SelectPhase()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && ((_entityId != 0L && target.EntityId == _entityId) || (_entityTypes != null && _entityTypes.Contains(target.EntityType))))
		{
			return "interaction_button";
		}
		return "interaction";
	}

	protected override void UpdateTargetTransform()
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			base.TargetTransform = null;
			return;
		}
		switch (base.CurrentPhase)
		{
		case "interaction":
		{
			if (_nextSearchTime > Time.time)
			{
				break;
			}
			_searchBuffer.Clear();
			InteractionSystem.SearchPropObjects(_searchBuffer);
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < _searchBuffer.Count; i++)
			{
				bool flag = true;
				GameObject obj = _searchBuffer[i];
				if (_entityId != 0L)
				{
					ulong entityId = ObjectIdentifier.GetEntityId(obj);
					flag &= _entityId == entityId;
				}
				if (_entityTypes != null && _entityTypes.Length > 0)
				{
					int entityType = ObjectIdentifier.GetEntityType(obj);
					flag &= _entityTypes.Contains(entityType);
				}
				if (_filter != null)
				{
					flag &= _filter(obj);
				}
				if (flag)
				{
					float distance = InteractionObject.GetDistance(obj);
					if (distance < num)
					{
						num2 = i;
						num = distance;
					}
				}
			}
			if (num2 != -1)
			{
				base.TargetTransform = _searchBuffer[num2].transform;
			}
			_searchBuffer.Clear();
			_nextSearchTime = Time.time + 1f;
			break;
		}
		case "interaction_button":
		{
			InteractionMenu interactionMenu = _interactionGroup.InteractionMenu.FindMenu(_action, _argument);
			if ((Object)(object)interactionMenu != (Object)null)
			{
				base.TargetTransform = ((Component)interactionMenu).transform;
			}
			break;
		}
		}
	}
}
