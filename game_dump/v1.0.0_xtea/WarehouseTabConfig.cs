using System;
using ItemSystem;
using L10N;
using UnityEngine;

public class WarehouseTabConfig : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabList;

	private bool _isInit;

	private bool _isShow;

	private AnimationWidget _animWidget;

	private int _tabSize;

	private bool _isChanged;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList.Nodes.Init(OnInitTabList);
			_animWidget = AnimationWidget.Get(((Component)this).gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
			_animWidget.SetAlpha(0f, useTween: false);
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnInitTabList(GameObject obj)
	{
		WarehouseTabConfigItem component = obj.GetComponent<WarehouseTabConfigItem>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectItem));
		component.OnChangeName += OnChangeName;
		component.OnUp += OnUp;
		component.OnDown += OnDown;
		component.OnRemove += OnRemove;
		component.OnAdd += OnAdd;
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().TrakingInventoryUpdated += UpdateData;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().TrakingInventoryUpdated -= UpdateData;
	}

	public void Show()
	{
		Init();
		if (!_isShow)
		{
			_isShow = true;
			((Component)this).gameObject.SetActive(true);
			_animWidget.Alpha = 1f;
			BlurController.BlurOn("InventoryOption", BlurController.Mask.UI);
		}
		UpdateData();
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			_animWidget.Alpha = 0f;
			BlurController.BlurOff("InventoryOption");
			Submit();
		}
	}

	private void UpdateData()
	{
		_isChanged = false;
		if (!_isShow)
		{
			Hide();
			return;
		}
		Inventory trakingInventory = GameSystem<InventorySystem>.Instance().TrakingInventory;
		if (trakingInventory.Type != Inventory.InventoryType.Warehouse)
		{
			Hide();
			return;
		}
		ListObjectPool nodes = _tabList.Nodes;
		int size = KUtility.GetSize(trakingInventory.Categories);
		nodes.Set(size);
		for (int i = 0; i < size; i++)
		{
			WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
			component.Set(trakingInventory.Categories[i]);
		}
		_tabSize = size;
		if (trakingInventory.CategoryCapacity > size)
		{
			WarehouseTabConfigItem warehouseTabConfigItem = ((ListObjectPoolBase<GameObject>)nodes).Add<WarehouseTabConfigItem>();
			warehouseTabConfigItem.SetAddButton();
		}
		_tabList.Reposition();
	}

	private void OnClick()
	{
		Hide();
	}

	private void OnSelectItem()
	{
		int num = _tabList.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		ListObjectPool nodes = _tabList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
			component.Select = num == i;
		}
	}

	private void OnChangeName(WarehouseTabConfigItem node)
	{
		string category = node.Text;
		UIManager.Popup.TextInput.Show(delegate(string text)
		{
			GameSystem<InventorySystem>.Instance().ChangeWarehouseCategoryName(category, text);
		}, T._("바꿀 이름을 적어주세요"), category);
	}

	private void OnUp(WarehouseTabConfigItem node)
	{
		int num = _tabList.Nodes.IndexOf(((Component)node).gameObject);
		if (num - 1 >= 0)
		{
			_tabList.Nodes.Swap(num, num - 1);
			_tabList.UpdateLayout(instant: false);
			_isChanged = true;
		}
	}

	private void OnDown(WarehouseTabConfigItem node)
	{
		int num = _tabList.Nodes.IndexOf(((Component)node).gameObject);
		if (num + 1 < _tabSize)
		{
			_tabList.Nodes.Swap(num, num + 1);
			_tabList.UpdateLayout(instant: false);
			_isChanged = true;
		}
	}

	private void OnRemove(WarehouseTabConfigItem node)
	{
		string key = node.Text;
		UIManager.MessageBox.Show(T._("{0:을} 삭제하시겠습니까?", key), delegate(bool ok)
		{
			if (ok)
			{
				Submit();
				GameSystem<InventorySystem>.Instance().RemoveWarehouseCategory(key);
			}
		});
	}

	private void OnAdd()
	{
		UIManager.Popup.TextInput.Show(delegate(string text)
		{
			Submit();
			GameSystem<InventorySystem>.Instance().AddWarehouseCategory(text);
		}, T._("추가할 이름을 적어주세요"));
	}

	private void Submit()
	{
		if (_isChanged)
		{
			ListObjectPool nodes = _tabList.Nodes;
			string[] array = new string[_tabSize];
			for (int i = 0; i < _tabSize; i++)
			{
				WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
				array[i] = component.Text;
			}
			GameSystem<InventorySystem>.Instance().SetWarehouseCategoryList(array);
			_isChanged = false;
		}
	}
}
