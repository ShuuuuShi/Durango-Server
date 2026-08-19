using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class SelectableStateSync : MonoBehaviour
{
	[Serializable]
	private struct SyncInfo
	{
		public SyncType Type;

		public Selectable.State FakeState;
	}

	private enum SyncType
	{
		None,
		Sync,
		Fake
	}

	public Action<Selectable.State> StateChanged;

	[EnumList(typeof(Selectable.State), false, 0, -1)]
	[SerializeField]
	private SyncInfo[] _syncInfos;

	[SerializeField]
	private List<SelectableWidget> _targets;

	private bool _stateUpdated;

	public Selectable.State CurrentState { get; private set; }

	private void Awake()
	{
		CurrentState = Selectable.State.Normal;
		foreach (SelectableWidget target in _targets)
		{
			AttachEvents(target);
		}
	}

	public void AddTarget(GameObject target)
	{
		AddTarget(target.AddMissingComponent<SelectableWidget>());
	}

	public void AddTarget(SelectableWidget target)
	{
		if (!_targets.Contains(target))
		{
			AttachEvents(target);
			_targets.Add(target);
		}
	}

	public void RemoveTarget(GameObject target)
	{
		RemoveTarget(target.AddMissingComponent<SelectableWidget>());
	}

	public void RemoveTarget(SelectableWidget target)
	{
		if (_targets.Contains(target))
		{
			DetachEvents(target);
			_targets.Remove(target);
		}
	}

	public void AddTargets(IEnumerable<GameObject> targets)
	{
		foreach (GameObject target in targets)
		{
			AddTarget(target);
		}
	}

	public void AddTargets(IEnumerable<SelectableWidget> targets)
	{
		foreach (SelectableWidget target in targets)
		{
			AddTarget(target);
		}
	}

	public void RemoveTargets(IEnumerable<GameObject> targets)
	{
		foreach (GameObject target in targets)
		{
			RemoveTarget(target);
		}
	}

	public void RemoveTargets(IEnumerable<SelectableWidget> targets)
	{
		foreach (SelectableWidget target in targets)
		{
			RemoveTarget(target);
		}
	}

	public void ClearTargets()
	{
		for (int num = _targets.Count - 1; num >= 0; num--)
		{
			DetachEvents(_targets[num]);
		}
		_targets.Clear();
	}

	private Selectable.State GetState()
	{
		int num = 0;
		Selectable.State result = Selectable.State.Normal;
		IEnumerable<Selectable.State> enumerable = _targets.Select((SelectableWidget x) => x.GetState());
		foreach (Selectable.State item in enumerable)
		{
			Selectable.State state = item;
			int num2 = StateToSyncOrderValue(ref state);
			if (num < num2)
			{
				num = num2;
				result = state;
			}
		}
		return result;
	}

	private int StateToSyncOrderValue(ref Selectable.State state)
	{
		SyncInfo syncInfo = _syncInfos[(int)state];
		switch (syncInfo.Type)
		{
		case SyncType.Sync:
			return StateToOrderValue(state);
		case SyncType.Fake:
			if (_syncInfos[(int)syncInfo.FakeState].Type == SyncType.Sync)
			{
				state = syncInfo.FakeState;
				return StateToOrderValue(syncInfo.FakeState);
			}
			return 0;
		default:
			return 0;
		}
	}

	private static int StateToOrderValue(Selectable.State state)
	{
		return state switch
		{
			Selectable.State.Hovered => 1, 
			Selectable.State.Selected => 2, 
			Selectable.State.Pressed => 3, 
			Selectable.State.Disabled => 4, 
			_ => 0, 
		};
	}

	private void AttachEvents([NotNull] Selectable target)
	{
		target.StateUpdated = (Action<Selectable, Selectable.State>)Delegate.Combine(target.StateUpdated, new Action<Selectable, Selectable.State>(OnSelectableStateUpdated));
	}

	private void DetachEvents([NotNull] Selectable target)
	{
		target.StateUpdated = (Action<Selectable, Selectable.State>)Delegate.Remove(target.StateUpdated, new Action<Selectable, Selectable.State>(OnSelectableStateUpdated));
	}

	private void OnSelectableStateUpdated(Selectable selectable, Selectable.State state)
	{
		_stateUpdated = true;
	}

	private void LateUpdate()
	{
		if (!_stateUpdated)
		{
			return;
		}
		_stateUpdated = false;
		CurrentState = GetState();
		int num = StateToOrderValue(CurrentState);
		foreach (SelectableWidget target in _targets)
		{
			Selectable.State state = target.GetState();
			if (_syncInfos[(int)state].Type != SyncType.Sync && StateToOrderValue(state) > num)
			{
				target.HoldWidgetState(isHold: false);
			}
			else
			{
				target.HoldWidgetState(isHold: true, CurrentState);
			}
		}
		if (StateChanged != null)
		{
			StateChanged(CurrentState);
		}
	}
}
