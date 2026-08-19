using System;
using System.Collections;
using System.Collections.Generic;
using ItemSystem;
using UnityEngine;
using Yaml;

public class CageAnimalListWidget : MonoBehaviour
{
	[Serializable]
	private struct Option
	{
		public ColorState InCage;

		public ColorState CanInsert;

		public ColorState CannotInsert;
	}

	[Serializable]
	private struct ColorState
	{
		public Color Background;

		public Color Icon;

		public Color Name;
	}

	private const float SetDelay = 0.2f;

	[SerializeField]
	private UILabel _capacityLabel;

	[SerializeField]
	private KScrollView _animals;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private Option _option;

	[SerializeField]
	private UIPanel _upperPanel;

	[SerializeField]
	private Vector3 _takeOutAnimationOffset;

	[SerializeField]
	private Vector3 _putInAnimationTarget;

	private Cage _cage;

	private List<ItemData> _items = new List<ItemData>();

	private int _cageCount;

	private string _cageCapacityFormat;

	private float _delayAt;

	private UISprite _animationSprite;

	private UISprite AnimationSprite
	{
		get
		{
			if ((Object)(object)_animationSprite == (Object)null)
			{
				_animationSprite = ((Component)_upperPanel).gameObject.AddChild<UISprite>();
				UISprite iconSprite = _animals.Nodes.BaseObject.GetComponent<CageAnimalListNode>().IconSprite;
				_animationSprite.atlas = iconSprite.atlas;
				TweenAlpha tweenAlpha = ((Component)_animationSprite).gameObject.AddComponent<TweenAlpha>();
				tweenAlpha.SetOnFinished(delegate
				{
					((Component)_animationSprite).gameObject.SetActive(false);
				});
			}
			return _animationSprite;
		}
	}

	public ulong SelectedAnimal { get; private set; }

