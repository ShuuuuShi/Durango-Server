using System;
using System.Collections.Generic;

public abstract class InputDispatcher<T> where T : InputCommandInternalMessageBase, new()
{
	private List<Action<T>> _orderedCallbacks = new List<Action<T>>();

	private static T _cachedMessage;

	protected InputDispatcher()
	{
		Array values = Enum.GetValues(typeof(InputSystem.Priority));
		_orderedCallbacks.Capacity = values.Length;
		for (int i = 0; i < values.Length; i++)
		{
			_orderedCallbacks.Add(null);
		}
	}

	public virtual void RegisterHandler(Action<T> callback, InputSystem.Priority priority = InputSystem.Priority.Default)
	{
		List<Action<T>> orderedCallbacks;
		int index;
		(orderedCallbacks = _orderedCallbacks)[index = (int)priority] = (Action<T>)Delegate.Combine(orderedCallbacks[index], callback);
	}

	public void UnregisterHandler(Action<T> callback, InputSystem.Priority priority = InputSystem.Priority.Default)
	{
		if (_orderedCallbacks[(int)priority] != null)
		{
			List<Action<T>> orderedCallbacks;
			int index;
			(orderedCallbacks = _orderedCallbacks)[index = (int)priority] = (Action<T>)Delegate.Remove(orderedCallbacks[index], callback);
		}
	}

	public void Dispatch(T message)
	{
		if (message != null)
		{
			for (int i = 0; i < _orderedCallbacks.Count; i++)
			{
				_orderedCallbacks[i]?.Invoke(message);
			}
		}
	}

	protected static T GetCachedMessage()
	{
		if (_cachedMessage == null)
		{
			_cachedMessage = new T();
		}
		return _cachedMessage;
	}
}
