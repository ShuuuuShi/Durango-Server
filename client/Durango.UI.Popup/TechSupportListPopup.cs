using System.Collections.Generic;
using System.Text;
using Crafting;
using Durango.UI.Control;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class TechSupportListPopup : TooltipBase
{
	[SerializeField]
	private KScrollView _listView;

	private bool _isFillData;

	protected override void OnEnable()
	{
		base.OnEnable();
		DragLock = true;
	}

	protected override void FillData()
	{
		if (_isFillData)
		{
			return;
		}
		_isFillData = true;
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		foreach (KeyValuePair<string, ReformTechSupport> item in SingletonDict<string, ReformTechSupport>.Instance)
		{
			if (item.Value.Tags == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, ReformTechSupportTag> tag in item.Value.Tags)
			{
				if (!dictionary.TryGetValue(tag.Key, out var value))
				{
					value = new HashSet<string>();
					dictionary[tag.Key] = value;
				}
				value.Add(item.Key);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		_listView.Nodes.BeginLoad();
		foreach (KeyValuePair<string, HashSet<string>> item2 in dictionary)
		{
			KeyValueLabel component = _listView.Nodes.GetNext().GetComponent<KeyValueLabel>();
			component.SetKey($"<tag>{item2.Key}</tag>");
			stringBuilder.Length = 0;
			foreach (string item3 in item2.Value)
			{
				Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(item3);
				string arg = ((recipe != null) ? recipe.Name : item3);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.AppendFormat("<ref>ui://Recipe/Crafting/{1},{0}</ref>", arg, item3);
			}
			component.SetValue(stringBuilder.ToString());
		}
		_listView.Nodes.EndLoad();
	}

	protected override void UpdateLayout()
	{
		int safeHeight = UIManager.SafeHeight;
		safeHeight = Mathf.Min(740, safeHeight - 120);
		GetComponent<RectLayoutComponent>().UpdateLayout(700f, safeHeight);
		UIUtility.UpdateAnchors(base.transform);
		foreach (GameObject node in _listView.Nodes)
		{
			node.GetComponent<RectLayoutComponent>().UpdateLayout();
		}
		_listView.ResetPosition();
	}
}
