using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class BlurMaskingGroup : UIBase
{
	public enum CloseTouchObject
	{
		All,
		Masking,
		None
	}

	private class BlurMask
	{
		public GameObject Object;

		public IList<WidgetObject> Widgets;

		public UIPanel Panel;
	}

	private struct WidgetObject
	{
		public UIWidget Widget;

		public UIPanel DrawPanel;

		public WidgetObject(UIWidget widget)
		{
			Widget = widget;
			DrawPanel = widget.DrawPanel;
		}
	}

	[SerializeField]
	private UIPanel _nguiOverPanel;

	private float _closableAt;

	private readonly List<BlurMask> _maskingList = new List<BlurMask>();

	private readonly List<UIPanel> _maskingPanels = new List<UIPanel>();

	private readonly List<UIPanel> _childPanels = new List<UIPanel>();

	private Action _onFinish;

	private int _overLayer;

	private int _nguiLayer;

	public CloseTouchObject CloseMethod { get; set; }

	public float CloseLockTimer { get; set; }

	public bool TouchBoxDisable { get; set; }

	public Func<bool, bool> OnPressBlur { get; set; }

	private void Start()
	{
		_background.gameObject.SetActive(value: false);
		_overLayer = LayerHelper.UIOverLayer;
		_nguiLayer = LayerHelper.UILayer;
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
		if (obj == null)
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
			if (blurMask.Object == obj || NGUITools.IsChild(blurMask.Object.transform, obj.transform))
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
				if (base.IsOpened)
				{
					ResetWidgets(blurMask2);
				}
			}
		}
		List<WidgetObject> list = new List<WidgetObject>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(obj.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			UIPanel component = transform.GetComponent<UIPanel>();
			if (component != null)
			{
				AddPanelInHierarchy(component);
				continue;
			}
			UIWidget component2 = transform.GetComponent<UIWidget>();
			if (component2 != null)
			{
				list.Add(new WidgetObject(component2));
			}
			int j = 0;
			for (int childCount = transform.childCount; j < childCount; j++)
			{
				stack.Push(transform.GetChild(j));
			}
		}
		if (list.Count != 0)
		{
			BlurMask blurMask3 = new BlurMask();
			blurMask3.Object = obj;
			blurMask3.Widgets = list;
			blurMask3.Panel = componentInParent;
			BlurMask blurMask4 = blurMask3;
			_maskingList.Add(blurMask4);
			if (base.IsOpened)
			{
				MoveToNGUIOver(blurMask4);
			}
		}
	}

	private void AddPanelInHierarchy(UIPanel panel)
	{
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(panel.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			UIPanel component = transform.GetComponent<UIPanel>();
			if (component != null)
			{
				AddPanel(component);
			}
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				stack.Push(transform.GetChild(i));
			}
		}
	}

	private void AddPanel(UIPanel panel)
	{
		if (!(panel == null) && !_maskingPanels.Contains(panel))
		{
			_maskingPanels.Add(panel);
			if (base.IsOpened)
			{
				MoveToNGUIOver(panel);
			}
		}
	}

	public void ClearObject()
	{
		if (base.IsOpened)
		{
			int i = 0;
			for (int count = _maskingList.Count; i < count; i++)
			{
				ResetWidgets(_maskingList[i]);
			}
			int j = 0;
			for (int count2 = _childPanels.Count; j < count2; j++)
			{
				UnityEngine.Object.Destroy(_childPanels[j].gameObject);
			}
			int k = 0;
			for (int count3 = _maskingPanels.Count; k < count3; k++)
			{
				ResetPanel(_maskingPanels[k]);
			}
		}
		_childPanels.Clear();
		_maskingPanels.Clear();
		_maskingList.Clear();
	}

	private new void Open()
	{
		base.Open();
	}

	protected override bool TryOpen()
	{
		if (UIManager.IsLoadingCurtain)
		{
			UIManager.OnLoadingCurtainHidden(Open);
			return false;
		}
		if (base.gameObject.layer != _overLayer)
		{
			NGUITools.SetLayer(base.gameObject, _overLayer);
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
		_background.gameObject.SetActive(value: true);
		_closableAt = Time.time + CloseLockTimer;
		_background.GetComponent<BoxCollider>().enabled = !TouchBoxDisable;
		BlurController.BlurOn("BlurMask", BlurController.Mask.UI);
		return true;
	}

	protected override bool TryClose()
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
		_background.gameObject.SetActive(value: false);
		BlurController.BlurOff("BlurMask");
		return true;
	}

	private void OnPress(GameObject obj, bool pressed)
	{
		if (!base.IsOpened || (OnPressBlur != null && OnPressBlur(pressed)) || pressed)
		{
			return;
		}
		switch (CloseMethod)
		{
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
		Ray currentRay = UICamera.currentRay;
		UICamera current = UICamera.current;
		float dist = ((!(current.rangeDistance > 0f)) ? (UICamera.currentCamera.farClipPlane - UICamera.currentCamera.nearClipPlane) : current.rangeDistance);
		int count;
		RaycastHit[] array = Collisions.RayCast(currentRay, dist, (1 << _overLayer) | (1 << _nguiLayer), out count);
		for (int i = 0; i < count; i++)
		{
			Transform transform = array[i].transform;
			if (!NGUITools.IsChild(base.transform, transform))
			{
				if (transform.gameObject.layer == _overLayer)
				{
					return true;
				}
				UIWidget component = transform.GetComponent<UIWidget>();
				if (!(component == null) && !(component.panel == null) && component.panel.gameObject.layer == _overLayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void MoveToNGUIOver(BlurMask mask)
	{
		OnBlurMasking component = mask.Object.GetComponent<OnBlurMasking>();
		if (component != null)
		{
			component.OnBlur(enable: true);
		}
		UIPanel panel = mask.Panel;
		UIPanel uIPanel;
		switch (panel.clipping)
		{
		case UIDrawCall.Clipping.SoftClip:
			uIPanel = base.gameObject.AddChild<UIPanel>();
			uIPanel.clipping = panel.clipping;
			uIPanel.transform.position = panel.transform.position;
			uIPanel.clipOffset = panel.clipOffset;
			uIPanel.baseClipRegion = panel.baseClipRegion;
			uIPanel.clipSoftness = panel.clipSoftness;
			uIPanel.depth = _nguiOverPanel.depth + panel.depth;
			uIPanel.gameObject.layer = _overLayer;
			_childPanels.Add(uIPanel);
			break;
		case UIDrawCall.Clipping.TextureMask:
			uIPanel = base.gameObject.AddChild<UIPanel>();
			uIPanel.clipping = panel.clipping;
			uIPanel.transform.position = panel.transform.position;
			uIPanel.clipTexture = panel.clipTexture;
			uIPanel.clipOffset = panel.clipOffset;
			uIPanel.baseClipRegion = panel.baseClipRegion;
			uIPanel.depth = _nguiOverPanel.depth + panel.depth;
			uIPanel.gameObject.layer = _overLayer;
			_childPanels.Add(uIPanel);
			break;
		default:
			uIPanel = _nguiOverPanel;
			break;
		}
		int i = 0;
		for (int count = mask.Widgets.Count; i < count; i++)
		{
			mask.Widgets[i].Widget.DrawPanel = uIPanel;
		}
	}

	private void MoveToNGUIOver(UIPanel panel)
	{
		OnBlurMasking component = panel.GetComponent<OnBlurMasking>();
		if (component != null)
		{
			component.OnBlur(enable: true);
		}
		panel.gameObject.layer = _overLayer;
		panel.BroadcastMessage("CheckLayer", SendMessageOptions.DontRequireReceiver);
	}

	private void ResetWidgets(BlurMask mask)
	{
		OnBlurMasking onBlurMasking = ((!(mask.Object == null)) ? mask.Object.GetComponent<OnBlurMasking>() : null);
		if (onBlurMasking != null)
		{
			onBlurMasking.OnBlur(enable: false);
		}
		int i = 0;
		for (int count = mask.Widgets.Count; i < count; i++)
		{
			WidgetObject widgetObject = mask.Widgets[i];
			if (!(widgetObject.Widget == null))
			{
				widgetObject.Widget.DrawPanel = widgetObject.DrawPanel;
			}
		}
	}

	private void ResetPanel(UIPanel panel)
	{
		OnBlurMasking component = panel.GetComponent<OnBlurMasking>();
		if (component != null)
		{
			component.OnBlur(enable: false);
		}
		panel.gameObject.layer = _nguiLayer;
		panel.BroadcastMessage("CheckLayer", SendMessageOptions.DontRequireReceiver);
	}
}
