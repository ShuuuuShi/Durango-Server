using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeyCodeCommandsDictionary : Dictionary<KeyCode, KeyCodeCommandsDictionary.Commands>
{
	public class Commands
	{
		public InputCommand DefaultCommand;

		public InputCommand CombatCommand;

		public void Set(InputCommandType type, InputCommand inputCommand)
		{
			switch (type)
			{
			case InputCommandType.Default:
				DefaultCommand = inputCommand;
				break;
			case InputCommandType.Combat:
				CombatCommand = inputCommand;
				break;
			}
		}

		public InputCommand Get(InputCommandType type)
		{
			InputCommand result = InputCommand.None;
			switch (type)
			{
			case InputCommandType.Default:
				result = DefaultCommand;
				break;
			case InputCommandType.Combat:
				result = CombatCommand;
				break;
			}
			return result;
		}

		public bool Has(InputCommandType type)
		{
			return Get(type) != InputCommand.None;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct KeyCodeComparer : IEqualityComparer<KeyCode>
	{
		public bool Equals(KeyCode x, KeyCode y)
		{
			return x == y;
		}

		public int GetHashCode(KeyCode x)
		{
			return (int)x;
		}
	}

	public KeyCodeCommandsDictionary()
		: base((IEqualityComparer<KeyCode>)default(KeyCodeComparer))
	{
	}

	public void AddCommand(KeyCode keyCode, InputCommandType type, InputCommand inputCommand)
	{
		if (!ContainsKey(keyCode))
		{
			Add(keyCode, new Commands());
		}
		base[keyCode].Set(type, inputCommand);
	}

	public bool HasCommand(KeyCode keyCode, InputCommandType inputCommandType)
	{
		if (ContainsKey(keyCode))
		{
			return base[keyCode].Has(inputCommandType);
		}
		return false;
	}
}
