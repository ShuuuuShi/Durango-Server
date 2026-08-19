using System.Collections.Generic;
using System.Runtime.InteropServices;
using Durango.Logic.InputSystem;
using JetBrains.Annotations;
using UnityEngine;

public class KeyCodeDictionary : Dictionary<KeySet, InputCommand>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct KeyCodeComparer : IEqualityComparer<KeySet>
	{
		public bool Equals(KeySet x, KeySet y)
		{
			if (x.Code == y.Code && x.Modifiers == y.Modifiers && x.Layers == y.Layers && x.Trigger == y.Trigger)
			{
				return x.IgnoreModifier == y.IgnoreModifier;
			}
			return false;
		}

		public int GetHashCode(KeySet obj)
		{
			return obj.GetHashCode();
		}
	}

	private Dictionary<InputCommand, List<KeySet>> _reverseMap = new Dictionary<InputCommand, List<KeySet>>();

	public InputCommand this[KeyCode a, Modifier mod = Modifier.None, Layer layer = Layer.Default, Trigger trigger = Trigger.Down]
	{
		get
		{
			if (!TryGetValue(new KeySet(a, mod, layer), out var value))
			{
				return InputCommand.None;
			}
			return value;
		}
		set
		{
			KeySet keySet = new KeySet(a, mod, layer, trigger);
			AddToReverseMap(value, keySet);
			base[keySet] = value;
		}
	}

	public InputCommand this[KeyCode a, Trigger trigger]
	{
		get
		{
			return this[a, Modifier.None, Layer.Default, trigger];
		}
		set
		{
			this[a, Modifier.None, Layer.Default, trigger] = value;
		}
	}

	public InputCommand this[KeyCode a, Layer layer, Trigger trigger = Trigger.Down]
	{
		get
		{
			return this[a, Modifier.None, layer, trigger];
		}
		set
		{
			this[a, Modifier.None, layer, trigger] = value;
		}
	}

	public KeyCodeDictionary()
		: base((IEqualityComparer<KeySet>)default(KeyCodeComparer))
	{
	}

	public new void Add(KeySet key, InputCommand value)
	{
		AddToReverseMap(value, key);
		base.Add(key, value);
	}

	public void SafeAdd(KeySet keySet, InputCommand command)
	{
		if (CheckSafe(keySet, command))
		{
			Add(keySet, command);
		}
	}

	public void SafeAdd(KeyCode keyCode, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode), command);
	}

	public void SafeAdd(KeyCode keyCode, Modifier modifier, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, modifier), command);
	}

	public void SafeAdd(KeyCode keyCode, Layer layer, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, Modifier.None, layer), command);
	}

	public void SafeAdd(KeyCode keyCode, Trigger trigger, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, Modifier.None, Layer.Default, trigger), command);
	}

	public void SafeAdd(KeyCode keyCode, Modifier modifier, Layer layer, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, modifier, layer), command);
	}

	public void SafeAdd(KeyCode keyCode, Layer layer, Trigger trigger, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, Modifier.None, layer, trigger), command);
	}

	public void SafeAddStream(KeyCode keyCode, Layer layer, InputCommand command)
	{
		SafeAdd(new KeySet(keyCode, Modifier.None, layer, Trigger.Stream), command);
	}

	public bool ContainsKey(KeyCode escape, Modifier modifier = Modifier.None)
	{
		return ContainsKey(new KeySet(escape, modifier));
	}

	[CanBeNull]
	public List<KeySet> GetKeySetList(InputCommand command)
	{
		if (_reverseMap.TryGetValue(command, out var value))
		{
			return value;
		}
		return null;
	}

	private void AddToReverseMap(InputCommand key, KeySet value)
	{
		if (!_reverseMap.TryGetValue(key, out var value2))
		{
			value2 = new List<KeySet>();
			_reverseMap.Add(key, value2);
		}
		KeyCodeComparer keyCodeComparer = default(KeyCodeComparer);
		if (value2.FindIndex((KeySet x) => keyCodeComparer.Equals(x, value)) < 0)
		{
			value2.Add(value);
		}
	}

	private bool CheckSafe(KeySet newKeySet, InputCommand command)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<KeySet, InputCommand> current = enumerator.Current;
				KeySet key = current.Key;
				if (key.Code == newKeySet.Code && key.Modifiers == newKeySet.Modifiers && (key.Layers & newKeySet.Layers) != 0 && (key.Trigger & newKeySet.Trigger) != 0)
				{
					_ = $"KeyCode SafeAdd Failed due to Key Conflict Between Command [{command}] and Command [{current.Value}] ";
					return false;
				}
			}
		}
		return true;
	}
}
