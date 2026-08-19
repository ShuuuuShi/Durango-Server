using System.Collections.Generic;
using Building_;
using Crafting;
using ItemSystem;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PerformancesPage : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private GameObject _noRecipes;

	[SerializeField]
	private ItemContextColorInfo _colorInfo;

	[SerializeField]
	private ListObjectPool _performanceControls;

	[SerializeField]
	private ListObjectPool _craftInfoControls;

	private readonly HashSet<string> _collapsedContextIds = new HashSet<string>();

	private readonly HashSet<Crafting.Recipe> _availableRecipes = new HashSet<Crafting.Recipe>();

	private readonly HashSet<Building_.Blueprint> _availableBlueprints = new HashSet<Building_.Blueprint>();

	private UIWidget _invisibleBox;

	public void Init()
	{
		_colorInfo.Init();
		_colorInfo.OnExpandChanged += OnControlExpandChanged;
		_performanceControls.Init(ItemContextControlInitializer);
		_craftInfoControls.Init(ItemContextControlInitializer);
	}

	public void ShowItemContent(ItemData itemData, bool enableCraftLink)
	{
		ItemContextBase.ResetFlag = true;
		_performanceControls.Clear();
		ItemColor colors = itemData.Colors;
		if (colors.HasValue)
		{
			((Component)_colorInfo).gameObject.SetActive(true);
			_colorInfo.Set(colors);
			_colorInfo.IsExpanded = !_collapsedContextIds.Contains(_colorInfo.Id);
		}
		else
		{
			((Component)_colorInfo).gameObject.SetActive(false);
		}
		int i = 0;
		for (int count = itemData.Performances.Count; i < count; i++)
		{
			PerformanceData performanceData = itemData.Performances[i];
			Dictionary<string, PerformanceVisibleInfo> dictionary = SingletonDict<string, Dictionary<string, PerformanceVisibleInfo>>.Get(performanceData.id);
			if (dictionary != null && dictionary.Count != 0)
			{
				ItemContextPerformance itemContextPerformance = ((ListObjectPoolBase<GameObject>)_performanceControls).Add<ItemContextPerformance>();
				itemContextPerformance.Set(itemData.Performances[i], dictionary);
				itemContextPerformance.IsExpanded = !_collapsedContextIds.Contains(itemContextPerformance.Id);
			}
		}
		if (itemData.Reins != null)
		{
			ItemContextPerformance itemContextPerformance2 = ((ListObjectPoolBase<GameObject>)_performanceControls).Add<ItemContextPerformance>();
			itemContextPerformance2.Set(itemData.Reins);
			itemContextPerformance2.IsExpanded = !_collapsedContextIds.Contains(itemContextPerformance2.Id);
		}
		if (itemData.Capsule != null)
		{
			ItemContextPerformance itemContextPerformance3 = ((ListObjectPoolBase<GameObject>)_performanceControls).Add<ItemContextPerformance>();
			itemContextPerformance3.Set(itemData.Capsule);
			itemContextPerformance3.IsExpanded = !_collapsedContextIds.Contains(itemContextPerformance3.Id);
		}
		if (itemData.IsEquipments)
		{
			_availableRecipes.Clear();
			_availableBlueprints.Clear();
		}
		else
		{
			GameSystem<RecipeSystem>.Instance().FillAvailableRecipesByItemData(_availableRecipes, itemData);
			GameSystem<RecipeSystem>.Instance().FillAvailableBlueprintsByItemData(_availableBlueprints, itemData);
		}
		_craftInfoControls.Clear();
		if (_availableRecipes.Count > 0)
		{
			ItemContextCraftInfo itemContextCraftInfo = ((ListObjectPoolBase<GameObject>)_craftInfoControls).Add<ItemContextCraftInfo>();
			itemContextCraftInfo.Set(_availableRecipes, enableCraftLink);
			itemContextCraftInfo.IsExpanded = !_collapsedContextIds.Contains(itemContextCraftInfo.Id);
		}
		if (_availableBlueprints.Count > 0)
		{
			ItemContextCraftInfo itemContextCraftInfo2 = ((ListObjectPoolBase<GameObject>)_craftInfoControls).Add<ItemContextCraftInfo>();
			itemContextCraftInfo2.Set(_availableBlueprints, enableCraftLink);
			itemContextCraftInfo2.IsExpanded = !_collapsedContextIds.Contains(itemContextCraftInfo2.Id);
		}
		_noRecipes.SetActive(!((Component)_colorInfo).gameObject.activeSelf && _performanceControls.Count + _craftInfoControls.Count == 0);
		UpdateLayout(instant: true);
		ItemContextBase.ResetFlag = false;
		_scrollView.ResetPosition();
	}

	private void OnEnable()
	{
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleBox);
	}

	private void UpdateLayout(bool instant)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		zero.y -= UIUtility.WidgetsReposition(_performanceControls.Get, _performanceControls.Count, Vector3.down, zero, 0f, instant);
		zero.y -= UIUtility.WidgetsReposition(_craftInfoControls.Get, _craftInfoControls.Count, Vector3.down, zero, 0f, instant);
		if (((Component)_colorInfo).gameObject.activeSelf)
		{
			_colorInfo.Widget.SetPosition(zero, 0.5f, 1f);
			zero.y -= (float)_colorInfo.Widget.height;
		}
	}

	private void ItemContextControlInitializer(GameObject obj)
	{
		ItemContextBase component = obj.GetComponent<ItemContextBase>();
		component.Init();
		component.OnExpandChanged += OnControlExpandChanged;
	}

	private void OnControlExpandChanged(ItemContextBase itemContext)
	{
		if (itemContext.IsExpanded)
		{
			_collapsedContextIds.Remove(itemContext.Id);
		}
		else
		{
			_collapsedContextIds.Add(itemContext.Id);
		}
		UpdateLayout(instant: false);
		_scrollView.RestrictWithinBounds(instant: false);
	}
}
