using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Building;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class BuildCheatWidget : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _clearButton;

	[SerializeField]
	private KGridScrollView _recipeScrollView;

	[SerializeField]
	private KScrollView _optionScrollView;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private IntSelector _levelSelector;

	[SerializeField]
	private SelectableButton _buildButton;

	[SerializeField]
	private RectLayout _layout;

	private List<Building.Blueprint> _blueprints;

	private int _selectedIndex;

	private void Start()
	{
		InitRecipeItems();
		_levelSelector.Set(60, 1, 60);
		SelectableButton buildButton = _buildButton;
		buildButton.Clicked = (Action)Delegate.Combine(buildButton.Clicked, new Action(BuildClicked));
		_buildButton.Text = "건설";
		_layout.UpdateOnSizeChange();
		EventDelegate.Set(_searchInput.onSubmit, delegate
		{
			ShowFilteredRecipes(_searchInput.value);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_clearButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			ShowFilteredRecipes(null);
		});
	}

	private void InitRecipeItems()
	{
		_recipeScrollView.gameObject.SetActive(value: true);
		_recipeScrollView.Nodes.Clear();
		_optionScrollView.Nodes.Clear();
		_blueprints = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetAllBlueprints();
		for (int i = 0; i < _blueprints.Count; i++)
		{
			Building.Blueprint blueprint = _blueprints[i];
			SelectableWidget selectableWidget = _recipeScrollView.Nodes.Add<SelectableWidget>();
			selectableWidget.Clicked = Recipe_OnSelectItem;
			selectableWidget.transform.Find("Icon").GetComponent<UISprite>().spriteName = ((!string.IsNullOrEmpty(blueprint.Icon)) ? blueprint.Icon : blueprint.ArtifactIcon);
			selectableWidget.transform.Find("Name").GetComponent<UILabel>().text = blueprint.Name;
		}
		_recipeScrollView.ResetPosition();
	}

	private void ShowFilteredRecipes(string text)
	{
		for (int i = 0; i < _blueprints.Count; i++)
		{
			bool active = string.IsNullOrEmpty(text) || _blueprints[i].Name.Contains(text);
			_recipeScrollView.Nodes[i].SetActive(active);
		}
		_recipeScrollView.ResetPosition();
	}

	private void Recipe_OnSelectItem()
	{
		SelectNode(_recipeScrollView.Nodes.IndexOf(Selectable.Current.gameObject));
	}

	private void SelectNode(int index)
	{
		_selectedIndex = index;
		for (int i = 0; i < _recipeScrollView.Nodes.Count; i++)
		{
			_recipeScrollView.Nodes[i].GetComponent<Selectable>().Selected = i == index;
		}
		Building.Blueprint blueprint = ((index != -1) ? _blueprints[index] : null);
		if (blueprint == null)
		{
			_optionScrollView.Nodes.Clear();
			_optionScrollView.ResetPosition();
			return;
		}
		_optionScrollView.Nodes.BeginLoad();
		_title.text = blueprint.Name;
		AddOption("내구도", new string[2] { "일반", "영구" });
		string[] array = new string[4];
		for (int j = 0; j < 4; j++)
		{
			int num = j + 1;
			if (j == 0)
			{
				array[j] = num.ToString();
			}
			else
			{
				array[j] = num + " X " + num;
			}
		}
		AddOption("개수", array);
		if (blueprint.IsSizeVariable)
		{
			AddSizeOptionButton(blueprint.Size.x, "X 크기");
			AddSizeOptionButton(blueprint.Size.y, "Y 크기");
			AddSizeOptionButton(5, "높이");
			AddOption("지붕", new string[2] { "있음", "없음" });
		}
		if (blueprint.IsLookChangeable())
		{
			for (int k = 0; k < blueprint.Slots.Length; k++)
			{
				Building.BlueprintSlot blueprintSlot = blueprint.Slots[k];
				if (!blueprintSlot.HasLook || blueprintSlot.Looks.Count <= 1)
				{
					continue;
				}
				string[] array2 = new string[blueprintSlot.Looks.Count];
				int num2 = 0;
				foreach (KeyValuePair<string, ArtifactLook> look in blueprintSlot.Looks)
				{
					string key = look.Key;
					Gettext gettext = look.Value.name;
					if (key == "default")
					{
						gettext = "(기본) " + gettext;
					}
					array2[num2++] = gettext;
				}
				AddOption(blueprintSlot.Name, array2);
			}
		}
		_optionScrollView.Nodes.EndLoad();
		_optionScrollView.ResetPosition();
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

	private void AddOption(string description, string[] options)
	{
		GameObject next = _optionScrollView.Nodes.GetNext();
		next.GetComponent<KeyValueLabel>().SetKey(description);
		next.GetComponent<ToggleWidget>().SetOptions(options);
	}

	private void BuildClicked()
	{
		Building.Blueprint blueprint = ((_selectedIndex != -1) ? _blueprints[_selectedIndex] : null);
		if (blueprint == null)
		{
			return;
		}
		int num = 0;
		bool isImmortal = _optionScrollView.Nodes[num++].GetComponent<ToggleWidget>().Index != 0;
		int num2 = _optionScrollView.Nodes[num++].GetComponent<ToggleWidget>().Index + 1;
		Point2 size = blueprint.Size;
		int? stories = null;
		bool hasRoof = true;
		if (blueprint.IsSizeVariable)
		{
			GameObject gameObject = _optionScrollView.Nodes[num++];
			GameObject gameObject2 = _optionScrollView.Nodes[num++];
			GameObject gameObject3 = _optionScrollView.Nodes[num++];
			GameObject obj = _optionScrollView.Nodes[num++];
			size = new Point2(int.Parse(gameObject.GetComponent<ToggleWidget>().Text.text), int.Parse(gameObject2.GetComponent<ToggleWidget>().Text.text));
			stories = int.Parse(gameObject3.GetComponent<ToggleWidget>().Text.text);
			hasRoof = obj.GetComponent<ToggleWidget>().Index == 0;
		}
		Point2 totalSize = size * num2;
		List<string> looks = new List<string>();
		for (int i = 0; i < blueprint.Slots.Length; i++)
		{
			Building.BlueprintSlot blueprintSlot = blueprint.Slots[i];
			if (blueprintSlot.HasLook && blueprintSlot.Looks.Count > 1)
			{
				int index = _optionScrollView.Nodes[num++].GetComponent<ToggleWidget>().Index;
				KeyValuePair<string, ArtifactLook> keyValuePair = blueprintSlot.Looks.ElementAt(index);
				looks.Add(blueprintSlot.Id + ":" + keyValuePair.Key);
			}
		}
		UIManager.FindScript<BuildGridGroupBase>().Open(blueprint, totalSize, stories, hasRoof, null, delegate(BuildSystem.GridResult result)
		{
			for (int j = 0; j < totalSize.x; j += size.x)
			{
				for (int k = 0; k < totalSize.y; k += size.y)
				{
					Point2 position = new Point2(result.Tile.x + j, result.Tile.y + k);
					CreateArtifact(result.Blueprint, looks, size, stories, result.Floor, result.Rotation, position, isImmortal, _levelSelector.Value.ToString());
				}
			}
		});
	}

	private void CreateArtifact(Building.Blueprint blueprint, IList<string> looks, Point2 size, int? stories, int? floor, Rotation rotation, Point2 position, bool isImmortal, string level = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (isImmortal)
		{
			stringBuilder.Append("immortal ");
		}
		stringBuilder.Append("prop ").Append(blueprint.EntityType.ToString());
		for (int i = 0; i < looks.Count; i++)
		{
			stringBuilder.Append(" ").Append(looks[i]);
		}
		StringBuilder stringBuilder2 = stringBuilder.Append(" rotation:");
		int num = (int)rotation;
		stringBuilder2.Append(num.ToString());
		stringBuilder.Append(" position:").Append($"{position.x},{position.y}");
		stringBuilder.Append(" size:").Append($"{size.x},{size.y}");
		if (!string.IsNullOrEmpty(level))
		{
			stringBuilder.Append(" level:").Append(level);
		}
		if (stories.HasValue)
		{
			stringBuilder.Append(" stories:").Append(stories.Value);
		}
		if (floor.HasValue)
		{
			stringBuilder.Append(" floor:").Append(floor.Value);
		}
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder.ToString()
		});
	}
}
