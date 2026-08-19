using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class InputMessageDispatcher
{
	public class InputMessageDictionary : Dictionary<InputCommand, List<Action<InputCommandMessage>>>
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct CommandComparer : IEqualityComparer<InputCommand>
		{
			public bool Equals(InputCommand x, InputCommand y)
			{
				return x == y;
			}

			public int GetHashCode(InputCommand x)
			{
				return (int)x;
			}
		}

		public InputMessageDictionary()
			: base((IEqualityComparer<InputCommand>)default(CommandComparer))
		{
		}
	}

	private readonly InputMessageDictionary _handlers = new InputMessageDictionary();

	private bool _stopPropagation;

	public void RegisterHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)
	{
		if (!_handlers.ContainsKey(inputCommand))
		{
			InitCommandHandler(inputCommand);
		}
		AddHandler(inputCommand, callback, priority);
	}

	public void UnregisterHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)
	{
		RemoveHandler(inputCommand, callback, priority);
	}

	public void Dispatch(InputCommand key, InputCommandMessage message)
	{
		_stopPropagation = false;
		if (!_handlers.TryGetValue(key, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (_stopPropagation)
			{
				break;
			}
			if (value[i] != null)
			{
				value[i](message);
			}
		}
	}

	public void StopPropagation()
	{
		_stopPropagation = true;
	}

	private void InitCommandHandler(InputCommand inputCommand)
	{
		_handlers[inputCommand] = new List<Action<InputCommandMessage>>();
		Array values = Enum.GetValues(typeof(InputSystem.Priority));
		for (int i = 0; i < values.Length; i++)
		{
			_handlers[inputCommand].Add(null);
		}
	}

	private void AddHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)
	{
		List<Action<InputCommandMessage>> list;
		int index;
		(list = _handlers[inputCommand])[index = (int)priority] = (Action<InputCommandMessage>)Delegate.Combine(list[index], callback);
	}

	private void RemoveHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority)
	{
		if (_handlers.ContainsKey(inputCommand))
		{
			List<Action<InputCommandMessage>> list;
			int index;
			(list = _handlers[inputCommand])[index = (int)priority] = (Action<InputCommandMessage>)Delegate.Remove(list[index], callback);
		}
	}
}
