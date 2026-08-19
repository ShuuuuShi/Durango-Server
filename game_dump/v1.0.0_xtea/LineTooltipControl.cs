using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using MsgPack;
using UnityEngine;

public class LineTooltipControl : TooltipBase
{
	private struct LineData
	{
		public string Key;

		public string Value;

		public LineData[] Children;

		public int ChildCount => (Children != null) ? Children.Length : 0;
	}

	private struct LineStruct
	{
		public string Key;

		public string Value;

		public int ChildCount;

		public bool HideChild;

		public LineStruct(string key, string value, int childCount = 0)
		{
			Key = key;
			Value = value;
			ChildCount = childCount;
			HideChild = true;
		}
	}

	private static bool _includeProperty;

	private static bool _includePrimitive;

	private static bool _includeStatic;

	private static HashSet<int> _recursionCheck = new HashSet<int>();

	private static readonly Type[] NodeTypes = new Type[7]
	{
		typeof(string),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Point2),
		typeof(Color),
		typeof(Gauge)
	};

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UISprite _titleBG;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private ListObjectPool _lineItems;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private float _key_value_min_margin;

	[SerializeField]
	private string _defaultPlusMacker;

	[SerializeField]
	private string _defaultMinusMacker;

	private string _valueDefaultColor;

	private int _lineVPadding;

	private int _lineHPadding;

	private string _title;

	private List<LineStruct> _lines;

	public int MaxWidth { get; set; }

	protected override void OnAwake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		LineTooltipItem component = _lineItems.BaseObject.GetComponent<LineTooltipItem>();
		UISpriteLabel keyLabel = component.KeyLabel;
		keyLabel.Label.color = Color.white;
		UISpriteLabel valueLabel = component.ValueLabel;
		_valueDefaultColor = UIManager.ColorBBCode(valueLabel.Label.color);
		valueLabel.Label.color = Color.white;
		_lineVPadding = component.Widget.height - keyLabel.Label.height;
		_lineHPadding = component.Widget.width - (int)Mathf.Abs(((Component)keyLabel).transform.localPosition.x) * 2;
		_lineItems.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = OnClick_LineItem;
			uIEventListener.onDrag = OnDrag_LineItem;
			uIEventListener.onPress = OnPress_LineItem;
		});
	}

	private void OnClick_LineItem(GameObject go)
	{
		LineTooltipItem component = go.GetComponent<LineTooltipItem>();
		int index = component.Index;
		if (index != -1 && _lines[index].ChildCount > 0)
		{
			LineStruct value = _lines[index];
			value.HideChild = !value.HideChild;
			_lines[index] = value;
			Refresh();
			HideArrow();
		}
	}

	private void OnDrag_LineItem(GameObject go, Vector2 delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		OnDrag(delta);
	}

	private void OnPress_LineItem(GameObject go, bool press)
	{
		OnPress(press);
	}

	public void Set(string title, string comment)
	{
		_title = title;
		if (!string.IsNullOrEmpty(comment))
		{
			_lines = new List<LineStruct>();
			LineStruct item = new LineStruct(comment, string.Empty);
			_lines.Add(item);
		}
	}

	public void Set(string title, IList<string> keys, IList<string> values)
	{
		int num = keys?.Count ?? 0;
		int num2 = values?.Count ?? 0;
		int num3 = Mathf.Max(num, num2);
		LineData[] array = new LineData[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i].Key = ((i >= num) ? string.Empty : keys[i]);
			array[i].Value = ((i >= num2) ? string.Empty : values[i]);
			array[i].Children = null;
		}
		Set(title, array);
	}

	private void Set(string title, [NotNull] IList<LineData> dataList)
	{
		_title = title;
		int count = dataList.Count;
		Stack<LineData> stack = new Stack<LineData>();
		for (int num = count - 1; num >= 0; num--)
		{
			stack.Push(dataList[num]);
		}
		List<LineStruct> list = new List<LineStruct>();
		LineStruct item = default(LineStruct);
		while (stack.Count > 0)
		{
			LineData lineData = stack.Pop();
			item.Key = lineData.Key;
			item.Value = lineData.Value;
			item.ChildCount = lineData.ChildCount;
			item.HideChild = true;
			list.Add(item);
			for (int num2 = item.ChildCount - 1; num2 >= 0; num2--)
			{
				stack.Push(lineData.Children[num2]);
			}
		}
		_lines = list;
	}

	private LineData LineStructToDetailLine(ref int index)
	{
		LineData result = default(LineData);
		result.Key = _lines[index].Key;
		result.Value = _lines[index].Value;
		int childCount = _lines[index].ChildCount;
		result.Children = ((childCount != 0) ? new LineData[childCount] : null);
		index++;
		for (int i = 0; i < childCount; i++)
		{
			ref LineData reference = ref result.Children[i];
			reference = LineStructToDetailLine(ref index);
		}
		return result;
	}

	protected override void FillData()
	{
		((Component)_titleWidget).gameObject.SetActive(false);
		_titleLabel.text = _title;
		if (_lines == null)
		{
			return;
		}
		int count = _lines.Count;
		_lineItems.Clear();
		for (int i = 0; i < count; i++)
		{
			string key = _lines[i].Key;
			string text = _lines[i].Value;
			if (string.IsNullOrEmpty(text) && _lines[i].ChildCount > 0)
			{
				string arg = ((!_lines[i].HideChild) ? _defaultMinusMacker : _defaultPlusMacker);
				text = $"[{arg}]";
			}
			LineTooltipItem lineTooltipItem = ((ListObjectPoolBase<GameObject>)_lineItems).Add<LineTooltipItem>();
			lineTooltipItem.KeyLabel.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
			lineTooltipItem.ValueLabel.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
			lineTooltipItem.Key = string.Format("{1}{0}[-]", key, _valueDefaultColor);
			lineTooltipItem.Value = string.Format("{1}{0}[-]", text, _valueDefaultColor);
			lineTooltipItem.Index = i;
			if (!_lines[i].HideChild)
			{
				continue;
			}
			int num;
			for (num = _lines[i].ChildCount; num > 0; num += _lines[i].ChildCount)
			{
				i++;
				if (i >= count)
				{
					break;
				}
				num--;
			}
		}
	}

	protected override void UpdateLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(((Component)_titleWidget).transform.localPosition.y);
		float num2 = Mathf.Abs(((Component)_titleLabel).transform.localPosition.y);
		float num3 = _titleLabel.Label.printedSize.x;
		int i = 0;
		for (int count = _lineItems.Count; i < count; i++)
		{
			LineTooltipItem component = _lineItems[i].GetComponent<LineTooltipItem>();
			float x = component.KeyLabel.Label.printedSize.x;
			float x2 = component.ValueLabel.Label.printedSize.x;
			float num4 = 0f;
			float num5 = x + x2;
			if (x > 0f && x2 > 0f)
			{
				num4 = _key_value_min_margin;
			}
			num5 += num4;
			if (MaxWidth > 0 && num5 > (float)MaxWidth)
			{
				num5 = MaxWidth;
				component.KeyLabel.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
				component.ValueLabel.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
				component.KeyLabel.Label.width = (int)((num5 - num4) * x / (x + x2));
				component.ValueLabel.Label.width = (int)((num5 - num4) * x2 / (x + x2));
				component.KeyLabel.Label.ProcessText();
				component.ValueLabel.Label.ProcessText();
			}
			num3 = Mathf.Max(num3, num5);
		}
		num3 += num * 2f + num2 * 2f;
		base.Widget.width = Mathf.Max((int)num3, _minWidth);
		float num6 = 0f;
		int width = (int)((float)base.Widget.width - num * 2f);
		Vector3 val;
		if (string.IsNullOrEmpty(_titleLabel.text))
		{
			((Component)_titleWidget).gameObject.SetActive(false);
			val = ((Component)_titleWidget).transform.localPosition;
		}
		else
		{
			((Component)_titleWidget).gameObject.SetActive(true);
			_titleWidget.width = width;
			float num7 = _titleLabel.Label.printedSize.y + num2 * 2f;
			_titleWidget.height = (int)num7;
			_titleLabel.Label.UpdateAnchors();
			_titleBG.UpdateAnchors();
			val = ((Component)_titleWidget).transform.localPosition + Vector3.down * num7;
			num6 = num * 2f + (float)_titleWidget.height;
		}
		int num8 = 0;
		int j = 0;
		for (int count2 = _lineItems.Count; j < count2; j++)
		{
			LineTooltipItem component2 = _lineItems[j].GetComponent<LineTooltipItem>();
			int num9 = Mathf.Max(component2.KeyLabel.Label.height, component2.ValueLabel.Label.height) + _lineVPadding;
			component2.Widget.height = num9;
			((Component)component2).transform.localPosition = val + Vector3.down * (float)num8;
			num8 += num9;
			component2.LineActive(active: true);
			component2.Widget.width = width;
			component2.UpdateLayout(_lineHPadding);
		}
		if (_lineItems.Count > 0)
		{
			LineTooltipItem component3 = _lineItems[_lineItems.Count - 1].GetComponent<LineTooltipItem>();
			component3.LineActive(active: false);
			num6 = Mathf.Abs(val.y) + (float)num8 + num;
		}
		base.Widget.height = (int)num6;
		NGUITools.UpdateWidgetCollider(((Component)base.Widget).gameObject);
		_bg.UpdateAnchors();
	}

	protected override void OnClickWidget()
	{
		VisibleTimeReset();
	}

	protected override void OnFinish()
	{
		MaxWidth = 0;
		base.OnFinish();
	}

	public void SetObject([NotNull] object obj, bool visiblePrimitive = false, bool visibleStatic = false, bool visibleProperty = false)
	{
		_title = obj.GetType().ToString();
		_includePrimitive = visiblePrimitive;
		_includeStatic = visibleStatic;
		_includeProperty = visibleProperty;
		List<LineStruct> list = new List<LineStruct>();
		ObjectToLines(list, obj);
		StringBuilder stringBuilder = new StringBuilder();
		List<int> list2 = new List<int>();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			int count2 = list2.Count;
			if (count2 > 0)
			{
				List<int> list3;
				List<int> list4 = (list3 = list2);
				int index;
				int index2 = (index = count2 - 1);
				index = list3[index];
				list4[index2] = index - 1;
				LineStruct value = list[i];
				stringBuilder.Remove(0, stringBuilder.Length);
				for (int j = 0; j < count2; j++)
				{
					stringBuilder.Append("  ");
				}
				stringBuilder.Append(value.Key);
				value.Key = stringBuilder.ToString();
				list[i] = value;
			}
			int childCount = list[i].ChildCount;
			if (childCount > 0)
			{
				list2.Add(childCount);
			}
			int num = list2.Count - 1;
			while (num >= 0 && list2[num] == 0)
			{
				list2.RemoveAt(num);
				num--;
			}
		}
		_lines = list;
		_recursionCheck.Clear();
	}

	private static int ObjectToLines(List<LineStruct> lines, object obj)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (IsNodeObject(obj))
		{
			lines.Add(new LineStruct(string.Empty, NodeObjectToString(obj)));
			return 1;
		}
		int hashCode = obj.GetHashCode();
		if (_recursionCheck.Contains(hashCode))
		{
			lines.Add(new LineStruct(string.Empty, obj.ToString()));
			return 1;
		}
		_recursionCheck.Add(hashCode);
		if (obj is IDictionary dict)
		{
			return DictToLines(lines, dict);
		}
		if (obj is IList list)
		{
			return ListToLines(lines, list);
		}
		if (obj is MessagePackObject val)
		{
			if (((MessagePackObject)(ref val)).IsDictionary)
			{
				return DictToLines(lines, (IDictionary)((MessagePackObject)(ref val)).AsDictionary());
			}
			if (((MessagePackObject)(ref val)).IsList || ((MessagePackObject)(ref val)).IsArray)
			{
				return ListToLines(lines, new List<MessagePackObject>(((MessagePackObject)(ref val)).AsList()));
			}
			lines.Add(new LineStruct(string.Empty, ((MessagePackObject)(ref val)).ToString()));
			return 1;
		}
		Type type = obj.GetType();
		if (type.IsGenericType && (object)type.GetGenericTypeDefinition() == typeof(KeyValuePair<, >))
		{
			return KeyValueToLines(lines, obj);
		}
		return ClassToLines(lines, obj);
	}

	private static int ClassToLines(List<LineStruct> lines, object obj)
	{
		Type type = obj.GetType();
		FieldInfo[] array = ((!_includePrimitive) ? type.GetFields() : type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		for (int i = 0; i < array.Length; i++)
		{
			FieldInfo fieldInfo = array[i];
			if (_includeStatic || (fieldInfo.Attributes & FieldAttributes.Static) == 0)
			{
				dictionary.Add(array[i].Name, array[i].GetValue(obj));
			}
		}
		if (_includeProperty)
		{
			PropertyInfo[] properties = type.GetProperties();
			for (int j = 0; j < properties.Length; j++)
			{
				MethodInfo getMethod = properties[j].GetGetMethod();
				if ((object)getMethod != null && (getMethod.Attributes & MethodAttributes.Static) == 0 && getMethod.GetParameters().Length <= 0)
				{
					try
					{
						dictionary.Add(properties[j].Name, getMethod.Invoke(obj, null));
					}
					catch (Exception ex)
					{
						dictionary.Add(properties[j].Name, ex.Message);
					}
				}
			}
		}
		return DictToLines(lines, dictionary);
	}

	private static int DictToLines(List<LineStruct> lines, IDictionary dict)
	{
		int num = 0;
		foreach (object item in dict)
		{
			num += KeyValueToLines(lines, item);
		}
		return num;
	}

	private static int KeyValueToLines(List<LineStruct> lines, object obj)
	{
		object value = WatchDocs.GetValue("Key", obj);
		object value2 = WatchDocs.GetValue("Value", obj);
		if (IsNodeObject(value))
		{
			if (IsNodeObject(value2))
			{
				lines.Add(new LineStruct(NodeObjectToString(value), NodeObjectToString(value2)));
			}
			else
			{
				LineStruct value3 = new LineStruct(NodeObjectToString(value), string.Empty);
				int count = lines.Count;
				lines.Add(default(LineStruct));
				value3.ChildCount = ObjectToLines(lines, value2);
				lines[count] = value3;
			}
			return 1;
		}
		LineStruct lineStruct = new LineStruct("Key", string.Empty);
		lines.Add(lineStruct);
		int count2 = lines.Count;
		lines.Add(default(LineStruct));
		lineStruct.ChildCount = ObjectToLines(lines, value);
		lines[count2] = lineStruct;
		if (IsNodeObject(value2))
		{
			lines.Add(new LineStruct("Value", NodeObjectToString(value2)));
		}
		else
		{
			LineStruct lineStruct2 = new LineStruct("Value", string.Empty);
			lines.Add(lineStruct2);
			lineStruct2.ChildCount = ObjectToLines(lines, value2);
			lines.Add(lineStruct2);
			int count3 = lines.Count;
			lines.Add(default(LineStruct));
			lineStruct2.ChildCount = ObjectToLines(lines, value2);
			lines[count3] = lineStruct2;
		}
		return 2;
	}

	private static int ListToLines(List<LineStruct> lines, IList list)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (IsNodeObject(list[i]))
			{
				lines.Add(new LineStruct(i.ToString(), NodeObjectToString(list[i])));
				continue;
			}
			LineStruct value = new LineStruct(i.ToString(), string.Empty);
			int count2 = lines.Count;
			lines.Add(default(LineStruct));
			value.ChildCount = ObjectToLines(lines, list[i]);
			lines[count2] = value;
		}
		return count;
	}

	private static bool IsNodeObject(object obj)
	{
		return obj == null || IsNodeType(obj.GetType());
	}

	private static bool IsNodeType(Type type)
	{
		return type.IsPrimitive || type.IsEnum || Array.IndexOf(NodeTypes, type) != -1;
	}

	private static string NodeObjectToString(object obj)
	{
		return (obj != null) ? obj.ToString() : "null";
	}
}
