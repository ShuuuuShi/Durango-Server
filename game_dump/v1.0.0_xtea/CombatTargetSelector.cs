using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CombatTargetSelector : MonoBehaviour
{
	[SerializeField]
	private KScrollView _kScrollView;

	[SerializeField]
	private UIPanel _uiPanel;

	private bool _initialized;

	private Vector2 _panelInnerSize;

	private float? _refreshScrollViewOffset;

	private bool _existAdditionalTarget;

	public float LengthOfWidgetsWithMargin { get; private set; }

	public event Action<GameObject> TargetWidgetClicked;

	public event Action ScrollViewUpdated;

	private void Update()
	{
		UpdateScrollView();
		UpdateTargetWidgets();
	}

	public void Init()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			_kScrollView.Nodes.Init(delegate(GameObject obj)
			{
				UIEventListener.Get(obj).onClick = OnTargetWidgetClick;
				CombatTargetWidget component = obj.GetComponent<CombatTargetWidget>();
				component.Init();
				component.SelectButtonClicked = (Action<CombatTargetWidget>)Delegate.Combine(component.SelectButtonClicked, new Action<CombatTargetWidget>(targetWidget_SelectTargetClicked));
				component.CancelButtonClicked = (Action<CombatTargetWidget>)Delegate.Combine(component.CancelButtonClicked, new Action<CombatTargetWidget>(targetWidget_CancelClicked));
			});
			_panelInnerSize = UIUtility.PanelInnerSize(_kScrollView.Panel);
			_initialized = true;
		}
	}

	public void ClearTargetWidgets()
	{
		_kScrollView.Nodes.Clear();
		_existAdditionalTarget = false;
	}

	public void RefreshTargetWidgets(IList<DamageableEntity> targetEntities, DamageableEntity additionalTargetEntity, GameObject currentTarget, GameObject focusTarget, bool scrollToCurrentTarget = true)
	{
		float currentOffset = _kScrollView.CurrentOffset;
		bool combatMode = GameSystem<CombatSystem>.Instance().CombatMode;
		_kScrollView.Nodes.Set(targetEntities.Count);
		_existAdditionalTarget = false;
		int num = -1;
		for (int i = 0; i < targetEntities.Count; i++)
		{
			DamageableEntity damageableEntity = targetEntities[i];
			CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(i);
			combatTargetWidget.SetEntity(damageableEntity, combatMode);
			combatTargetWidget.IsExtended = (Object)(object)damageableEntity.GameObject == (Object)(object)focusTarget;
			if ((Object)(object)damageableEntity.GameObject == (Object)(object)currentTarget)
			{
				combatTargetWidget.IsSelected = true;
				num = i;
			}
			else
			{
				combatTargetWidget.IsSelected = false;
			}
		}
		if (additionalTargetEntity != null)
		{
			CombatTargetWidget combatTargetWidget2 = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Add<CombatTargetWidget>();
			combatTargetWidget2.SetEntity(additionalTargetEntity, combatMode);
			combatTargetWidget2.IsExtended = (Object)(object)additionalTargetEntity.GameObject == (Object)(object)focusTarget;
			if ((Object)(object)additionalTargetEntity.GameObject == (Object)(object)currentTarget)
			{
				combatTargetWidget2.IsSelected = true;
				num = _kScrollView.Nodes.Count - 1;
			}
			else
			{
				combatTargetWidget2.IsSelected = false;
			}
			_existAdditionalTarget = true;
		}
		_refreshScrollViewOffset = ((!scrollToCurrentTarget || num == -1) ? currentOffset : GetScrollOffset(num));
	}

	public void SetCurrentTarget(GameObject currentTarget)
	{
		int num = -1;
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(i);
			if (combatTargetWidget.Entity != null)
			{
				if ((Object)(object)combatTargetWidget.Entity.GameObject == (Object)(object)currentTarget)
				{
					combatTargetWidget.IsSelected = true;
					num = i;
				}
				else
				{
					combatTargetWidget.IsSelected = false;
				}
			}
		}
		if (num != -1)
		{
			_refreshScrollViewOffset = GetScrollOffset(num);
		}
	}

	public void SetFocusTarget(GameObject focusTarget)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(i);
			if (combatTargetWidget.Entity != null)
			{
				if (combatTargetWidget.IsExtended)
				{
					num = i;
				}
				if ((Object)(object)combatTargetWidget.Entity.GameObject == (Object)(object)focusTarget)
				{
					combatTargetWidget.IsExtended = true;
					num2 = i;
				}
				else
				{
					combatTargetWidget.IsExtended = false;
				}
			}
		}
		int num3 = ((!((Object)(object)focusTarget != (Object)null)) ? num : num2);
		if (num3 != -1)
		{
			_refreshScrollViewOffset = GetScrollOffset(num3);
		}
	}

	public void CreateAdditionalTargetWidget([NotNull] DamageableEntity targetEntity)
	{
		RemoveAdditionalTargetWidget();
		CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Add<CombatTargetWidget>();
		combatTargetWidget.SetEntity(targetEntity, GameSystem<CombatSystem>.Instance().CombatMode);
		combatTargetWidget.IsSelected = false;
		combatTargetWidget.IsExtended = false;
		_refreshScrollViewOffset = GetScrollOffset(_kScrollView.Nodes.Count - 1);
		_existAdditionalTarget = true;
	}

	public void RemoveAdditionalTargetWidget()
	{
		if (_existAdditionalTarget && _kScrollView.Nodes.Count > 0)
		{
			_kScrollView.Nodes.Set(_kScrollView.Nodes.Count - 1);
			_refreshScrollViewOffset = ((_kScrollView.Nodes.Count <= 0) ? 0f : GetScrollOffset(_kScrollView.Nodes.Count - 1));
			_existAdditionalTarget = false;
		}
	}

	public void RefreshSelectButtons(bool combatMode)
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(i);
			combatTargetWidget.RefreshSelectButton(combatMode);
		}
	}

	private float GetScrollOffset(int index)
	{
		float num = _kScrollView.GetNodeOffset(index);
		CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(index);
		if ((Object)(object)combatTargetWidget != (Object)null)
		{
			num -= (_panelInnerSize.x - (float)((Component)combatTargetWidget).GetComponent<UIWidget>().width) / 2f;
		}
		return num;
	}

	private void UpdateTargetWidgets()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			CombatTargetWidget combatTargetWidget = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<CombatTargetWidget>(i);
			combatTargetWidget.UpdateWidget(currentPosition);
		}
	}

	private void UpdateScrollView()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (_refreshScrollViewOffset.HasValue)
		{
			float num = _kScrollView.UpdateLayout();
			if (num > _panelInnerSize.x)
			{
				_kScrollView.MoveTo(_refreshScrollViewOffset.Value, instant: false);
				((Behaviour)_kScrollView.ScrollView).enabled = true;
			}
			else
			{
				_kScrollView.MoveTo(0f, instant: false, restrictWithinPanel: false);
				((Behaviour)_kScrollView.ScrollView).enabled = false;
			}
			_refreshScrollViewOffset = null;
			LengthOfWidgetsWithMargin = num + _uiPanel.clipSoftness.x * 2f;
			if (this.ScrollViewUpdated != null)
			{
				this.ScrollViewUpdated();
			}
		}
	}

	private void OnTargetWidgetClick(GameObject obj)
	{
		CombatTargetWidget component = obj.GetComponent<CombatTargetWidget>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		if (component.IsExtended)
		{
			if (component.IsSelected && this.TargetWidgetClicked != null)
			{
				this.TargetWidgetClicked(null);
			}
		}
		else if (component.Entity != null && this.TargetWidgetClicked != null)
		{
			this.TargetWidgetClicked(component.Entity.GameObject);
		}
	}

	private void targetWidget_SelectTargetClicked(CombatTargetWidget widget)
	{
		if (!(widget.Entity != null))
		{
			return;
		}
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		if (combatSystem.CombatMode)
		{
			GameObject gameObject = widget.Entity.GameObject;
			if ((Object)(object)gameObject != (Object)null && (Object)(object)combatSystem.Target != (Object)(object)gameObject)
			{
				combatSystem.RequestChangeTarget(gameObject);
			}
		}
		else
		{
			combatSystem.TryServerSideBattleEnter(widget.Entity.GetEntityId());
		}
	}

	private void targetWidget_CancelClicked(CombatTargetWidget widget)
	{
		if (widget.IsExtended && this.TargetWidgetClicked != null)
		{
			this.TargetWidgetClicked(null);
		}
	}
}
