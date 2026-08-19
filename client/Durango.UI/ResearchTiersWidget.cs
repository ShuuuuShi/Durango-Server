using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ResearchTiersWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _prevTierButton;

	[SerializeField]
	private Selectable _nextTierButon;

	[SerializeField]
	private KInfiniteScrollView _researchsList;

	private KInfiniteScrollView.View<LaboratoryTier, ResearchTierWidget> _view;

	private AvailablePersonalResearch _researchs;

	private readonly List<LaboratoryTier> _availableTiers = new List<LaboratoryTier>();

	private ResearchCategory _category;

	public LaboratoryTier? SelectedTier { get; private set; }

	public string SelectedResearch { get; private set; }

	public int? RequiredPioneerGrade { get; private set; }

	public event Action ResearchSelected;

	void IUIInitializable.Init()
	{
		_view = _researchsList.Initialize(delegate(ResearchTierWidget w, LaboratoryTier tier)
		{
			w.Set(_researchs, tier, SelectedResearch);
		}, delegate(ResearchTierWidget w)
		{
			w.Init();
			w.ResearchClicked += OnSelectResearch;
		});
		_view.SetList(_availableTiers);
		Selectable prevTierButton = _prevTierButton;
		prevTierButton.Clicked = (Action)Delegate.Combine(prevTierButton.Clicked, (Action)delegate
		{
			int num2 = _availableTiers.IndexOf(SelectedTier.GetValueOrDefault(LaboratoryTier.Invalid));
			MoveToTierPage(num2 - 1, instant: false);
			OnSelectResearch(null);
		});
		Selectable nextTierButon = _nextTierButon;
		nextTierButon.Clicked = (Action)Delegate.Combine(nextTierButon.Clicked, (Action)delegate
		{
			int num = _availableTiers.IndexOf(SelectedTier.GetValueOrDefault(LaboratoryTier.Invalid));
			MoveToTierPage(num + 1, instant: false);
			OnSelectResearch(null);
		});
	}

	public void Refresh()
	{
		if (_view == null)
		{
			return;
		}
		foreach (ResearchTierWidget item in _view.List)
		{
			item.Set(_researchs, item.Tier, SelectedResearch);
		}
	}

	private void OnSelectResearch(string id, int? requiredPioneerGrade = null)
	{
		SelectedResearch = id;
		RequiredPioneerGrade = requiredPioneerGrade;
		Refresh();
		if (this.ResearchSelected != null)
		{
			this.ResearchSelected();
		}
	}

	private void MoveToTierPage(int index, bool instant)
	{
		SelectedTier = _availableTiers[index];
		_researchsList.MoveToNode(index, instant);
		_prevTierButton.Disabled = index == 0;
		_nextTierButon.Disabled = index == _availableTiers.Count - 1;
		_titleLabel.text = $"{_category.GetName()} {SelectedTier.Value.GetName()}";
	}

	public bool Set(AvailablePersonalResearch research, bool reset)
	{
		_researchs = research;
		if (reset)
		{
			SelectedTier = null;
			SelectedResearch = null;
			RequiredPioneerGrade = null;
			_view.NodeResize(new Point2(_researchsList.ViewSize));
		}
		_availableTiers.Clear();
		_category = ResearchCategory.Invalid;
		using (Reusable<HashSet<LaboratoryTier>> reusable = ReusableHashSet<LaboratoryTier>.Pop())
		{
			HashSet<LaboratoryTier> value = reusable.Value;
			foreach (Pair<string, int?> item in research.ResearchableIds())
			{
				PersonalResearch personalResearch = SingletonDict<string, PersonalResearch>.Get(item.Item1);
				if (personalResearch != null)
				{
					if (_category == ResearchCategory.Invalid)
					{
						_category = personalResearch.Category;
					}
					value.Add(personalResearch.Tier);
				}
			}
			_availableTiers.AddRange(value);
		}
		if (_availableTiers.Count == 0)
		{
			return false;
		}
		_availableTiers.Sort();
		int num = _availableTiers.IndexOf(SelectedTier.GetValueOrDefault(LaboratoryTier.Invalid));
		if (num == -1)
		{
			num = _availableTiers.Count - 1;
			SelectedTier = _availableTiers[num];
		}
		if (reset)
		{
			_researchsList.UpdateLayout();
		}
		else
		{
			_view.Redraw();
		}
		MoveToTierPage(num, instant: true);
		return true;
	}
}
