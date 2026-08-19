using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MarketCheatWidget : MonoBehaviour
{
	[SerializeField]
	private KGridScrollView _productScrollView;

	[SerializeField]
	private KScrollView _optionScrollView;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private SelectableButton _applyButton;

	[SerializeField]
	private RectLayout _layout;

	private int _targetIndex;

	private void Start()
	{
		SelectableButton applyButton = _applyButton;
		applyButton.Clicked = (Action)Delegate.Combine(applyButton.Clicked, new Action(ApplyOption));
		_applyButton.Text = "적용";
		_layout.UpdateOnSizeChange();
		_title.text = "만료 기한";
		_optionScrollView.Nodes.BeginLoad();
		AddTimeOption("일", 30);
		AddTimeOption("시", 23);
		AddTimeOption("분", 59);
		AddTimeOption("초", 59);
		_optionScrollView.Nodes.EndLoad();
		_optionScrollView.Nodes[3].GetComponent<ToggleWidget>().MoveIndex(1);
		_optionScrollView.ResetPosition();
	}

	private void OnEnable()
	{
		_productScrollView.Nodes.Clear();
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = "list products"
		}).On(delegate(Info msg, PacketHeader header)
		{
			if (!string.IsNullOrEmpty(msg.Text))
			{
				List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
				string[] array = msg.Text.Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new char[1] { ' ' }, 3);
					KeyValuePair<string, string> item = new KeyValuePair<string, string>(array2[1], array2[2]);
					list.Add(item);
				}
				UpdateProducts(list);
			}
		});
		_targetIndex = -1;
	}

	private void UpdateProducts(List<KeyValuePair<string, string>> products)
	{
		_productScrollView.gameObject.SetActive(value: true);
		_productScrollView.Nodes.Clear();
		foreach (KeyValuePair<string, string> product in products)
		{
			SelectableWidget selectableWidget = _productScrollView.Nodes.Add<SelectableWidget>();
			selectableWidget.Clicked = OnSelectProduct;
			selectableWidget.transform.Find("Id").GetComponent<UILabel>().text = product.Key;
			selectableWidget.transform.Find("Name").GetComponent<UILabel>().text = product.Value;
		}
		_productScrollView.ResetPosition();
	}

	private void OnSelectProduct()
	{
		_targetIndex = _productScrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		for (int i = 0; i < _productScrollView.Nodes.Count; i++)
		{
			bool selected = i == _targetIndex;
			_productScrollView.Nodes[i].GetComponent<Selectable>().Selected = selected;
		}
	}

	private void AddTimeOption(string description, int end)
	{
		string[] array = new string[end + 1];
		for (int i = 0; i <= end; i++)
		{
			array[i] = i.ToString();
		}
		AddOption(description, array);
	}

	private void AddOption(string description, string[] options)
	{
		GameObject next = _optionScrollView.Nodes.GetNext();
		next.GetComponent<KeyValueLabel>().SetKey(description);
		next.GetComponent<ToggleWidget>().SetOptions(options);
	}

	private void ApplyOption()
	{
		if (_targetIndex != -1)
		{
			int index = _optionScrollView.Nodes[0].GetComponent<ToggleWidget>().Index;
			int index2 = _optionScrollView.Nodes[1].GetComponent<ToggleWidget>().Index;
			int index3 = _optionScrollView.Nodes[2].GetComponent<ToggleWidget>().Index;
			int index4 = _optionScrollView.Nodes[3].GetComponent<ToggleWidget>().Index;
			int num = ((index * 24 + index2) * 60 + index3) * 60 + index4;
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = $"expire product {_targetIndex} {num}"
			});
		}
	}
}
