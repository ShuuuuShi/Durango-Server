using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocator
{
	protected Dictionary<string, ClickTargetData> ClickTargetDict;

	protected ClickTargetData CurrentClickTarget;

	private float _nextVisibleTime;

	private UIWidget _widget;

	private float _updateTrasformAt = -1f;

	private Transform _targetTransform;

	public Transform TargetTransform
	{
		get
		{
			return _targetTransform;
		}
		protected set
		{
			_targetTransform = value;
			_widget = ((!((Object)(object)value == (Object)null)) ? ((Component)value).GetComponent<UIWidget>() : null);
		}
	}

	public string CurrentPhase { get; private set; }

	public virtual void Initialize([NotNull] Dictionary<string, ClickTargetData> dict)
	{
		ClickTargetDict = dict;
		OnInitialized();
	}

	public void Process()
	{
		string text = SelectPhase();
		if (CurrentPhase != text)
		{
			TargetTransform = null;
			CurrentClickTarget = null;
			CurrentPhase = text;
			_nextVisibleTime = Time.time + 0.1f;
		}
		if (CurrentClickTarget == null)
		{
			CurrentClickTarget = ClickTargetDict.Get(CurrentPhase);
			if (CurrentClickTarget == null)
			{
				CurrentClickTarget = new ClickTargetData();
			}
		}
		UpdateTargetTransform();
	}

	public Vector3 GetNGUIPosition()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetTransform == (Object)null)
		{
			return Vector3.zero;
		}
		int layer = ((Component)TargetTransform).gameObject.layer;
		if (UIManager.IsUILayer(layer))
		{
			Vector3 result = MainCamera.NGUILocalPositionToNGUIPosition(TargetTransform.localPosition, TargetTransform.parent);
			result.x += 5f;
			result.y -= 5f;
			return result;
		}
		return MainCamera.WorldToNGUIPos(KUtility.GetInteractionPosition(((Component)TargetTransform).gameObject, ignoreY: false));
	}

	public Vector2 GetOffset()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(CurrentClickTarget.x, CurrentClickTarget.y);
	}

	public bool IsFlip()
	{
		return CurrentClickTarget.flip;
	}

	public float Rotate()
	{
		return CurrentClickTarget.rotate;
	}

	public bool IsVisible()
	{
		if (_nextVisibleTime > 0f && Time.time < _nextVisibleTime)
		{
			return false;
		}
		if (GameSystem<TimerSystem>.Instance().HasTimerExceptPostProcess())
		{
			return false;
		}
		return (Object)(object)TargetTransform != (Object)null && ((Component)TargetTransform).gameObject.activeInHierarchy && ((Object)(object)_widget == (Object)null || _widget.isVisible);
	}

	protected virtual void OnInitialized()
	{
	}

	protected virtual string SelectPhase()
	{
		return "current";
	}

	protected virtual void UpdateTargetTransform()
	{
		TargetTransform = KSingleton<UIManager>.Instance().FindTransform(CurrentClickTarget.id);
	}
}
