using System;
using System.Collections.Generic;
using InteractionData;
using Shared.System;
using UnityEngine;

public class InteractionMenuControl : MonoBehaviour
{
	public Action<InteractionMenuData> OnClickInteractionMenu;

	public Action<InteractionMenuData> OnLongClickInteractionMenu;

	public Action<string> OnGatheringQueueClick;

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _visibleStartDegree;

	[SerializeField]
	private int[] _visibleOrder = new int[6] { 0, 1, 2, 3, 4, 5 };

	private int _visibleStartIndex;

	[SerializeField]
	private UILabel _targetNameLabel;

	[SerializeField]
	private UISprite _hexLineBase;

	[SerializeField]
	private InteractionMenu _interactionMenu;

	[SerializeField]
	private GameObject _nextArrow;

	private GameObject _prevArrow;

	[SerializeField]
	private float _majorScale;

	[SerializeField]
	private float _minorScale;

	private GameObject _hexLineContainer;

	private List<UISprite> _hexLines;

	private Queue<InteractionMenu> _interactionMenuPool;

	private readonly List<InteractionMenu> _menus = new List<InteractionMenu>();

	private bool _isInit;

	private InteractionObject _menuTarget;

	private AnimationWidget _animWidget;

	private bool _updateFlag;

	private bool _updateResetFlag;

	private bool _isShow;

	private DelayedFunction _updateQueueFunc;

	public static float MajorScale { get; private set; }

	public static float MinorScale { get; private set; }

	private int VisibleCount => _visibleOrder.Length;

