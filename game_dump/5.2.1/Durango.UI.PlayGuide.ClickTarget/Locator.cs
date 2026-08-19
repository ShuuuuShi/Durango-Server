using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.PlayGuide.ClickTarget;

public class Locator
{
	protected Dictionary<string, Parameter> Parameters;

	private float _nextVisibleTime;

	private UIWidget _widget;

	private Transform _targetTransform;

	public Parameter CurrentParameter { get; protected set; }

	public Transform TargetTransform
	{
		get
		{
			return _targetTransform;
		}
		protected set
		{
			_targetTransform = value;
			OnChangeTargetTransform();
		}
	}

	public int PanelDepth { get; private set; }

	public int PanelLayer { get; private set; }

	public string CurrentPhase { get; private set; }

	private void OnChangeTargetTransform()
	{
		_widget = ((!(_targetTransform == null)) ? _targetTransform.GetComponent<UIWidget>() : null);
		PanelDepth = 0;
		PanelLayer = LayerHelper.UILayer;
		if (!(_targetTransform == null) && LayerHelper.IsUILayer(_targetTransform.gameObject.layer))
		{
			UIPanel componentInParent = _targetTransform.GetComponentInParent<UIPanel>();
			PanelDepth = componentInParent.depth + 10;
			PanelLayer = _targetTransform.gameObject.layer;
		}
	}

	public virtual void Initialize([NotNull] Dictionary<string, Parameter> dict)
	{
		Parameters = dict;
		OnInitialized();
	}

	public void Process()
	{
		string text = SelectPhase();
		if (CurrentPhase != text)
		{
			TargetTransform = null;
			CurrentParameter = null;
			CurrentPhase = text;
			_nextVisibleTime = Time.time + 0.1f;
		}
		if (CurrentParameter == null)
		{
			CurrentParameter = Parameters.Get(CurrentPhase);
			if (CurrentParameter == null)
			{
				CurrentParameter = new Parameter();
			}
		}
		UpdateTargetTransform();
	}

	public Vector3 GetNGUIPosition()
	{
		if (TargetTransform == null)
		{
			return Vector3.zero;
		}
		if (!LayerHelper.IsUILayer(TargetTransform.gameObject.layer))
		{
			return MainCamera.WorldToNGUIPos(InteractionObject.GetInteractionPosition(TargetTransform.gameObject, ignoreY: false));
		}
		if (_widget == null)
		{
			return UIUtility.ToRootPosition(TargetTransform.gameObject);
		}
		Vector3 localCenter = _widget.localCenter;
		return UIUtility.ToRootPosition(_widget.gameObject, localCenter);
	}

	public Vector2 GetOffset()
	{
		return new Vector2(CurrentParameter.x, CurrentParameter.y);
	}

	public float Rotate()
	{
		return CurrentParameter.rotate;
	}

	public bool IsVisible()
	{
		if (_nextVisibleTime > 0f && Time.time < _nextVisibleTime)
		{
			return false;
		}
		if (TargetTransform != null && TargetTransform.gameObject.activeInHierarchy)
		{
			if (!(_widget == null))
			{
				return _widget.isVisible;
			}
			return true;
		}
		return false;
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
		TargetTransform = Singleton<UIManager>.Instance().FindTransform(CurrentParameter.id);
	}
}
