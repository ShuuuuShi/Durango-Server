using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class AnimalCheatWidget : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _clearButton;

	[SerializeField]
	private IntSelector _intSelector;

	[SerializeField]
	private KGridScrollView _animalScrollView;

	[SerializeField]
	private KScrollView _animalInfoScrollView;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private SelectableButton _spawnButton;

	[SerializeField]
	private RectLayout _layout;

	private int _currentAnimalEntityTypeId;

	private ToggleWidget _herdIdToggle;

	private void Start()
	{
		SelectableButton spawnButton = _spawnButton;
		spawnButton.Clicked = (Action)Delegate.Combine(spawnButton.Clicked, new Action(SpawnAnimal));
		_spawnButton.Text = "소환";
		_layout.UpdateOnSizeChange();
		UpdateAnimals(string.Empty);
		EventDelegate.Set(_searchInput.onSubmit, delegate
		{
			UpdateAnimals(_searchInput.value);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_clearButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UpdateAnimals(null);
		});
		_intSelector.Set(60, 1, 60);
		_animalInfoScrollView.Nodes.BeginLoad();
		GameObject next = _animalInfoScrollView.Nodes.GetNext();
		next.GetComponent<KeyValueLabel>().SetKey("허드 ID");
		_herdIdToggle = next.GetComponent<ToggleWidget>();
		_herdIdToggle.SetOptions(new string[7] { "단순", "01", "02", "05", "10", "20", "30" });
		_animalInfoScrollView.Nodes.EndLoad();
	}

	private void UpdateAnimals(string keyword)
	{
		_animalScrollView.gameObject.SetActive(value: true);
		_animalScrollView.Nodes.Clear();
		_animalInfoScrollView.Nodes.Clear();
		foreach (KeyValuePair<int, Animal> item in SingletonDict<int, Animal>.Instance)
		{
			string text = item.Value.Name.ToString();
			if (string.IsNullOrEmpty(keyword) || text.Contains(keyword))
			{
				SelectableWidget selectableWidget = _animalScrollView.Nodes.Add<SelectableWidget>();
				selectableWidget.Clicked = Refresh;
				selectableWidget.transform.Find("Icon").GetComponent<UISprite>().spriteName = item.Value.Portrait;
				selectableWidget.transform.Find("Name").GetComponent<UILabel>().text = text;
				selectableWidget.transform.Find("Id").GetComponent<UILabel>().text = item.Key.ToString();
			}
		}
		_animalScrollView.ResetPosition();
	}

	private void Refresh()
	{
		int num = _animalScrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		for (int i = 0; i < _animalScrollView.Nodes.Count; i++)
		{
			bool selected = i == num;
			_animalScrollView.Nodes[i].GetComponent<Selectable>().Selected = selected;
		}
		_currentAnimalEntityTypeId = int.Parse(Selectable.Current.transform.Find("Id").GetComponent<UILabel>().text);
		_title.text = AnimalYaml.GetName(_currentAnimalEntityTypeId);
		_animalInfoScrollView.ResetPosition();
	}

	private void SpawnAnimal()
	{
		if (_herdIdToggle.Index == 0)
		{
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = $"spawn animal {_currentAnimalEntityTypeId} {_intSelector.Value}"
			});
		}
		else
		{
			string text = _herdIdToggle.Text.text;
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = $"spawn animal herd {_currentAnimalEntityTypeId}{text}"
			});
		}
	}
}
