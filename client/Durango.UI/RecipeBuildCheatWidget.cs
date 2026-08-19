using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Building;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class RecipeBuildCheatWidget : MonoBehaviour
{
	public struct CheatArguments
	{
		public Building.Blueprint Blueprint;

		public List<string> Looks;

		public Point2 Size;

		public Rotation Rotation;

		public Point2 Tile;

		public int? Floor;
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _descriptionWidget;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _cheatOptions;

	[SerializeField]
	private SelectableButton _nextButton;

	private Building.Blueprint _blueprint;

	private CheatArguments _lastCheatArguments;

	private void Start()
	{
		_nextButton.Text = T._("건설");
		SelectableButton nextButton = _nextButton;
		nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, new Action(BuildClicked));
	}

	public void Show(Building.Blueprint blueprint)
	{
		base.gameObject.SetActive(value: true);
		_blueprint = blueprint;
		if (string.IsNullOrEmpty(blueprint.Description))
		{
			_descriptionWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_descriptionWidget.gameObject.SetActive(value: true);
			_descriptionLabel.text = blueprint.Description;
			_descriptionWidget.height = (int)_descriptionLabel.printedSize.y + 36;
		}
		_cheatOptions.BeginLoad();
		_titleLabel.text = blueprint.Name;
		if (blueprint.IsSizeVariable)
		{
			AddSizeOptionButton(blueprint.Size.x, T._("X 크기"));
			AddSizeOptionButton(blueprint.Size.y, T._("Y 크기"));
		}
		if (blueprint.IsLookChangeable())
		{
			for (int i = 0; i < blueprint.Slots.Length; i++)
			{
				Building.BlueprintSlot blueprintSlot = blueprint.Slots[i];
				if (!blueprintSlot.HasLook || blueprintSlot.Looks.Count <= 1)
				{
					continue;
				}
				string[] array = new string[blueprintSlot.Looks.Count];
				int num = 0;
				foreach (KeyValuePair<string, ArtifactLook> look in blueprintSlot.Looks)
				{
					string key = look.Key;
					Gettext gettext = look.Value.name;
					if (key == "default")
					{
						gettext = T._("(기본) ") + gettext;
					}
					array[num++] = gettext;
				}
				AddOption(blueprintSlot.Name, array);
			}
		}
		_cheatOptions.EndLoad();
		_scrollView.Widgets.Clear();
		_scrollView.Widgets.Add(_descriptionWidget);
		foreach (GameObject cheatOption in _cheatOptions)
		{
			_scrollView.Widgets.Add(cheatOption.GetComponent<UIWidget>());
		}
		_scrollView.ResetPosition();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void AddOption(string description, string[] options)
	{
		GameObject next = _cheatOptions.GetNext();
		next.GetComponent<KeyValueLabel>().SetKey(description);
		next.GetComponent<ToggleWidget>().SetOptions(options);
	}

	private void AddSizeOptionButton(int size, string description)
	{
		string[] array = new string[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = (i + 1).ToString();
		}
		AddOption(description, array);
	}

	private void BuildClicked()
	{
		Building.Blueprint blueprint = _blueprint;
		int num = 0;
		Point2 size = blueprint.Size;
		if (blueprint.IsSizeVariable)
		{
			GameObject gameObject = _cheatOptions[num++];
			GameObject gameObject2 = _cheatOptions[num++];
			size = new Point2(int.Parse(gameObject.GetComponent<ToggleWidget>().Text.text), int.Parse(gameObject2.GetComponent<ToggleWidget>().Text.text));
		}
		List<string> looks = new List<string>();
		for (int i = 0; i < blueprint.Slots.Length; i++)
		{
			Building.BlueprintSlot blueprintSlot = blueprint.Slots[i];
			if (blueprintSlot.HasLook)
			{
				int index = 0;
				if (blueprintSlot.Looks.Count > 1)
				{
					index = _cheatOptions[num++].GetComponent<ToggleWidget>().Index;
				}
				KeyValuePair<string, ArtifactLook> keyValuePair = blueprintSlot.Looks.ElementAt(index);
				looks.Add(blueprintSlot.Id + ":" + keyValuePair.Key);
			}
		}
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		menuListGroupBase.SetLastOpenUri(blueprint.Icon, "Recipe/LastBuildCheat");
		_lastCheatArguments.Blueprint = blueprint;
		_lastCheatArguments.Size = size;
		_lastCheatArguments.Looks = looks;
		UIManager.FindScript<BuildGridGroupBase>().Open(blueprint, size, null, hasRoof: true, null, delegate(BuildSystem.GridResult result)
		{
			Point2 tile = new Point2(result.Tile.x, result.Tile.y);
			OnSubmit(new CheatArguments
			{
				Blueprint = blueprint,
				Looks = looks,
				Size = size,
				Rotation = result.Rotation,
				Tile = tile,
				Floor = result.Floor
			});
		});
	}

	public void SelectLastCheatBuildGrid()
	{
		CheatArguments lastCheatArguments = _lastCheatArguments;
		Building.Blueprint blueprint = lastCheatArguments.Blueprint;
		if (blueprint != null)
		{
			Point2 size = lastCheatArguments.Size;
			List<string> looks = lastCheatArguments.Looks;
			UIManager.FindScript<BuildGridGroupBase>().Open(blueprint, size, null, hasRoof: true, null, delegate(BuildSystem.GridResult result)
			{
				Point2 tile = new Point2(result.Tile.x, result.Tile.y);
				OnSubmit(new CheatArguments
				{
					Blueprint = blueprint,
					Looks = looks,
					Size = size,
					Rotation = result.Rotation,
					Tile = tile,
					Floor = result.Floor
				});
			});
		}
	}

	private void OnSubmit(CheatArguments arguments)
	{
		_lastCheatArguments = arguments;
		if (arguments.Blueprint == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (arguments.Blueprint.IsNatural)
		{
			stringBuilder.Append("natural ");
			stringBuilder.AppendFormat(" {0} {1}", arguments.Tile.x, arguments.Tile.y);
			stringBuilder.Append(" ");
			stringBuilder.Append(arguments.Blueprint.EntityType);
		}
		else
		{
			stringBuilder.Append("immortal ");
			stringBuilder.Append("prop ").Append(arguments.Blueprint.EntityType.ToString());
			if (arguments.Looks != null)
			{
				for (int i = 0; i < arguments.Looks.Count; i++)
				{
					stringBuilder.Append(" ").Append(arguments.Looks[i]);
				}
			}
			StringBuilder stringBuilder2 = stringBuilder.Append(" rotation:");
			int rotation = (int)arguments.Rotation;
			stringBuilder2.Append(rotation.ToString());
			stringBuilder.Append(" position:").Append($"{arguments.Tile.x},{arguments.Tile.y}");
			stringBuilder.Append(" size:").Append($"{arguments.Size.x},{arguments.Size.y}");
			if (arguments.Floor.HasValue)
			{
				stringBuilder.Append(" floor:").Append(arguments.Floor.Value);
			}
			stringBuilder.Append(" level:").Append(60);
		}
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder.ToString()
		});
	}
}
