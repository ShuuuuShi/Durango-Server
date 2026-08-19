using System;
using System.Collections.Generic;
using L10N;
using UnityEngine;

public class RecipeInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KWidgetScrollView _scrollVIew;

	[SerializeField]
	private UIWidget _descriptionWidget;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget _timeWidget;

	[SerializeField]
	private UISpriteLabel _timeKeyLabel;

	[SerializeField]
	private UISpriteLabel _timeValueLabel;

	[SerializeField]
	private UIWidget _maxLevelWidget;

	[SerializeField]
	private UILabel _maxLevelLabel;

	[SerializeField]
	private UIWidget _conditionWidget;

	[SerializeField]
	private ListObjectPool _conditionItems;

	[SerializeField]
	private int _conditionMargin;

	[SerializeField]
	private UIWidget _sizeSelectorWidget;

	[SerializeField]
	private IntSelector _xSelector;

	[SerializeField]
	private IntSelector _ySelector;

	[SerializeField]
	private UIWidget _materialWidget;

	[SerializeField]
	private ListObjectPool _materialItemWidgets;

	[SerializeField]
	private UIWidget _buttonWidget;

	[SerializeField]
	private DefaultSelectableButton _nextButton;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	private int _descriptionPadding;

	private int _conditionsPadding;

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

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Point2 Size => new Point2(_xSelector.Value, _ySelector.Value);

	public event Action Confirmed;

	public event Action<int, int> BuildSizeChanged;

	public void Init()
	{
		_descriptionPadding = _descriptionWidget.height - _descriptionLabel.height;
		_conditionsPadding = _conditionWidget.height - _conditionItems.BaseObject.GetComponent<UIWidget>().height;
		DefaultSelectableButton nextButton = _nextButton;
		nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, (Action)delegate
		{
			if (this.Confirmed != null)
			{
				this.Confirmed();
			}
		});
		IntSelector xSelector = _xSelector;
		xSelector.ValueChanged = (Action)Delegate.Combine(xSelector.ValueChanged, new Action(OnBuildSizeChange));
		IntSelector ySelector = _ySelector;
		ySelector.ValueChanged = (Action)Delegate.Combine(ySelector.ValueChanged, new Action(OnBuildSizeChange));
		AnimWidget.SetAlpha(0f, useTween: false);
		_scrollVIew.Reposition(resetPosition: true);
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		AnimWidget.Alpha = 1f;
	}

	public void Hide()
	{
		AnimWidget.Alpha = 0f;
	}

	public void SetTitle(string title)
	{
		_titleLabel.text = title;
	}

	public void SetDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			((Component)_descriptionWidget).gameObject.SetActive(false);
			return;
		}
		((Component)_descriptionWidget).gameObject.SetActive(true);
		_descriptionLabel.text = description;
		_descriptionWidget.height = _descriptionLabel.height + _descriptionPadding;
	}

	public void SetRemainTime(float time, string keyText)
	{
		if (time <= 0f)
		{
			((Component)_timeWidget).gameObject.SetActive(false);
			return;
		}
		((Component)_timeWidget).gameObject.SetActive(true);
		_timeKeyLabel.text = keyText;
		_timeValueLabel.text = TimerSystem.TimeToString(time);
	}

	public void SetMaxLevel(int level)
	{
		if (level > 0)
		{
			((Component)_maxLevelWidget).gameObject.SetActive(true);
			_maxLevelLabel.text = LocalizeSystem.Format("#recipe_max_level_format", level.ToString());
		}
		else
		{
			((Component)_maxLevelWidget).gameObject.SetActive(false);
		}
	}

	public void SetConditions(IList<Tuple<string, string, string>> list)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		if (list == null || list.Count == 0)
		{
			((Component)_conditionWidget).gameObject.SetActive(false);
			return;
		}
		((Component)_conditionWidget).gameObject.SetActive(true);
		_conditionItems.Set(list.Count);
		Vector3 localPosition = _conditionItems.BaseObject.transform.localPosition;
		int num = 0;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			SimpleContainer component = _conditionItems[i].GetComponent<SimpleContainer>();
			UILabel uILabel = component.Get<UILabel>("Key");
			UISpriteLabel uISpriteLabel = component.Get<UISpriteLabel>("Value");
			UIWidget uIWidget = component.Get<UIWidget>("ValueBg");
			UISprite uISprite = component.Get<UISprite>("Icon");
			uILabel.text = list[i].Item1;
			uISpriteLabel.text = list[i].Item2;
			Vector2 printedSize = uISpriteLabel.Label.printedSize;
			uIWidget.width = (int)(printedSize.x + Mathf.Abs(((Component)uISpriteLabel).transform.localPosition.x - ((Component)uIWidget).transform.localPosition.x) * 2f);
			uIWidget.height = (int)(printedSize.y + Mathf.Abs(((Component)uISpriteLabel).transform.localPosition.y - ((Component)uIWidget).transform.localPosition.y) * 2f);
			if (string.IsNullOrEmpty(list[i].Item3))
			{
				((Component)uISprite).gameObject.SetActive(false);
			}
			else
			{
				((Component)uISprite).gameObject.SetActive(true);
				uISprite.spriteName = list[i].Item3;
				UIUtility.ResizeToSquare(uISprite, uISpriteLabel.Label.fontSize + 4);
				((Component)uISprite).transform.localPosition = ((Component)uISpriteLabel).transform.localPosition + Vector3.left * (float)(uIWidget.width + 3);
			}
			int num2 = (int)Mathf.Max(uILabel.printedSize.y, printedSize.y);
			component.Get<UIWidget>((string)null).height = num2;
			((Component)component).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += num2;
			if (i < count - 1)
			{
				num += _conditionMargin;
			}
		}
		_conditionWidget.height = num + _conditionsPadding;
	}

	public void SetResizable(int xMax, int yMax)
	{
		if (xMax <= 1 && yMax <= 1)
		{
			SetNonResiable();
			return;
		}
		((Component)_sizeSelectorWidget).gameObject.SetActive(true);
		_xSelector.Set(_xSelector.Value, 1, xMax);
		_ySelector.Set(_ySelector.Value, 1, yMax);
	}

	public void SetNonResiable()
	{
		((Component)_sizeSelectorWidget).gameObject.SetActive(false);
	}

	public void SetMaterials(IList<Tuple<string, int, int>> list, Tuple<string, int, int> toolInfo)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		int num = list.Count + ((toolInfo != null) ? 1 : 0);
		((Component)_materialWidget).gameObject.SetActive(num > 0);
		if (num > 0)
		{
			_materialItemWidgets.Set((toolInfo == null) ? 1 : 2);
			ReceipeMaterialInfoWidget component = _materialItemWidgets[0].GetComponent<ReceipeMaterialInfoWidget>();
			component.Set(T._("재질"), list);
			if (toolInfo != null)
			{
				ReceipeMaterialInfoWidget component2 = _materialItemWidgets[1].GetComponent<ReceipeMaterialInfoWidget>();
				component2.Set(T._("도구"), new Tuple<string, int, int>[1] { toolInfo });
			}
			int num2 = 0;
			int i = 0;
			for (int count = _materialItemWidgets.Count; i < count; i++)
			{
				num2 += _materialItemWidgets[i].GetComponent<UIWidget>().height;
			}
			_materialWidget.height = num2;
			UIUtility.WidgetsReposition(_materialItemWidgets.Get, _materialItemWidgets.Count, Vector3.down, _materialItemWidgets.BaseObject.transform.localPosition);
		}
	}

	private void OnBuildSizeChange()
	{
		if (this.BuildSizeChanged != null)
		{
			this.BuildSizeChanged(_xSelector.Value, _ySelector.Value);
		}
	}

	public Transform GetButtonTransform()
	{
		return ((Component)_buttonWidget).transform;
	}

	public void SetNextButton(string text, bool enable = true)
	{
		_nextButton.Text = text;
		_nextButton.Disable = !enable;
	}

	public void UpdateLayout(bool reset)
	{
		UIUtility.UpdateAnchors(((Component)this).transform);
		_scrollVIew.Reposition(reset);
	}
}
