using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class AnimalInfo : DiscoveryInfo
{
	[SerializeField]
	private TweenerPlayer _selector;

	private readonly List<Pair<string, bool>> _animalNames = new List<Pair<string, bool>>();

	private static readonly Dictionary<ushort, bool> UnknownAnimals = new Dictionary<ushort, bool>
	{
		{ 0, false },
		{ 1, false },
		{ 2, false }
	};

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_selector.gameObject.SetActive(value: false);
			_nodes.Init(delegate(GameObject obj)
			{
				UIEventListener uIEventListener = UIEventListener.Get(obj);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickAnimal));
			});
		}
	}

	private void OnClickAnimal(GameObject obj)
	{
		int num = _nodes.IndexOf(obj);
		if (num != -1)
		{
			string title = string.Format("[size=24]{0}[/size]", (!_animalNames[num].Item2) ? T._("아직 찾지 못한 동물입니다.") : ("<em>" + _animalNames[num].Item1 + "</em>"));
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(title, null);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(obj, Vector2.zero, 60f);
			widgetTooltipControl.AddOnFinished(OnAnimalTooltipHide);
			UIWidget component = _selector.GetComponent<UIWidget>();
			component.SetAnchor(obj, 0, 0, 0, 0);
			component.gameObject.SetActive(value: true);
			_selector.Play();
		}
	}

	private void OnAnimalTooltipHide()
	{
		_selector.gameObject.SetActive(value: false);
	}

	public override void ShowUnknown()
	{
		Set(UnknownAnimals);
	}

	public void Set([NotNull] Dictionary<ushort, bool> animalTypes)
	{
		Init();
		_animalNames.Clear();
		int size = KUtility.GetSize(animalTypes);
		if (size == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int num = 0;
		_nodes.BeginLoad();
		foreach (KeyValuePair<ushort, bool> animalType in animalTypes)
		{
			GameObject next = _nodes.GetNext();
			UISprite component = next.transform.Find("Portrait").GetComponent<UISprite>();
			UISprite component2 = next.transform.Find("Bg").GetComponent<UISprite>();
			Transform transform = next.transform.Find("Unknown");
			Transform transform2 = next.transform.Find("Portrait/Tamable");
			string item = string.Empty;
			if (animalType.Value)
			{
				Animal animal = SingletonDict<int, Animal>.Get(animalType.Key);
				if (animal != null)
				{
					item = animal.Name.ToString();
					component.gameObject.SetActive(value: true);
					component.spriteName = animal.Portrait;
					transform2.gameObject.SetActive(animal.Tamable);
				}
				else
				{
					component.gameObject.SetActive(value: false);
				}
				component2.color = new Color32(149, 100, 100, 60);
				transform.gameObject.SetActive(value: false);
				num++;
			}
			else
			{
				component.gameObject.SetActive(value: false);
				component2.color = new Color(0f, 0f, 0f, 0.5f);
				transform.gameObject.SetActive(value: true);
			}
			_animalNames.Add(new Pair<string, bool>(item, animalType.Value));
		}
		_nodes.EndLoad();
		string countLabel = $"<em>{num}</em>/{size}";
		SetCountLabel(countLabel);
		UIWidget component3 = GetComponent<UIWidget>();
		Vector3 basePos = _nodesWidget.localCorners[1] + new Vector3(15f, -20f);
		Vector2 localSize = _nodes.BaseObject.GetComponent<UIWidget>().localSize;
		Vector2 vector = UIUtility.WidgetsGridReposition(_nodes, null, Vector3.down, basePos, (float)component3.width - 30f, localSize, 5f, 5f);
		_nodesWidget.height = (int)vector.y + 40;
		_layout.UpdateLayout(component3.width, 0f);
		base.gameObject.SetActive(value: true);
	}
}
