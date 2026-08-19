using System;
using System.Collections.Generic;
using UnityEngine;

public class BlurMaskingGroup : UIBase
{
	private class BlurMask
	{
		public GameObject Object;

		public IList<UIWidget> Widgets;

		public UIPanel Panel;

		public UIPanel OverPanel;
	}

	public enum CloseTouchObject
	{
		All,
		Masking,
		None
	}

	[SerializeField]
	private UIPanel _nguiOverPanel;

	[SerializeField]
	private UISprite _background;

	private float _closableAt;

	private List<BlurMask> _maskingList = new List<BlurMask>();

	private List<UIPanel> _maskingPanels = new List<UIPanel>();

	private List<UIPanel> _childPanels = new List<UIPanel>();

	private Action _onFinish;

	private int _overLayer;

	private int _nguiLayer;

	public CloseTouchObject CloseMethod { get; set; }

	public float CloseLockTimer { get; set; }

	public bool TouchBoxDisable { get; set; }

	public Func<bool, bool> OnPressBlur { get; set; }

	private void Start()
	{
		((Component)_background).gameObject.SetActive(false);
		_overLayer = LayerMask.NameToLayer("NGUI Over");
		_nguiLayer = LayerMask.NameToLayer("NGUI");
	}

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
	}

	public void Open(Action onFinish)
	{
		_onFinish = onFinish;
		Open();
	}

	public void AddObject(GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return;
		}
		UIPanel componentInParent = obj.GetComponentInParent<UIPanel>();
		if (_maskingPanels.Contains(componentInParent))
		{
			return;
		}
		int i = 0;
		for (int count = _maskingList.Count; i < count; i++)
		{
			BlurMask blurMask = _maskingList[i];
			if ((Object)(object)blurMask.Object == (Object)(object)obj || NGUITools.IsChild(blurMask.Object.transform, obj.transform))
			{
				return;
			}
		}
		for (int num = _maskingList.Count - 1; num >= 0; num--)
		{
			BlurMask blurMask2 = _maskingList[num];
			if (NGUITools.IsChild(obj.transform, blurMask2.Object.transform))
			{
				_maskingList.RemoveAt(num);
				if (base.IsOpen)
				{
					ResetWidgets(blurMask2);
				}
			}
		}
		List<UIWidget> list = new List<UIWidget>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(obj.transform);
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			UIPanel component = ((Component)val).GetComponent<UIPanel>();
			if ((Object)(object)component != (Object)null)
			{
				AddPanel(component, inHierarchy: true);
				continue;
			}
			UIWidget component2 = ((Component)val).GetComponent<UIWidget>();
			if ((Object)(object)component2 != (Object)null)
			{
				list.Add(component2);
			}
			int j = 0;
			for (int childCount = val.childCount; j < childCount; j++)
			{
				stack.Push(val.GetChild(j));
			}
		}
		if (list.Count != 0)
		{
			BlurMask blurMask3 = new BlurMask();
			blurMask3.Object = obj;
			blurMask3.Widgets = list;
			blurMask3.Panel = componentInParent;
			blurMask3.OverPanel = null;
			BlurMask blurMask4 = blurMask3;
			_maskingList.Add(blurMask4);
			if (base.IsOpen)
			{
				MoveToNGUIOver(blurMask4);
			}
		}
	}

	private void AddPanel(UIPanel panel, bool inHierarchy = false)
	{
		if ((Object)(object)panel == (Object)null || _maskingPanels.Contains(panel))
		{
			return;
		}
		if (inHierarchy)
		{
			Stack<Transform> stack = new Stack<Transform>();
			stack.Push(((Component)panel).transform);
			while (stack.Count > 0)
			{
				Transform val = stack.Pop();
				UIPanel component = ((Component)val).GetComponent<UIPanel>();
				if ((Object)(object)component != (Object)null)
				{
					AddPanel(component);
				}
				int i = 0;
				for (int childCount = val.childCount; i < childCount; i++)
				{
					stack.Push(val.GetChild(i));
				}
			}
		}
		_maskingPanels.Add(panel);
		if (base.IsOpen)
		{
			MoveToNGUIOver(panel);
		}
	}

	public void ClearObject()
	{
		if (base.IsOpen)
		{
			int i = 0;
			for (int count = _maskingList.Count; i < count; i++)
			{
				ResetWidgets(_maskingList[i]);
			}
			int j = 0;
			for (int count2 = _childPanels.Count; j < count2; j++)
			{
				Object.Destroy((Object)(object)((Component)_childPanels[j]).gameObject);
			}
			_childPanels.Clear();
			int k = 0;
			for (int count3 = _maskingPanels.Count; k < count3; k++)
			{
				ResetPanel(_maskingPanels[k]);
			}
			_maskingList.Clear();
		}
		_maskingList.Clear();
	}

	protected override bool OnOpen()
	{
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if ((Object)(object)loadingCurtainGroup != (Object)null && loadingCurtainGroup.IsVisible)
		{
			EventDelegate.Add(loadingCurtainGroup.FadeOutFinished, Open, oneShot: true);
			return false;
		}
		if (((Component)this).gameObject.layer != _overLayer)
		{
			NGUITools.SetLayer(((Component)this).gameObject, _overLayer);
		}
		int i = 0;
		for (int count = _maskingList.Count; i < count; i++)
		{
			MoveToNGUIOver(_maskingList[i]);
		}
		int j = 0;
		for (int count2 = _maskingPanels.Count; j < count2; j++)
		{
			MoveToNGUIOver(_maskingPanels[j]);
		}
		((Component)_background).gameObject.SetActive(true);
		_closableAt = Time.time + CloseLockTimer;
		((Collider)((Component)_background).GetComponent<BoxCollider>()).enabled = !TouchBoxDisable;
		BlurController.BlurOn("BlurMask", BlurController.Mask.UI);
		return true;
	}

	protected override bool OnClose()
	{
		if (Time.time < _closableAt)
		{
			return false;
		}
		ClearObject();
		CloseLockTimer = 0f;
		CloseMethod = CloseTouchObject.All;
		TouchBoxDisable = false;
		OnPressBlur = null;
		if (_onFinish != null)
		{
			_onFinish();
			_onFinish = null;
		}
		((Component)_background).gameObject.SetActive(false);
		BlurController.BlurOff("BlurMask");
		return true;
	}

	private void OnPress(GameObject obj, bool pressed)
	{
		if (!base.IsOpen || (OnPressBlur != null && OnPressBlur(pressed)) || pressed)
		{
			return;
		}
		switch (CloseMethod)
		{
		case CloseTouchObject.None:
			break;
		case CloseTouchObject.Masking:
			if (IsTouchOverlay())
			{
				Close();
			}
			break;
		case CloseTouchObject.All:
			Close();
			break;
		}
	}

	public bool IsTouchOverlay()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Ray currentRay = UICamera.currentRay;
		UICamera current = UICamera.current;
		float num = ((!(current.rangeDistance > 0f)) ? (UICamera.currentCamera.farClipPlane - UICamera.currentCamera.nearClipPlane) : current.rangeDistance);
		RaycastHit[] array = Physics.RaycastAll(currentRay, num, (1 << _overLayer) | (1 << _nguiLayer));
		for (int i = 0; i < array.Length; i++)
		{
			if (!NGUITools.IsChild(((Component)this).transform, ((RaycastHit)(ref array[i])).transform))
			{
				if (((Component)((RaycastHit)(ref array[i])).transform).gameObject.layer == _overLayer)
				{
					return true;
				}
				UIWidget component = ((Component)((RaycastHit)(ref array[i])).transform).GetComponent<UIWidget>();
				if (!((Object)(object)component == (Object)null) && !((Object)(object)component.panel == (Object)null) && ((Component)component.panel).gameObject.layer == _overLayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void MoveToNGUIOver(BlurMask mask)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		OnBlurMasking component = mask.Object.GetComponent<OnBlurMasking>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnBlur(enable: true);
		}
		UIPanel panel = mask.Panel;
		UIPanel uIPanel;
		switch (panel.clipping)
		{
		case UIDrawCall.Clipping.SoftClip:
			uIPanel = ((Component)this).gameObject.AddChild<UIPanel>();
			uIPanel.clipping = panel.clipping;
			((Component)uIPanel).transform.position = ((Component)panel).transform.position;
			uIPanel.clipOffset = panel.clipOffset;
			uIPanel.baseClipRegion = panel.baseClipRegion;
			uIPanel.clipSoftness = panel.clipSoftness;
			uIPanel.depth = _nguiOverPanel.depth + panel.depth;
			((Component)uIPanel).gameObject.layer = _overLayer;
			_childPanels.Add(uIPanel);
			break;
		case UIDrawCall.Clipping.TextureMask:
			uIPanel = ((Component)this).gameObject.AddChild<UIPanel>();
			uIPanel.clipping = panel.clipping;
			((Component)uIPanel).transform.position = ((Component)panel).transform.position;
			uIPanel.clipTexture = panel.clipTexture;
			uIPanel.clipOffset = panel.clipOffset;
			uIPanel.baseClipRegion = panel.baseClipRegion;
			uIPanel.depth = _nguiOverPanel.depth + panel.depth;
			((Component)uIPanel).gameObject.layer = _overLayer;
			_childPanels.Add(uIPanel);
			break;
		default:
			uIPanel = _nguiOverPanel;
			break;
		}
		int i = 0;
		for (int count = mask.Widgets.Count; i < count; i++)
		{
			UIWidget uIWidget = mask.Widgets[i];
			panel.RemoveWidget(uIWidget);
			uIPanel.AddWidget(uIWidget);
			uIWidget.panel = uIPanel;
			uIWidget.MarkAsChanged();
		}
		mask.OverPanel = uIPanel;
	}

	private void MoveToNGUIOver(UIPanel panel)
	{
		OnBlurMasking component = ((Component)panel).GetComponent<OnBlurMasking>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnBlur(enable: true);
		}
		((Component)panel).gameObject.layer = _overLayer;
	}

	private void ResetWidgets(BlurMask mask)
	{
		OnBlurMasking onBlurMasking = ((!((Object)(object)mask.Object == (Object)null)) ? mask.Object.GetComponent<OnBlurMasking>() : null);
		if ((Object)(object)onBlurMasking != (Object)null)
		{
			onBlurMasking.OnBlur(enable: false);
		}
		UIPanel panel = mask.Panel;
		int i = 0;
		for (int count = mask.Widgets.Count; i < count; i++)
		{
			UIWidget uIWidget = mask.Widgets[i];
			if (!((Object)(object)uIWidget == (Object)null))
			{
				mask.OverPanel.RemoveWidget(uIWidget);
				panel.AddWidget(uIWidget);
				uIWidget.panel = panel;
				uIWidget.MarkAsChanged();
			}
		}
	}

	private void ResetPanel(UIPanel panel)
	{
		OnBlurMasking component = ((Component)panel).GetComponent<OnBlurMasking>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnBlur(enable: false);
		}
		((Component)panel).gameObject.layer = _nguiLayer;
	}
}
