using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Durango.Logic.InputSystem;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct KeySet : IEquatable<KeySet>
{
	public static readonly KeySet Invalid = new KeySet
	{
		Code = KeyCode.None
	};

	public Modifier Modifiers { get; private set; }

	public KeyCode Code { get; private set; }

	public Layer Layers { get; private set; }

	public Trigger Trigger { get; private set; }

	public bool IgnoreModifier { get; private set; }

	public KeySet(KeyCode code, Modifier modifiers = Modifier.None, Layer layer = Layer.Default, Trigger trigger = Trigger.Down, bool ignoreModifier = false)
	{
		this = default(KeySet);
		Code = code;
		Modifiers = modifiers;
		Layers = layer;
		Trigger = trigger;
		IgnoreModifier = ignoreModifier;
	}

	public KeySet(KeyCode code, bool ignoreModifier)
	{
		this = default(KeySet);
		Code = code;
		Modifiers = Modifier.None;
		Layers = Layer.Default;
		Trigger = Trigger.Down;
		IgnoreModifier = ignoreModifier;
	}

	public static KeySet CreateStream(KeyCode code, Modifier mod = Modifier.None, Layer layer = Layer.Default)
	{
		return new KeySet(code, mod, layer, Trigger.Stream);
	}

	public bool Equals(KeySet x, KeySet y)
	{
		return x.Code == y.Code && x.Modifiers == y.Modifiers && x.Layers == y.Layers && x.Trigger == y.Trigger && x.IgnoreModifier == y.IgnoreModifier;
	}

	public bool Equals(KeySet other)
	{
		return Equals(this, other);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		return obj is KeySet && Equals((KeySet)obj);
	}

	public static bool operator ==(KeySet c1, KeySet c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(KeySet c1, KeySet c2)
	{
		return !c1.Equals(c2);
	}

	public override int GetHashCode()
	{
		int modifiers = (int)Modifiers;
		modifiers = (modifiers * 397) ^ (int)Code;
		modifiers = (modifiers * 397) ^ (int)Layers;
		modifiers = (modifiers * 397) ^ (int)Trigger;
		return (modifiers * 397) ^ IgnoreModifier.GetHashCode();
	}

	public List<KeyCode> ToKeyCodes()
	{
		List<KeyCode> list = ModifiersToKeyCodes(Modifiers);
		if (!list.Contains(Code))
		{
			list.Add(Code);
		}
		return list;
	}

	public static List<KeyCode> ModifiersToKeyCodes(Modifier modifiers)
	{
		List<KeyCode> list = new List<KeyCode>();
		if ((modifiers & Modifier.LeftControl) != 0)
		{
			list.Add(KeyCode.LeftControl);
		}
		if ((modifiers & Modifier.RightControl) != 0)
		{
			list.Add(KeyCode.RightControl);
		}
		if ((modifiers & Modifier.LeftAlt) != 0)
		{
			list.Add(KeyCode.LeftAlt);
		}
		if ((modifiers & Modifier.RightAlt) != 0)
		{
			list.Add(KeyCode.RightAlt);
		}
		if ((modifiers & Modifier.LeftShift) != 0)
		{
			list.Add(KeyCode.LeftShift);
		}
		if ((modifiers & Modifier.RightShift) != 0)
		{
			list.Add(KeyCode.RightShift);
		}
		if ((modifiers & Modifier.LeftCommand) != 0)
		{
			list.Add(KeyCode.LeftCommand);
		}
		if ((modifiers & Modifier.RightCommand) != 0)
		{
			list.Add(KeyCode.RightCommand);
		}
		return list;
	}
}