	public event Action<ItemData> Selected;

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		_delayAt = 0f;
		if (_cage != null)
		{
			UpdateList();
		}
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		OnUnselect();
		_cage = null;
		SelectedAnimal = 0uL;
	}

	private void OnUpdateInventory()
	{
		if (_cage != null)
		{
			Set(_cage);
		}
	}

	public void SelectAnimal(ulong id)
	{
		OnUnselect();
		if (SelectedAnimal == id)
		{
			id = 0uL;
		}
		SelectedAnimal = id;
		int num = -1;
		for (int i = 0; i < _animals.Nodes.Count; i++)
		{
			CageAnimalListNode component = _animals.Nodes[i].GetComponent<CageAnimalListNode>();
			if (component.Id == SelectedAnimal)
			{
				num = i;
				component.Select = true;
			}
			else
			{
				component.Select = false;
			}
		}
		if (num != -1 && num < _cageCount)
		{
			_cage.HighlightAnimal(id, enable: true);
		}
		if (this.Selected != null)
		{
			this.Selected((num != -1) ? _items[num] : null);
		}
	}

	private void OnUnselect()
	{
		int num = Util.IndexOf(_items, SelectedAnimal);
		if (num != -1 && num < _cageCount)
		{
			_cage.HighlightAnimal(_items[num].Id, enable: false);
		}
	}

	public void TakeOutAnimation(ulong id)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		int num = Util.IndexOf(_items, SelectedAnimal);
		if (num != -1)
		{
			UISprite animationSprite = AnimationSprite;
			((Component)animationSprite).gameObject.SetActive(true);
			animationSprite.spriteName = _items[num].Icon;
			CageAnimalListNode component = _animals.Nodes[num].GetComponent<CageAnimalListNode>();
			((Component)animationSprite).transform.position = ((Component)component.IconSprite).transform.position;
			Vector3 localPosition = ((Component)animationSprite).transform.localPosition;
			((Component)animationSprite).transform.localPosition = localPosition + _takeOutAnimationOffset;
			TweenPosition.Begin(((Component)animationSprite).gameObject, 1f, localPosition);
			animationSprite.color = component.IconSprite.color;
			animationSprite.alpha = 0f;
			TweenAlpha.Begin(((Component)animationSprite).gameObject, 0.5f, 1f);
		}
	}

	public void PutInAnimation(ulong id)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		int num = Util.IndexOf(_items, SelectedAnimal);
		if (num != -1)
		{
			UISprite animationSprite = AnimationSprite;
			((Component)animationSprite).gameObject.SetActive(true);
			animationSprite.spriteName = _items[num].Icon;
			CageAnimalListNode component = _animals.Nodes[num].GetComponent<CageAnimalListNode>();
			((Component)animationSprite).transform.position = ((Component)component.IconSprite).transform.position;
			TweenPosition.Begin(((Component)animationSprite).gameObject, 1f, _putInAnimationTarget);
			animationSprite.color = component.IconSprite.color;
			animationSprite.alpha = 1f;
			TweenAlpha.Begin(((Component)animationSprite).gameObject, 0.5f, 0f);
		}
	}

	public void Set(Cage cage)
	{
		_cage = cage;
		float time = Time.time;
		if (_delayAt > time || !((Component)this).gameObject.activeInHierarchy)
		{
			_delayAt = time + 0.2f;
			return;
		}
		_delayAt = time + 0.2f;
		((MonoBehaviour)this).StartCoroutine(DelaySet());
	}

	private IEnumerator DelaySet()
	{
		while (true)
		{
			float now = Time.time;
			if (now >= _delayAt)
			{
				break;
			}
			yield return (object)new WaitForSeconds(_delayAt - now);
		}
		UpdateList();
	}

	private void UpdateList()
	{
		if (_cage == null)
		{
			_items.Clear();
			Refresh();
			return;
		}
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		_items.Clear();
		for (int i = 0; i < _cage.ReinsList.Count; i++)
		{
			ItemData item = _cage.ReinsList[i];
			Add(item);
		}
		_cageCount = _items.Count;
		for (int j = 0; j < playerItemList.Count; j++)
		{
			ItemData item2 = playerItemList[j];
			Add(item2);
		}
		ulong selectedAnimal = SelectedAnimal;
		Refresh();
		SelectAnimal(selectedAnimal);
	}

	private void Add(ItemData item)
	{
		if (item != null && item.Reins != null)
		{
			_items.Add(item);
		}
	}

	private void Refresh()
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (_cageCapacityFormat == null)
		{
			_cageCapacityFormat = _capacityLabel.text;
		}
		_capacityLabel.text = string.Format(_cageCapacityFormat, _cage.Capacity - _cage.RemainSize, _cage.Capacity);
		int remainSize = _cage.RemainSize;
		ListObjectPool nodes = _animals.Nodes;
		nodes.Init(OnInitNode);
		nodes.Clear();
		for (int i = 0; i < _items.Count; i++)
		{
			CageAnimalListNode cageAnimalListNode = ((ListObjectPoolBase<GameObject>)nodes).Add<CageAnimalListNode>();
			ItemData itemData = _items[i];
			Reins reins = itemData.Reins;
			bool flag = i < _cageCount;
			ColorState colorState = ((!flag) ? ((reins.Size > remainSize) ? _option.CannotInsert : _option.CanInsert) : _option.InCage);
			cageAnimalListNode.Set(itemData.Id, reins.PetName, AnimalYaml.GetPortrait(reins.VehicleEntityType), reins.Hungry, flag);
			cageAnimalListNode.SetColor(colorState.Icon, colorState.Background, colorState.Name);
		}
		_noData.gameObject.SetActive(nodes.Count == 0);
		_animals.Reposition();
	}

	private void OnInitNode(GameObject obj)
	{
		CageAnimalListNode component = obj.GetComponent<CageAnimalListNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickAnimalNode));
	}

	private void OnClickAnimalNode()
	{
		CageAnimalListNode cageAnimalListNode = Selectable.Current as CageAnimalListNode;
		ulong id = ((!((Object)(object)cageAnimalListNode == (Object)null)) ? cageAnimalListNode.Id : 0);
		SelectAnimal(id);
	}
}
