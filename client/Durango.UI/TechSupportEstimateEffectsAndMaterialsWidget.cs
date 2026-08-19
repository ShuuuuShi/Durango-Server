using System.Collections.Generic;
using Crafting;
using Durango.UI.Control;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class TechSupportEstimateEffectsAndMaterialsWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private TechSupportTag _techSupportTagBase;

	[SerializeField]
	private UIWidget _materialsStart;

	[SerializeField]
	private UIWidget _bgForMaterials;

	[SerializeField]
	private TechSupportMaterial _techSupportMaterialBase;

	private readonly List<int> _recipeSlotCount = new List<int>();

	private ListObjectPool<TechSupportTag> _tagItems;

	private ListObjectPool<TechSupportMaterial> _materialItems;

	void IUIInitializable.Init()
	{
		_tagItems = new ListObjectPool<TechSupportTag>();
		_tagItems.BaseObject = _techSupportTagBase;
		_tagItems.UseBase = true;
		_materialItems = new ListObjectPool<TechSupportMaterial>();
		_materialItems.BaseObject = _techSupportMaterialBase;
		_materialItems.UseBase = true;
	}

	public void Refresh(RecipeReform recipe, ReformSlot? reformSlot, TechSupportEstimate? estimate)
	{
		_scrollView.Widgets.Clear();
		if (recipe != null && reformSlot.HasValue)
		{
			ReformTechSupport reformTechSupport = SingletonDict<string, ReformTechSupport>.Instance.Get(recipe.Id);
			if (reformTechSupport != null)
			{
				if (estimate.HasValue)
				{
					AddTechSupportTagWithEstimate(reformSlot.Value, estimate.Value, reformTechSupport);
					AddTechSupportMaterials(recipe);
				}
				else
				{
					AddTechSupportTag(reformSlot.Value, reformTechSupport);
					AddTechSupportMaterials(null);
				}
			}
		}
		else
		{
			_tagItems.Clear();
			_materialItems.Clear();
		}
		_scrollView.ResetPosition();
	}

	private void AddTechSupportTag(ReformSlot reformSlot, ReformTechSupport yamlTechSupport)
	{
		_tagItems.BeginLoad();
		Messages.Tag[] tags = reformSlot.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag tag = tags[i];
			TechSupportTag next = _tagItems.GetNext();
			next.SetBeforeOnly(tag.Id, tag.Level, reformSlot.TagRareness.Get(tag.Id, (TagLevelRareness)0), TechSupportTag.GetMaxLevelFromTechSupport(tag.Id, yamlTechSupport), hideAfterText: true);
			next.Widget.UpdateAnchors();
			_scrollView.Widgets.Add(next.Widget);
		}
		_tagItems.EndLoad();
	}

	private void AddTechSupportTagWithEstimate(ReformSlot reformSlot, TechSupportEstimate estimate, ReformTechSupport yamlTechSupport)
	{
		_tagItems.BeginLoad();
		Messages.Tag[] tags = reformSlot.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag tag = tags[i];
			TechSupportTag next = _tagItems.GetNext();
			next.SetAll(tag.Id, tag.Level, reformSlot.TagRareness.Get(tag.Id, (TagLevelRareness)0), TechSupportTag.GetTag(estimate.Tags, tag.Id).Level, estimate.TagRareness.Get(tag.Id, (TagLevelRareness)0));
			next.Widget.UpdateAnchors();
			_scrollView.Widgets.Add(next.Widget);
		}
		_tagItems.EndLoad();
	}

	private void AddTechSupportMaterials(RecipeReform recipe)
	{
		int num = 0;
		_materialItems.BeginLoad();
		if (recipe != null)
		{
			RecipeSystem.HasMaterials(recipe, out var _, _recipeSlotCount);
			num = Mathf.Min(recipe.Slots.Length, _recipeSlotCount.Count);
		}
		if (num > 0)
		{
			_materialsStart.gameObject.SetActive(value: true);
			_materialsStart.UpdateAnchors();
			_scrollView.Widgets.Add(_materialsStart);
			for (int i = 0; i < num; i++)
			{
				Crafting.RecipeSlot recipeSlot = recipe.Slots[i];
				TechSupportMaterial next = _materialItems.GetNext();
				next.SetMaterial(recipeSlot.Name, (recipeSlot.SlotType == Crafting.RecipeSlot.Type.ReformBase) ? 1 : _recipeSlotCount[i], recipeSlot.Count);
				next.UpdateAnchors();
				_scrollView.Widgets.Add(next);
			}
			_bgForMaterials.bottomAnchor.absolute = -(_techSupportMaterialBase.height * num);
			UIUtility.UpdateAnchors(_bgForMaterials.transform);
			if (3 > num)
			{
				for (int num2 = 3 - num; num2 > 0; num2--)
				{
					TechSupportMaterial next2 = _materialItems.GetNext();
					next2.SetEmpty();
					next2.UpdateAnchors();
					_scrollView.Widgets.Add(next2);
				}
			}
		}
		else
		{
			_materialsStart.gameObject.SetActive(value: false);
		}
		_materialItems.EndLoad();
	}
}
