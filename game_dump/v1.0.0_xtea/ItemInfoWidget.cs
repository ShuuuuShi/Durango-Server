using ItemSystem;
using UnityEngine;

public class ItemInfoWidget : MonoBehaviour
{
	private const string PageIndexPrefsKey = "ItemDetailPageIndex";

	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private WidgetLayout _verticalLayout;

	[SerializeField]
	private WidgetLayout _horizontalLayout;

	[SerializeField]
	private ItemCommonInfoWidget _commonInfo;

	[SerializeField]
	private GameObject _itemSwipeView;

	[SerializeField]
	private PageSwipe _detailPageSwipe;

	[SerializeField]
	private PerformancesPage _performancesPage;

	[SerializeField]
	private TagsPage _tagsPage;

	private bool _isInit;

	private UIWidget _widget;

	private bool _enableCraftLink;

	public bool IsOpen { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public ItemData CurrentItem { get; private set; }

	public void Init(int bottomMargin, bool enableCraftLink)
	{
		if (!_isInit)
		{
			_isInit = true;
			_enableCraftLink = enableCraftLink;
			UpdateLayout(bottomMargin);
			_performancesPage.Init();
			_detailPageSwipe.SetDefaultIndex(PlayerPrefs.GetInt("ItemDetailPageIndex"));
			UIPanel componentInParent = ((Component)this).GetComponentInParent<UIPanel>();
			UIPanel[] componentsInChildren = ((Component)this).GetComponentsInChildren<UIPanel>(true);
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				componentsInChildren[i].depth = componentInParent.depth + componentsInChildren[i].depth;
			}
		}
	}

	public void UpdateLayout(int bottomMargin)
	{
		Transform parent = ((Component)this).transform.parent;
		if ((Object)(object)parent != (Object)null)
		{
			Widget.SetAnchor(((Component)parent).gameObject, 0, 0, 0, 0);
			Widget.updateAnchors = UIRect.AnchorUpdate.OnEnable;
			Widget.UpdateAnchors();
		}
		_container.bottomAnchor.absolute = bottomMargin;
		_container.ResetAndUpdateAnchors();
		UpdateLayout();
	}

	[ExposedInEditor(null)]
	private void UpdateLayout()
	{
		WidgetLayout widgetLayout = ((_container.width >= _container.height) ? _horizontalLayout : _verticalLayout);
		widgetLayout.UpdateLayout(_container);
		UIUtility.UpdateAnchors(((Component)_container).transform);
	}

	public void SetItemData(ItemData item)
	{
		CurrentItem = item;
		ShowItemContent();
	}

	public void Open()
	{
		if (!IsOpen)
		{
			((Component)this).gameObject.SetActive(true);
			IsOpen = true;
		}
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
		SavePreferences();
		IsOpen = false;
	}

	private void SavePreferences()
	{
		PlayerPrefs.SetInt("ItemDetailPageIndex", _detailPageSwipe.CurrentIndex);
		PlayerPrefs.Save();
	}

	private void ShowItemContent()
	{
		if (CurrentItem != null)
		{
			_commonInfo.Set(CurrentItem);
			int contentCount = CurrentItem.ContentCount;
			int num = (int)CurrentItem.GetFloatAttribute("capacity");
			ItemData itemData = ((contentCount <= 0 || num <= 0) ? CurrentItem : CurrentItem.GetContent(0));
			_performancesPage.ShowItemContent(itemData, _enableCraftLink);
			_tagsPage.ShowItemContent(itemData);
			UpdateLayout();
		}
	}
}
