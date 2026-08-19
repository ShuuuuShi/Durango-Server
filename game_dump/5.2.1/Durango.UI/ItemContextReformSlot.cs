using Crafting;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class ItemContextReformSlot : UIWidget
{
	[SerializeField]
	private UIWidget _slotInfo;

	[SerializeField]
	private GameObject _upperDotLine;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private NestedPrefabLinker _tagsViewerLinker;

	private TagsViewerWidget _tagsViewer;

	public void Init()
	{
		_tagsViewer = _tagsViewerLinker.Object.GetComponent<TagsViewerWidget>();
	}

	public void ShowUpperDotLine(bool show)
	{
		_upperDotLine.SetActive(show);
	}

	public void Set(ReformSlot slot)
	{
		RecipeReform reformRecipe = TechSupportSystem.GetReformRecipe(slot);
		if (reformRecipe != null)
		{
			_icon.gameObject.SetActive(value: true);
			_icon.spriteName = reformRecipe.Icon;
			if (!string.IsNullOrEmpty(slot.Decorator))
			{
				_text.text = reformRecipe.RecipeNameForSlot + " (" + slot.Decorator + ")";
			}
			else
			{
				_text.text = reformRecipe.RecipeNameForSlot;
			}
		}
		else
		{
			_icon.gameObject.SetActive(value: false);
			_text.text = T._("개조되지 않음");
		}
		_tagsViewer.SettingBegin();
		Tag[] tags = slot.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Tag tag = tags[i];
			_tagsViewer.AddTagData(tag.Id, tag.Level);
		}
		_tagsViewerLinker.gameObject.SetActive(_tagsViewer.SettingEnd());
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		if (_tagsViewerLinker.gameObject.activeSelf)
		{
			base.height = _slotInfo.height + _tagsViewerLinker.GetComponent<UIWidget>().height;
		}
		else
		{
			base.height = _slotInfo.height;
		}
	}
}
