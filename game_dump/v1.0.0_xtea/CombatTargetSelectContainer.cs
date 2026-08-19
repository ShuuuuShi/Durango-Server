using System.Collections;
using UnityEngine;

public class CombatTargetSelectContainer : MonoBehaviour
{
	private enum Mode
	{
		Hide,
		Show,
		FixForCombat
	}

	[SerializeField]
	private float _checkCoolTime = 5f;

	[SerializeField]
	private CombatTargetSelector _targetSelector;

	[SerializeField]
	private CombatTargetSelectorHandle _handle;

	[SerializeField]
	private TweenPosition _tweenPosition;

	private WaitForSeconds _waits;

	private CombatTargetSearcher _searcher = new CombatTargetSearcher();

	private GameObject _currentTarget;

	private GameObject _focusTarget;

	private Vector3 _hidePosition;

	private Mode _currentMode;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		_waits = new WaitForSeconds(_checkCoolTime);
		_hidePosition = new Vector3((float)((Component)this).GetComponent<UIWidget>().width, 0f, 0f);
		_targetSelector.Init();
		_targetSelector.TargetWidgetClicked += _targetSelector_TargetWidgetClicked;
		_targetSelector.ScrollViewUpdated += _targetSelector_ScrollViewUpdated;
		_tweenPosition.AddOnFinished(OnFinishedTweenPosition);
		_handle.Clicked += _handle_Clicked;
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(SearchTargets());
	}

	public void SetCurrentTarget(GameObject target)
	{
		if (!((Object)(object)_currentTarget == (Object)(object)target))
		{
			_currentTarget = target;
			CreateAdditionalTargetIfNecessary(_currentTarget);
			RemoveUnnecessaryAdditionalTarget();
			if (_currentMode != 0)
			{
				_targetSelector.SetFocusTarget(null);
				_targetSelector.SetCurrentTarget(_currentTarget);
			}
			_focusTarget = null;
			RefreshCameraForForcusTarget();
		}
	}

	public void SetFocusTarget(GameObject target, bool refreshInteractionMenu = false)
	{
		if (!((Object)(object)_focusTarget == (Object)(object)target))
		{
			_focusTarget = target;
			if (refreshInteractionMenu)
			{
				RefreshInteractionForFocusTarget();
			}
			RefreshCameraForForcusTarget();
			CreateAdditionalTargetIfNecessary(_focusTarget);
			RemoveUnnecessaryAdditionalTarget();
			if (_currentMode != 0)
			{
				_targetSelector.SetFocusTarget(_focusTarget);
			}
		}
	}

	public void SetCombatMode(bool combatMode)
	{
		switch (_currentMode)
		{
		case Mode.Hide:
			if (combatMode)
			{
				SetCurrentMode(Mode.FixForCombat);
				RefreshTargetWidgets();
			}
			break;
		case Mode.Show:
			if (combatMode)
			{
				SetCurrentMode(Mode.FixForCombat);
				_targetSelector.SetCurrentTarget(_currentTarget);
			}
			break;
		case Mode.FixForCombat:
			if (!combatMode)
			{
				SetCurrentMode(Mode.Hide);
			}
			break;
		}
		_targetSelector.RefreshSelectButtons(combatMode);
	}

	private IEnumerator SearchTargets()
	{
		while (true)
		{
			_searcher.SearchTargets(2000f);
			if (_searcher.Count > 0)
			{
				if ((Object)(object)_focusTarget != (Object)null && !_searcher.Contains(_focusTarget))
				{
					_focusTarget = null;
					RefreshCameraForForcusTarget();
				}
				RefreshTargetWidgets(scrollToSelected: false);
			}
			else
			{
				if ((Object)(object)_focusTarget != (Object)null)
				{
					_focusTarget = null;
					RefreshCameraForForcusTarget();
				}
				if (_currentMode == Mode.Show)
				{
					SetCurrentMode(Mode.Hide);
				}
			}
			RefreshHandleColor();
			yield return _waits;
		}
	}

	private void RefreshTargetWidgets(bool scrollToSelected = true)
	{
		if (_currentMode != 0)
		{
			_targetSelector.RefreshTargetWidgets(_searcher.TargetEntities, _searcher.AdditionalTargetEntity, _currentTarget, _focusTarget, scrollToSelected);
		}
	}

	private void CreateAdditionalTargetIfNecessary(GameObject target)
	{
		if ((Object)(object)target != (Object)null && !_searcher.Contains(target) && _searcher.CreateAdditionalTargetEntity(target) && _currentMode != 0)
		{
			_targetSelector.CreateAdditionalTargetWidget(_searcher.AdditionalTargetEntity);
		}
	}

	private void RemoveUnnecessaryAdditionalTarget()
	{
		if (_searcher.AdditionalTargetEntity != null && (Object)(object)_searcher.AdditionalTargetEntity.GameObject != (Object)(object)_currentTarget && (Object)(object)_searcher.AdditionalTargetEntity.GameObject != (Object)(object)_focusTarget)
		{
			if (_currentMode != 0)
			{
				_targetSelector.RemoveAdditionalTargetWidget();
			}
			_searcher.RemoveAdditionalTargetEntity();
		}
	}

	private void SetCurrentMode(Mode mode)
	{
		_currentMode = mode;
		switch (_currentMode)
		{
		case Mode.Hide:
			HideContainer();
			UIManager.FindScript<ToDoListGroup>().SetVisible(visible: true);
			break;
		case Mode.Show:
			ShowContainer(instant: false);
			UIManager.FindScript<ToDoListGroup>().SetVisible(visible: false);
			break;
		case Mode.FixForCombat:
			ShowContainer(instant: true);
			break;
		}
		RefreshHandleColor();
	}

	private void RefreshInteractionForFocusTarget()
	{
		if (_currentMode != Mode.FixForCombat)
		{
			if ((Object)(object)_focusTarget != (Object)null)
			{
				InteractionObject interactionTarget = new InteractionObject(_focusTarget);
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(interactionTarget);
			}
			else
			{
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			}
		}
	}

	private void RefreshCameraForForcusTarget()
	{
		CameraController cameraController = KSingleton<CameraController>.Instance();
		if ((Object)(object)_focusTarget != (Object)null)
		{
			cameraController.SetCameraTarget(_focusTarget);
		}
		else if ((Object)(object)cameraController.CurrentCameraTarget != (Object)null)
		{
			cameraController.ResetCameraTarget();
		}
	}

	private void ShowContainer(bool instant)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(_hidePosition.x - _targetSelector.LengthOfWidgetsWithMargin, 0f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num, 0f, 0f);
		if (((Component)_targetSelector).transform.localPosition != val)
		{
			if (instant)
			{
				((Component)_targetSelector).transform.localPosition = val;
				return;
			}
			_tweenPosition.from = ((Component)_targetSelector).transform.localPosition;
			_tweenPosition.to = val;
			_tweenPosition.tweenFactor = 0f;
			_tweenPosition.PlayForward();
		}
	}

	private void HideContainer()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)_targetSelector).transform.localPosition != _hidePosition)
		{
			_tweenPosition.from = ((Component)_targetSelector).transform.localPosition;
			_tweenPosition.to = _hidePosition;
			_tweenPosition.tweenFactor = 0f;
			_tweenPosition.PlayForward();
		}
	}

	private void OnFinishedTweenPosition()
	{
		if (_currentMode == Mode.Hide)
		{
			_targetSelector.ClearTargetWidgets();
		}
	}

	private void RefreshHandleColor()
	{
		if (_currentMode != Mode.FixForCombat)
		{
			if (_searcher.Count > 0)
			{
				_handle.Show(show: true);
			}
			else
			{
				_handle.Show(show: false);
			}
		}
		else
		{
			_handle.Show(show: false);
		}
	}

	private void _targetSelector_TargetWidgetClicked(GameObject target)
	{
		SetFocusTarget(target, refreshInteractionMenu: true);
	}

	private void _targetSelector_ScrollViewUpdated()
	{
		switch (_currentMode)
		{
		case Mode.Show:
			ShowContainer(instant: false);
			break;
		case Mode.FixForCombat:
			ShowContainer(instant: true);
			break;
		}
	}

	private void _handle_Clicked()
	{
		switch (_currentMode)
		{
		case Mode.Hide:
			if (_searcher.Count > 0)
			{
				SetCurrentMode(Mode.Show);
				RefreshTargetWidgets();
				_targetSelector.SetFocusTarget(_focusTarget);
			}
			break;
		case Mode.Show:
			SetCurrentMode(Mode.Hide);
			break;
		}
	}
}
