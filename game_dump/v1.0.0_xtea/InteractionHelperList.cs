using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHelperList : MonoBehaviour
{
	[SerializeField]
	private InteractionHelperLabel _baseHelper;

	[SerializeField]
	private float _hideDelay;

	private List<GameObject> _objectBuffer = new List<GameObject>();

	private ListObjectPool<InteractionHelperLabel> _helpers;

	private AnimationWidget _animWidget;

	private float _hideAt;

	private int _showFrame;

	private bool _isShow;

	private bool _isInit;

	public bool IsShow => _isShow;

	public event Action ShowStateChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_helpers = new ListObjectPool<InteractionHelperLabel>();
			_helpers.BaseObject = _baseHelper;
			_helpers.Init(OnInitHelperLabel);
			_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			_animWidget.SetAlpha(0f, useTween: false);
			((Component)this).gameObject.SetActive(false);
			KSingleton<PlayerController>.Instance().MoveStarted += Hide;
		}
	}

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPressScreen));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPressScreen));
	}

	private void Update()
	{
		UpdatePosition();
		if (_hideAt > 0f && _hideAt < Time.time)
		{
			Hide();
		}
	}

	private void OnInitHelperLabel(InteractionHelperLabel lb)
	{
		lb.Clicked = (Action)Delegate.Combine(lb.Clicked, new Action(OnClickHelperLabel));
	}

	private void OnPressScreen(GameObject obj, bool press)
	{
		if (_showFrame != Time.frameCount && press)
		{
			Hide((!NGUITools.IsChild(((Component)this).transform, (!((Object)(object)obj == (Object)null)) ? obj.transform : null)) ? 0f : _hideDelay);
		}
	}

	private void OnClickHelperLabel()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, ((Component)_baseHelper).transform));
		InteractionHelperLabel interactionHelperLabel = null;
		float num = float.MaxValue;
		for (int i = 0; i < _helpers.Count; i++)
		{
			Vector3 localPosition = ((Component)_helpers[i]).transform.localPosition;
			Vector3 val2 = localPosition - val;
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				interactionHelperLabel = _helpers[i];
				num = sqrMagnitude;
			}
		}
		if (!((Object)(object)interactionHelperLabel == (Object)null) && !((Object)(object)interactionHelperLabel.Target == (Object)null))
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(new InteractionObject(interactionHelperLabel.Target));
		}
	}

	public void Show()
	{
		Init();
		if (!_isShow)
		{
			_isShow = true;
			((Component)this).gameObject.SetActive(true);
			_animWidget.SetAlpha(1f, useTween: false);
			if (this.ShowStateChanged != null)
			{
				this.ShowStateChanged();
			}
		}
		_showFrame = Time.frameCount;
		_hideAt = 0f;
		RefreshHelpers();
	}

	private void Hide(float delay)
	{
		_hideAt = Time.time + delay;
	}

	private void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			_animWidget.Alpha = 0f;
			if (this.ShowStateChanged != null)
			{
				this.ShowStateChanged();
			}
		}
	}

	private void RefreshHelpers()
	{
		_helpers.Clear();
		_objectBuffer.Clear();
		InteractionSystem.SearchMovableObjects(_objectBuffer);
		MakeHelpers(_objectBuffer);
		_objectBuffer.Clear();
		InteractionSystem.SearchPropObjects(_objectBuffer);
		MakeHelpers(_objectBuffer);
	}

	private void MakeHelpers(IList<GameObject> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			InteractionHelperLabel interactionHelperLabel = _helpers.Add();
			interactionHelperLabel.Set(list[i]);
		}
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		int i = 0;
		for (int count = _helpers.Count; i < count; i++)
		{
			_helpers[i].UpdatePosition();
		}
	}
}
