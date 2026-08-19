using System;
using ItemSystem;
using UnityEngine;

public class SortOptionContainer : MonoBehaviour
{
	[SerializeField]
	private SortOptionWidget _basePrefab;

	[SerializeField]
	private int _depth;

	private SortOptionWidget _optionWidget;

	private bool _isInit;

	public event Action<Util.SortOption, bool> SortOptionSelected;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_optionWidget = ((Component)this).gameObject.AddChild(((Component)_basePrefab).gameObject).GetComponent<SortOptionWidget>();
			UIRect component = ((Component)_optionWidget).GetComponent<UIRect>();
			component.SetAnchor(((Component)this).gameObject, 0, 0, 0, 0);
			_optionWidget.SetDepth(_depth);
			_optionWidget.SortOptionSelected += OnSelectSortOption;
		}
	}

	private void OnSelectSortOption(Util.SortOption option, bool isDescending)
	{
		if (this.SortOptionSelected != null)
		{
			this.SortOptionSelected(option, isDescending);
		}
	}
}
