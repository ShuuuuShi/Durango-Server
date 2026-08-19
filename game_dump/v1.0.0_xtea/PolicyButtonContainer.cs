using System;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PolicyButtonContainer : MonoBehaviour
{
	[SerializeField]
	private UILabel _textPolicyName;

	[SerializeField]
	private UILabel _textPolicyLevel;

	[SerializeField]
	private UILabel _textDescription;

	[SerializeField]
	private GameObject _arrowUp;

	[SerializeField]
	private GameObject _arrowDown;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _policyButtons;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action<PolicyButton> PolicyClicked;

	private void OnEnable()
	{
		UpdateScrollViewAndArrows();
	}

	public void Init()
	{
		_policyButtons.Init(delegate(GameObject obj)
		{
			UIEventListener.Get(obj).onClick = OnClick_PolicyButton;
		});
		TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
		component.SetOnFinished(delegate
		{
			if (Math.Abs(Widget.alpha) < float.Epsilon)
			{
				((Component)this).gameObject.SetActive(false);
			}
		});
		RefreshDescription(null);
	}

	public void RefreshButtons(CombatSystem.CombatPolicyInfo[] policies)
	{
		_policyButtons.Clear();
		if (policies != null)
		{
			for (int i = 0; i < policies.Length; i++)
			{
				string id = policies[i].Id;
				ActionPolicy actionPolicy = SingletonDict<string, ActionPolicy>.Get(id);
				PolicyButton policyButton = ((ListObjectPoolBase<GameObject>)_policyButtons).Add<PolicyButton>();
				policyButton.Set(id, policies[i].Level, (actionPolicy != null) ? actionPolicy.name.ToString() : string.Empty, (actionPolicy != null) ? actionPolicy.descrption.ToString() : string.Empty, (actionPolicy != null) ? actionPolicy.icon : string.Empty);
			}
		}
		UpdateLayout();
	}

	public PolicyButton FindPolicyButton(string id)
	{
		int i = 0;
		for (int count = _policyButtons.Count; i < count; i++)
		{
			PolicyButton component = _policyButtons[i].GetComponent<PolicyButton>();
			if (component.PolicyId == id)
			{
				return component;
			}
		}
		return null;
	}

	public void SelectPolicyById(string id)
	{
		int i = 0;
		for (int count = _policyButtons.Count; i < count; i++)
		{
			PolicyButton component = _policyButtons[i].GetComponent<PolicyButton>();
			if (component.PolicyId == id)
			{
				component.IsSelected = true;
				RefreshDescription(component);
			}
			else
			{
				component.IsSelected = false;
			}
		}
	}

	public void Show()
	{
		if (!((Component)this).gameObject.activeSelf)
		{
			TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
			if (_policyButtons.Count > 0)
			{
				((Component)this).gameObject.SetActive(true);
				component.tweenFactor = 0f;
				component.PlayForward();
			}
			else
			{
				component.PlayReverse();
			}
		}
	}

	public void Hide()
	{
		TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
		component.PlayReverse();
	}

	private void OnClick_PolicyButton(GameObject obj)
	{
		PolicyButton component = obj.GetComponent<PolicyButton>();
		if ((Object)(object)component != (Object)null)
		{
			if (this.PolicyClicked != null)
			{
				this.PolicyClicked(component);
			}
			RefreshDescription(component);
		}
	}

	private void RefreshDescription(PolicyButton button)
	{
		if ((Object)(object)button != (Object)null)
		{
			_textPolicyName.text = button.PolicyName;
			_textDescription.text = button.Description;
			_textPolicyLevel.text = T.Format("[FFFFFF]{0:lv:}[-]", button.PolicyLevel);
			_textPolicyLevel.UpdateAnchors();
		}
		else
		{
			_textPolicyName.text = string.Empty;
			_textDescription.text = string.Empty;
			_textPolicyLevel.text = string.Empty;
		}
	}

	private void UpdateLayout()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		PolicyButton component = _policyButtons.BaseObject.GetComponent<PolicyButton>();
		int count = _policyButtons.Count;
		int height = ((Component)component).GetComponent<UIWidget>().height;
		Vector3 localPosition = ((Component)component).transform.localPosition;
		for (int i = 0; i < count; i++)
		{
			PolicyButton component2 = _policyButtons[i].GetComponent<PolicyButton>();
			((Component)component2).transform.localPosition = localPosition;
			localPosition.y -= (float)height;
		}
		if (((Component)this).gameObject.activeSelf)
		{
			UpdateScrollViewAndArrows();
		}
	}

	private void UpdateScrollViewAndArrows()
	{
		_scrollView.ResetPosition();
		bool shouldMoveVertically = _scrollView.shouldMoveVertically;
		_arrowUp.SetActive(shouldMoveVertically);
		_arrowDown.SetActive(shouldMoveVertically);
	}
}