	private string TargetName
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_targetNameLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_targetNameLabel).gameObject.SetActive(true);
			_targetNameLabel.text = value;
		}
	}

	public int VisiblePage { get; set; }

	private void Start()
	{
		_updateQueueFunc = new DelayedFunction(SetGatheringQueueList);
		if (_isShow)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void OnEnable()
	{
		GameSystem<GatheringSystem>.Instance().GatheringQueueUpdated += OnUpdateGatheringQueue;
		GameSystem<InteractionSystem>.Instance().MenuList.Updated += OnUpdateInteractionMenu;
	}

	private void OnDisable()
	{
		GameSystem<GatheringSystem>.Instance().GatheringQueueUpdated -= OnUpdateGatheringQueue;
		GameSystem<InteractionSystem>.Instance().MenuList.Updated -= OnUpdateInteractionMenu;
		_menuTarget = null;
	}

	private void LateUpdate()
	{
		RepositionInteractionMenuContainer();
		LateUpdateMenuList();
	}

	public void Show()
	{
		Init();
		_isShow = true;
		((Component)_animWidget).gameObject.SetActive(true);
		_animWidget.Alpha = 1f;
		UpdateMenuList();
	}

	public void Hide()
	{
		Init();
		_isShow = false;
		_animWidget.Alpha = 0f;
	}

	private void Init()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_interactionMenuPool = new Queue<InteractionMenu>();
			_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			_hexLines = new List<UISprite>();
			((Component)_hexLineBase).gameObject.SetActive(false);
			((Component)_interactionMenu).gameObject.SetActive(false);
			for (int i = 0; i < 6; i++)
			{
				GameObject val = ((Component)((Component)_hexLineBase).transform.parent).gameObject.AddChild(((Component)_hexLineBase).gameObject);
				val.SetActive(true);
				_hexLines.Add(val.GetComponent<UISprite>());
			}
			_hexLineContainer = ((Component)((Component)_hexLineBase).transform.parent).gameObject;
			_prevArrow = ((Component)_nextArrow.transform.parent).gameObject.AddChild(_nextArrow.gameObject);
			for (int j = 0; j < _prevArrow.transform.childCount; j++)
			{
				Transform child = _prevArrow.transform.GetChild(j);
				child.localEulerAngles += Vector3.forward * 180f;
			}
			UIEventListener.Get(_nextArrow).onClick = OnClickArrow;
			UIEventListener.Get(_prevArrow).onClick = OnClickArrow;
			UIEventListener.Get(((Component)_targetNameLabel).gameObject).onDrag = UIManager.IgnoreUIDrag;
			MajorScale = _majorScale;
			MinorScale = _minorScale;
		}
	}

	private void OnUpdateInteractionMenu()
	{
		if (_isShow)
		{
			UpdateMenuList();
		}
	}

	private void OnUpdateGatheringQueue()
	{
		if (_isShow)
		{
			_updateQueueFunc.Call((MonoBehaviour)(object)this);
		}
	}

	private void UpdateMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		_updateFlag = true;
		_updateResetFlag |= menuList.ResetFrame == Time.frameCount;
	}

	private void LateUpdateMenuList()
	{
		if (_updateFlag)
		{
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			TargetName = menuList.Name;
			InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
			_menuTarget = ((lastInteractionTarget != null && lastInteractionTarget.IsValid()) ? lastInteractionTarget : null);
			if (_updateResetFlag)
			{
				RemoveAll();
			}
			Set(menuList);
			Reposition(!_updateResetFlag);
			_updateFlag = false;
			_updateResetFlag = false;
		}
	}

	private void OnClickMenu(InteractionMenu menu)
	{
		if (OnClickInteractionMenu != null)
		{
			OnClickInteractionMenu(menu.Data);
		}
	}

	private void OnLongClickMenu(InteractionMenu menu)
	{
		if (OnLongClickInteractionMenu != null)
		{
			OnLongClickInteractionMenu(menu.Data);
		}
	}

	private void OnGatheringQueueClickMenu(InteractionMenu menu)
	{
		if (OnGatheringQueueClick != null)
		{
			OnGatheringQueueClick(menu.Data.GatheringId);
		}
	}

	public InteractionMenu FindMenu(Shared.System.Interaction action, string argument = null)
	{
		for (int i = 0; i < _menus.Count; i++)
		{
			if (_menus[i].Data.IsServer && _menus[i].Data.Action == (int)action)
			{
				if (string.IsNullOrEmpty(_menus[i].Data.GatheringId) && string.IsNullOrEmpty(argument))
				{
					return _menus[i];
				}
				if (_menus[i].Data.GatheringId == argument)
				{
					return _menus[i];
				}
			}
		}
		return null;
	}

	private void SetGatheringQueueList()
	{
		List<GatheringQueueData> gatheringQueue = GameSystem<GatheringSystem>.Instance().GatheringQueue;
		int count = gatheringQueue.Count;
		Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
		Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
		for (int i = 0; i < count; i++)
		{
			List<string> list;
			if (dictionary.TryGetValue(gatheringQueue[i].Data.Id, out var value))
			{
				list = dictionary2[gatheringQueue[i].Data.Id];
			}
			else
			{
				value = new List<int>();
				list = new List<string>();
				dictionary.Add(gatheringQueue[i].Data.Id, value);
				dictionary2.Add(gatheringQueue[i].Data.Id, list);
			}
			value.Add(gatheringQueue[i].ID);
			list.Add(gatheringQueue[i].Data.Icon);
		}
		for (int j = 0; j < _menus.Count; j++)
		{
			InteractionMenu interactionMenu = _menus[j];
			if (interactionMenu.Data.IsServer && interactionMenu.Data.Action == 506)
			{
				if (dictionary.TryGetValue(interactionMenu.Data.GatheringId, out var value2))
				{
					interactionMenu.SetGatheringQueueItems(value2, dictionary2[interactionMenu.Data.GatheringId]);
				}
				else
				{
					interactionMenu.ClearGatheringQueueItem();
				}
			}
		}
	}

	private void Reposition(bool instant)
	{
		RepositionMenuItems();
		RepositionInteractionMenuHexLines(instant);
	}

	private void RepositionInteractionMenuContainer()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = ((_menuTarget != null) ? MainCamera.WorldToNGUIPos(_menuTarget.Position, ((Component)this).transform.parent) : Vector3.zero);
	}

	private void RepositionInteractionMenuHexLines(bool instant)
	{
		int count = _menus.Count;
		float radius = _radius - 30f;
		if (count > 1)
		{
			RepositionMenuMultipleLines(radius, 37);
		}
		else if (count == 1)
		{
			RepositionMenuOneHexLine(radius, 37);
		}
		else
		{
			_hexLineContainer.SetActive(false);
		}
		TweenAlpha component = _hexLineContainer.GetComponent<TweenAlpha>();
		if (!instant && !((Behaviour)component).enabled && _hexLineContainer.activeSelf)
		{
			_hexLineContainer.GetComponent<UIWidget>().alpha = 0f;
			component.delay = (float)count * 0.05f + 0.1f;
			component.tweenFactor = 0f;
			component.PlayForward();
		}
	}

	private void RepositionMenuMultipleLines(float radius, int eraseWidth)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		_hexLineContainer.SetActive(true);
		int i = 0;
		for (int visibleCount = VisibleCount; i < visibleCount; i++)
		{
			int num = _visibleOrder[(i + _visibleStartIndex) % VisibleCount];
			UISprite uISprite = _hexLines[num];
			((Component)uISprite).gameObject.SetActive(true);
			float num2 = (_visibleStartDegree + (float)num * 60f) * ((float)Math.PI / 180f);
			Vector3 val = radius * (Vector3.right * Mathf.Sin(num2) + Vector3.up * Mathf.Cos(num2));
			float num3 = (float)num * 60f;
			float num4 = radius;
			int num5 = _visibleOrder.IndexOf(num);
			int num6 = _visibleOrder.IndexOf((num + 1) % VisibleCount);
			num5 = (num5 + (VisibleCount - _visibleStartIndex)) % VisibleCount;
			num6 = (num6 + (VisibleCount - _visibleStartIndex)) % VisibleCount;
			int num7 = VisibleIndexToMenuIndex(num5);
			if (num7 != -1)
			{
				InteractionMenu interactionMenu = _menus[num7];
				float num8 = ((!interactionMenu.IsMajor) ? _minorScale : _majorScale);
				float num9 = (float)eraseWidth * (1f - (1f - num8) * 0.2f);
				val = val + Vector3.right * Mathf.Cos(num3 * ((float)Math.PI / 180f)) * num9 + Vector3.down * Mathf.Sin(num3 * ((float)Math.PI / 180f)) * num9;
				num4 -= num9;
			}
			num7 = VisibleIndexToMenuIndex(num6);
			if (num7 != -1)
			{
				InteractionMenu interactionMenu2 = _menus[num7];
				float num10 = ((!interactionMenu2.IsMajor) ? _minorScale : _majorScale);
				float num11 = (float)eraseWidth * (1f - (1f - num10) * 0.2f);
				num4 -= num11;
			}
			((Component)uISprite).transform.localPosition = val;
			((Component)uISprite).transform.localEulerAngles = Vector3.back * num3;
			uISprite.width = (int)num4;
			uISprite.color = Color.black;
			uISprite.alpha = 0.45f;
			uISprite.height = 4;
		}
	}

	private void RepositionMenuOneHexLine(float radius, int eraseWidth)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		InteractionMenu interactionMenu = _menus[0];
		float num = ((!interactionMenu.IsMajor) ? _minorScale : _majorScale);
		_hexLineContainer.SetActive(true);
		for (int i = 0; i < _hexLines.Count; i++)
		{
			((Component)_hexLines[i]).gameObject.SetActive(false);
		}
		UISprite uISprite = _hexLines[0];
		((Component)uISprite).gameObject.SetActive(true);
		float num2 = (float)Math.PI / 2f - interactionMenu.MenuRadian;
		((Component)uISprite).transform.localPosition = (Vector3.right * Mathf.Cos(num2) + Vector3.up * Mathf.Sin(num2)) * 30f;
		((Component)uISprite).transform.localEulerAngles = Vector3.forward * num2 * 57.29578f;
		uISprite.width = (int)(radius - (float)eraseWidth * num - 30f);
		uISprite.color = Color.white;
		uISprite.alpha = 0.45f;
		uISprite.height = 2;
	}

	private int VisibleIndexToMenuIndex(int visibleIndex)
	{
		int i = 0;
		for (int count = _menus.Count; i < count; i++)
		{
			if (_menus[i].Index - VisiblePage * VisibleCount == visibleIndex)
			{
				return i;
			}
		}
		return -1;
	}

	private void RepositionMenuItems()
	{
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		float radius = _radius;
		int num = VisiblePage;
		int count = _menus.Count;
		int num2 = num * VisibleCount;
		int num3 = (num + 1) * VisibleCount;
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			InteractionMenu interactionMenu = _menus[i];
			if (interactionMenu.Index < num2 || interactionMenu.Index >= num3)
			{
				interactionMenu.Index = -1;
			}
			else
			{
				num4++;
			}
		}
		if (num4 == 0 && num > 0)
		{
			num = (VisiblePage = num - 1);
			num2 -= VisibleCount;
			num3 -= VisibleCount;
		}
		for (int j = 0; j < count; j++)
		{
			InteractionMenu interactionMenu2 = _menus[j];
			if (interactionMenu2.Index == -1)
			{
				interactionMenu2.Index = FindEmptyIndex();
				interactionMenu2.NeedInitAnimation = true;
			}
		}
		_visibleStartIndex = 0;
		int num6 = 0;
		Vector2 val = default(Vector2);
		for (int k = 0; k < count; k++)
		{
			InteractionMenu interactionMenu3 = _menus[k];
			if (interactionMenu3.Index < num2 || interactionMenu3.Index >= num3)
			{
				((Component)interactionMenu3).gameObject.SetActive(false);
				continue;
			}
			((Component)interactionMenu3).gameObject.SetActive(true);
			int num7 = interactionMenu3.Index % VisibleCount;
			float num8 = ((!interactionMenu3.IsMajor) ? _minorScale : _majorScale);
			int num9 = _visibleOrder[(num7 + _visibleStartIndex) % VisibleCount];
			float num10 = _visibleStartDegree + (float)num9 * 60f;
			float num11 = num10 * ((float)Math.PI / 180f);
			float num12 = radius + (float)interactionMenu3.Widget.width * 0.5f * (num8 - 1f);
			val.x = Mathf.Sin(num11) * num12;
			val.y = Mathf.Cos(num11) * num12;
			interactionMenu3.MenuRadian = num11;
			if (interactionMenu3.NeedInitAnimation)
			{
				((Component)interactionMenu3).transform.localPosition = Vector3.Lerp(Vector3.zero, Vector2.op_Implicit(val), 0.5f);
				interactionMenu3.Widget.alpha = 0f;
				float delay = (float)num6++ * 0.05f + 0.1f;
				TweenPosition positionTweener = interactionMenu3.PositionTweener;
				positionTweener.from = ((Component)interactionMenu3).transform.localPosition;
				positionTweener.to = Vector2.op_Implicit(val);
				positionTweener.delay = delay;
				positionTweener.tweenFactor = 0f;
				positionTweener.PlayForward();
				TweenAlpha alphaTweener = interactionMenu3.AlphaTweener;
				alphaTweener.from = interactionMenu3.Widget.alpha;
				alphaTweener.to = interactionMenu3.Alpha;
				alphaTweener.delay = delay;
				alphaTweener.tweenFactor = 0f;
				alphaTweener.PlayForward();
			}
			else
			{
				((Component)interactionMenu3).transform.localPosition = Vector2.op_Implicit(val);
			}
			interactionMenu3.UpdateNameLabelPosition();
		}
		if (num > 0)
		{
			_prevArrow.SetActive(true);
			_prevArrow.transform.localPosition = Vector3.left * radius * 1.7f + Vector3.down * 50f;
		}
		else
		{
			_prevArrow.SetActive(false);
		}
		if ((num + 1) * VisibleCount < count)
		{
			_nextArrow.SetActive(true);
			_nextArrow.transform.localPosition = Vector3.right * radius * 1.7f + Vector3.down * 50f;
		}
		else
		{
			_nextArrow.SetActive(false);
		}
	}

	private void OnClickArrow(GameObject go)
	{
		if ((Object)(object)go == (Object)(object)_prevArrow)
		{
			VisiblePage--;
		}
		else if ((Object)(object)go == (Object)(object)_nextArrow)
		{
			VisiblePage++;
		}
		for (int i = 0; i < _menus.Count; i++)
		{
			_menus[i].NeedInitAnimation = true;
		}
		Reposition(instant: true);
	}

	private int IndexOf(InteractionMenuData data)
	{
		for (int i = 0; i < _menus.Count; i++)
		{
			if (_menus[i].Data.IsEqualKey(data))
			{
				return i;
			}
		}
		return -1;
	}

	private void Set(InteractionMenuList list)
	{
		for (int i = 0; i < _menus.Count; i++)
		{
			_menus[i].Valid = false;
		}
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			Add(list[j]);
		}
		for (int num = _menus.Count - 1; num >= 0; num--)
		{
			if (!_menus[num].Valid)
			{
				RemoveAt(num);
			}
		}
	}

	private void Add(InteractionMenuData data)
	{
		int num = IndexOf(data);
		if (num != -1)
		{
			_menus[num].Set(data);
			return;
		}
		InteractionMenu interactionMenu = InteractionMenu_Pop();
		interactionMenu.Set(data);
		_menus.Add(interactionMenu);
		interactionMenu.Index = FindEmptyIndex();
	}

	private void RemoveAll()
	{
		for (int i = 0; i < _menus.Count; i++)
		{
			InteractionMenu_Push(_menus[i]);
		}
		_menus.Clear();
	}

	private void RemoveAt(int index)
	{
		if (index >= 0 && index < _menus.Count)
		{
			InteractionMenu menu = _menus[index];
			_menus.RemoveAt(index);
			InteractionMenu_Push(menu);
		}
	}

	private int FindEmptyIndex()
	{
		int count = _menus.Count;
		int num = 0;
		while (true)
		{
			bool flag = true;
			for (int i = 0; i < count; i++)
			{
				InteractionMenu interactionMenu = _menus[i];
				if ((Object)(object)interactionMenu != (Object)null && interactionMenu.Index == num)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private InteractionMenu InteractionMenu_Pop()
	{
		InteractionMenu interactionMenu;
		if (_interactionMenuPool.Count == 0)
		{
			GameObject val = ((Component)((Component)_interactionMenu).transform.parent).gameObject.AddChild(((Component)_interactionMenu).gameObject);
			interactionMenu = val.GetComponent<InteractionMenu>();
			interactionMenu.OnClickMenu = OnClickMenu;
			interactionMenu.OnLongClickMenu = OnLongClickMenu;
			interactionMenu.OnGatheringQueueClick = OnGatheringQueueClickMenu;
			interactionMenu.Parent = this;
		}
		else
		{
			interactionMenu = _interactionMenuPool.Dequeue();
		}
		((Component)interactionMenu).gameObject.SetActive(true);
		interactionMenu.TouchCollider.enabled = true;
		interactionMenu.NeedInitAnimation = true;
		interactionMenu.Valid = false;
		interactionMenu.Index = -1;
		return interactionMenu;
	}

	private void InteractionMenu_Push(InteractionMenu menu)
	{
		((Component)menu).gameObject.SetActive(false);
		_interactionMenuPool.Enqueue(menu);
	}
}
