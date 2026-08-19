using System;
using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;
using InteractionData;
using UnityEngine;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorInteraction : Locator
{
	private static readonly List<GameObject> SearchBuffer = new List<GameObject>();

	private InteractionGroup _interactionGroup;

	private readonly Predicate<GameObject> _filter;

	private readonly bool _movable;

	private string _entityId;

	private int[] _entityTypes;

	private Interaction _action;

	private string[] _arguments;

	private float _nextSearchTime;

	public LocatorInteraction(Predicate<GameObject> filter = null, bool movable = false)
	{
		_filter = filter;
		_movable = movable;
	}

	protected override void OnInitialized()
	{
		_interactionGroup = UIManager.FindScript<InteractionGroup>();
		Parameter parameter = Parameters.Get("interaction");
		if (parameter != null)
		{
			_entityId = parameter.id;
			if (!string.IsNullOrEmpty(parameter.param))
			{
				string[] array = parameter.param.Split('|');
				_entityTypes = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					_entityTypes[i] = array[i].ToInt();
				}
			}
		}
		Parameter parameter2 = Parameters.Get("interaction_button");
		if (parameter2 == null)
		{
			return;
		}
		_action = parameter2.id.ToEnum(Interaction.None);
		if (!string.IsNullOrEmpty(parameter2.param))
		{
			_arguments = parameter2.param.Split('|');
			for (int j = 0; j < _arguments.Length; j++)
			{
				_arguments[j] = _arguments[j].Trim();
			}
		}
	}

	protected override string SelectPhase()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && ((!string.IsNullOrEmpty(_entityId) && target.EntityId == _entityId) || (_entityTypes != null && _entityTypes.Contains(target.EntityType))))
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
			SearchBuffer.Clear();
			if (_movable)
			{
				InteractionSystem.SearchMovableObjects(SearchBuffer);
			}
			else
			{
				InteractionSystem.SearchPropObjects(SearchBuffer);
			}
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < SearchBuffer.Count; i++)
			{
				bool flag = true;
				GameObject obj = SearchBuffer[i];
				if (!string.IsNullOrEmpty(_entityId))
				{
					string entityId = ObjectIdentifier.GetEntityId(obj);
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
				base.TargetTransform = SearchBuffer[num2].transform;
			}
			SearchBuffer.Clear();
			_nextSearchTime = Time.time + 1f;
			break;
		}
		case "interaction_button":
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase = _interactionGroup.InteractionMenu.FindMenu(_action, _arguments);
			if (interactionMenuWidgetBase != null)
			{
				base.TargetTransform = interactionMenuWidgetBase.transform;
			}
			break;
		}
		}
	}
}
