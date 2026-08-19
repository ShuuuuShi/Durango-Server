using System;
using System.Collections.Generic;
using InteractionData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ContextActionGroup : UIBase
{
	private readonly List<Interaction> _actionList = new List<Interaction>();

	private float _touchLockTime;

	private bool _isShow;

	[SerializeField]
	private ContextActionButtons _actionButtons;

	private Vector3 _baseActionPos;

	public bool IsShow
	{
		get
		{
			return _isShow;
		}
		private set
		{
			if (_isShow != value)
			{
				_isShow = value;
				if (this.OnShowContextAction != null)
				{
					this.OnShowContextAction(_isShow);
				}
			}
		}
	}

	public event Action<bool> OnShowContextAction;

	private void Start()
	{
		_actionButtons.ActionClicked += OnClickActionButton;
		KSingleton<PlayerController>.Instance().MoveStarted += OnStartMove;
		KSingleton<PlayerController>.Instance().MoveEnded += OnEndMove;
		base.OnVisible += delegate(bool visible)
		{
			if (visible)
			{
				HideActionList();
			}
			else
			{
				RefreshActionList();
			}
		};
		base.OnOpenSucceed += RefreshActionList;
		base.OnCloseSucceed += HideActionList;
		UIManager.OnLoadingCurtainHidden(delegate
		{
			if (GameSystem<CombatSystem>.Instance().CombatMode)
			{
				Close();
			}
			else
			{
				Open();
			}
		});
		ToDoListGroup toDoListGroup = UIManager.FindScript<ToDoListGroup>();
		toDoListGroup.WidthRatioChanged += OnChangeTodoWidthRatio;
	}

	private void OnPortraitMode(bool isPortrait)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_baseActionPos = -Vector3.one;
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_baseActionPos == -Vector3.one)
		{
			_baseActionPos = ((Component)_actionButtons).transform.localPosition;
		}
		Vector3 baseActionPos = _baseActionPos;
		baseActionPos.x -= (1f - ratio) * 100f;
		((Component)_actionButtons).transform.localPosition = baseActionPos;
	}

	public void OnClickActionButton(Interaction action)
	{
		if (!(Time.time < _touchLockTime))
		{
			GameSystem<InteractionSystem>.Instance().DoNoneTargetAction(action);
			_touchLockTime = Time.time + 0.5f;
			HideActionList();
		}
	}

	private void OnStartMove()
	{
		HideActionList();
	}

	private void OnEndMove()
	{
		RefreshActionList();
	}

	private void HideActionList()
	{
		_actionButtons.Hide();
		IsShow = false;
	}

	private void RefreshActionList()
	{
		if (PlayerBehavior.LocalPlayer.IsMoving)
		{
			HideActionList();
			return;
		}
		GameSystem<InteractionSystem>.Instance().GetPlayerActionList(_actionList);
		_actionButtons.SetActions(_actionList);
		IsShow = _actionList.Count > 0;
	}

	public void RefreshSearchWarpholeCooltime(double searchedAt)
	{
		double reactiveAt = searchedAt + (double)Singleton<Constants>.Instance.exploring.search_cooltime;
		_actionButtons.SetActionCooltime(Interaction.SearchWarphole, searchedAt, reactiveAt);
	}
}
