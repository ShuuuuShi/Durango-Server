using System;
using Estate;
using L10N;
using Messages;
using Shared.Estate;
using UnityEngine;

public class EstateLicenseNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private EstateLicensePresetSelector _presetSelector;

	[SerializeField]
	private AnimationWidget _rightsWidget;

	[SerializeField]
	private ListObjectPool _rights;

	[SerializeField]
	private GameObject _opener;

	[SerializeField]
	private UISprite _openerSprite;

	[SerializeField]
	private UIWidget _separator;

	private WidgetLayoutController _layoutController;

	private bool _isInit;

	public bool IsExtend { get; private set; }

	public Estate.LicenseCategory Category { get; private set; }

	public MemberRole ClanRole { get; private set; }

	public AccessRights Right { get; private set; }

	public bool IsChanged { get; private set; }

	public event Action<EstateLicenseNode> ExtendViewStateChanged;

	private void Init()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_layoutController = ((Component)this).GetComponent<WidgetLayoutController>();
			_presetSelector.RightChanged += OnSelectPreset;
			UIEventListener uIEventListener = UIEventListener.Get(_opener);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickOpener));
			_rights.Init(delegate(GameObject o)
			{
				EstateRightNode component2 = o.GetComponent<EstateRightNode>();
				component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickRightNode));
			});
			AccessRights[] rightsList = EstateSystem.RightsList;
			_rights.Set(rightsList.Length);
			for (int i = 0; i < _rights.Count; i++)
			{
				EstateRightNode component = _rights[i].GetComponent<EstateRightNode>();
				component.Set(rightsList[i]);
				component.EnableSeparator(i < _rights.Count - 1);
			}
			float num = _rights.Reposition(Vector3.down);
			_rightsWidget.Widget.height = (int)num;
			SetExtendView(show: false, instant: true);
		}
	}

	private void OnClickRightNode()
	{
		EstateRightNode estateRightNode = Selectable.Current as EstateRightNode;
		if (!((Object)(object)estateRightNode == (Object)null))
		{
			if ((Right & estateRightNode.Right) == 0)
			{
				Right |= estateRightNode.Right;
			}
			else
			{
				Right &= ~estateRightNode.Right;
			}
			UpdateRights();
		}
	}

	public void Set(Estate.LicenseCategory category, AccessRights right)
	{
		Init();
		_titleLabel.text = category.GetName();
		Category = category;
		ClanRole = default(MemberRole);
		Right = right;
		UpdateRights();
		IsChanged = false;
	}

	public void Set(MemberRole role, AccessRights right)
	{
		Init();
		_titleLabel.text = role.Name;
		Category = Estate.LicenseCategory.Clan;
		ClanRole = role;
		Right = right;
		UpdateRights();
		IsChanged = false;
	}

	private void UpdateRights()
	{
		_presetSelector.Set(Right);
		for (int i = 0; i < _rights.Count; i++)
		{
			EstateRightNode component = _rights[i].GetComponent<EstateRightNode>();
			component.Select = (Right & component.Right) != 0;
		}
		IsChanged = true;
	}

	private void OnSelectPreset(AccessRights right)
	{
		Right = right;
		UpdateRights();
	}

	private void OnClickOpener(GameObject obj)
	{
		SetExtendView(!IsExtend, instant: false);
	}

	private void SetExtendView(bool show, bool instant)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		IsExtend = show;
		Quaternion val = Quaternion.Euler(Vector3.forward * ((!show) ? 0f : 180f));
		if (instant)
		{
			((Component)(object)_openerSprite).SetEnable<TweenRotation>(enable: false);
			((Component)_openerSprite).transform.localRotation = val;
			((Component)_rightsWidget).gameObject.SetActive(show);
			_rightsWidget.SetAlpha((!show) ? 0f : 1f, useTween: false);
			((Behaviour)_rightsWidget.Widget).enabled = show;
		}
		else
		{
			TweenRotation.Begin(((Component)_openerSprite).gameObject, 0.2f, val);
			if (show)
			{
				((Component)_rightsWidget).gameObject.SetActive(true);
				((Behaviour)_rightsWidget.Widget).enabled = true;
				_rightsWidget.Alpha = 1f;
			}
			else
			{
				((Behaviour)_rightsWidget.Widget).enabled = false;
				_rightsWidget.Alpha = 0f;
			}
		}
		_layoutController.UpdateLayout();
		_separator.UpdateAnchors();
		if (!instant && this.ExtendViewStateChanged != null)
		{
			this.ExtendViewStateChanged(this);
		}
	}
}
