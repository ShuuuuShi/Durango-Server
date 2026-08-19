using System;
using System.Collections.Generic;
using System.Reflection;
using Homans.Console;
using UnityEngine;

internal class ConsoleWatch : MonoBehaviour
{
	private class Watch
	{
		public string name;

		public FieldInfo field;

		public PropertyInfo property;

		public WeakReference instance;

		public string lastValue;
	}

	private List<Watch> watches = new List<Watch>();

	private void Start()
	{
		Console.Instance.RegisterCommand("AddWatch", (object)this, "AddWatchCommand");
		((MonoBehaviour)this).InvokeRepeating("UpdateWatches", 1f, 1f);
	}

	private void UpdateWatches()
	{
		watches.RemoveAll((Watch m) => m.instance.Target == null);
		foreach (Watch watch in watches)
		{
			if ((object)watch.field != null)
			{
				watch.lastValue = watch.field.GetValue(watch.instance.Target).ToString();
			}
			else if ((object)watch.property != null)
			{
				watch.lastValue = watch.property.GetValue(watch.instance.Target, null).ToString();
			}
		}
	}

	private void OnGUI()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (watches.Count != 0)
		{
			GUILayout.BeginArea(new Rect((float)(Screen.width - 300), 0f, 300f, (float)Screen.height));
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			for (int i = 0; i < watches.Count; i++)
			{
				Watch watch = watches[i];
				GUILayout.Label(watch.name + ": " + watch.lastValue, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}

	public void AddWatchField(string name, string fieldName, object instance)
	{
		Watch watch = new Watch();
		watch.name = name;
		watch.instance = new WeakReference(instance, trackResurrection: false);
		watch.field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (instance != null && (object)watch.field != null)
		{
			watches.Add(watch);
		}
	}

	public void AddWatchProperty(string name, string fieldName, object instance)
	{
		Watch watch = new Watch();
		watch.name = name;
		watch.instance = new WeakReference(instance, trackResurrection: false);
		watch.property = instance.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (instance != null && (object)watch.property != null)
		{
			watches.Add(watch);
		}
	}

	[Help("Usage: \"AddWatch name object.component.field\"\nDisplays the given field or property on the screen. Will automaticly update.")]
	private void AddWatchCommand(string name, string goPath)
	{
		string[] array = default(string[]);
		string text = default(string);
		string text2 = default(string);
		Console.parseGameObjectString(goPath, ref array, ref text, ref text2);
		string text3 = string.Empty;
		string[] array2 = array;
		foreach (string text4 in array2)
		{
			text3 = text3 + "/" + text4;
		}
		GameObject val = GameObject.Find(text3);
		if ((Object)(object)val == (Object)null)
		{
			Console.Instance.Print("Unknown gameobject");
			return;
		}
		Component component = val.GetComponent(text);
		if ((Object)(object)component == (Object)null)
		{
			Console.Instance.Print("Unknown component");
			return;
		}
		FieldInfo field = ((object)component).GetType().GetField(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)field == null)
		{
			PropertyInfo property = ((object)component).GetType().GetProperty(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property == null)
			{
				Console.Instance.Print("Unknown field or property");
			}
			else
			{
				AddWatchProperty(name, text2, component);
			}
		}
		else
		{
			AddWatchField(name, text2, component);
		}
	}
}
