using System;
using Durango.UI.Control;
using L10N;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class AccessRightsPage : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private BinaryToggleSlider _toggleButton;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private UIWidget _emptyWidget;

	private bool _isWritable;

	private bool _isInit;

	public OwnerType Owner { get; private set; }

	public AccessRights Rights { get; private set; }

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_scrollView.Nodes.Init(delegate(GameObject o)
		{
			AccessRightNode component2 = o.GetComponent<AccessRightNode>();
			component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickRightNode));
		});
		BinaryToggleSlider toggleButton = _toggleButton;
		toggleButton.ValueRatioChanged = (Action<float>)Delegate.Combine(toggleButton.ValueRatioChanged, new Action<float>(OnToggleButtonRatioChange));
		BinaryToggleSlider toggleButton2 = _toggleButton;
		toggleButton2.ValueChanged = (Action<bool>)Delegate.Combine(toggleButton2.ValueChanged, new Action<bool>(OnToggleButtonChange));
		OnToggleButtonRatioChange(_toggleButton.Ratio);
		_scrollView.Nodes.BeginLoad();
		Array values = Enum.GetValues(typeof(AccessRights));
		for (int i = 0; i < values.Length; i++)
		{
			AccessRights accessRights = (AccessRights)values.GetValue(i);
			if (accessRights != 0)
			{
				AccessRightNode component = _scrollView.Nodes.GetNext().GetComponent<AccessRightNode>();
				component.Set(accessRights);
			}
		}
		_scrollView.Nodes.EndLoad();
		_scrollView.ResetPosition();
	}

	private void OnClickRightNode()
	{
		if (!_isWritable)
		{
			UIManager.SystemMsg(T._("최고 관리자 등급은 권한을 수정할 수 없습니다."));
			return;
		}
		AccessRightNode accessRightNode = Selectable.Current as AccessRightNode;
		if (!(accessRightNode == null))
		{
			AccessRights right = accessRightNode.Right;
			if ((Rights & right) == 0)
			{
				Rights |= right;
				accessRightNode.Selected = true;
			}
			else
			{
				Rights &= ~right;
				accessRightNode.Selected = false;
			}
			if (Rights == AccessRights.None)
			{
				_toggleButton.Set(0f, sendEvent: false, playAnimation: true);
			}
		}
	}

	public void Set(string nameText, OwnerType owner, AccessRights rights, bool writable)
	{
		Init();
		Owner = owner;
		Rights = rights;
		RefreshRightsNodes();
		_titleLabel.text = T._("{0} 권한 설정", nameText);
		_isWritable = writable;
		_toggleButton.Set((rights == AccessRights.None) ? 0f : 1f);
		_toggleButton.SetDisabled(!writable);
		_scrollView.ResetPosition();
	}

	private void RefreshRightsNodes()
	{
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			AccessRightNode component = _scrollView.Nodes[i].GetComponent<AccessRightNode>();
			component.Set(Owner);
			component.Selected = (component.Right & Rights) != 0;
		}
	}

	private void OnToggleButtonChange(bool on)
	{
		Rights = (on ? EstateSystem.RightsOnPreset : AccessRights.None);
		RefreshRightsNodes();
		_scrollView.ResetPosition();
	}

	private void OnToggleButtonRatioChange(float ratio)
	{
		float num = ((!_isWritable) ? 0.5f : 1f);
		if (ratio > 0f && ratio < 1f)
		{
			_emptyWidget.gameObject.SetActive(value: true);
			_scrollView.gameObject.SetActive(value: true);
			_emptyWidget.alpha = 1f - ratio;
			_scrollView.Panel.alpha = Mathf.Lerp(0f, num, ratio);
		}
		else if (ratio <= 0f)
		{
			_emptyWidget.alpha = 1f;
			_scrollView.Panel.alpha = 0f;
			_emptyWidget.gameObject.SetActive(value: true);
			_scrollView.gameObject.SetActive(value: false);
		}
		else
		{
			_scrollView.Panel.alpha = num;
			_emptyWidget.alpha = 0f;
			_emptyWidget.gameObject.SetActive(value: false);
			_scrollView.gameObject.SetActive(value: true);
		}
	}
}
