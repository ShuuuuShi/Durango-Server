using System;
using Durango.UI;
using Durango.UI.Control;
using L10N;
using Shared.Ability;
using UnityEngine;

[Uri("Status")]
public class CharacterStatusGroup : UIBase
{
	[Serializable]
	private struct AbilityLayout
	{
		[LocalizableString]
		public string Title;

		public Derived[] Abilities;
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private UIWidget _mainContainer;

	[SerializeField]
	private CharacterBasicStatusWidget _basicStatusWidget;

	[SerializeField]
	private CharacterDerivedStatusWidget _derivedStatusWidget;

	[SerializeField]
	private AbilityLayout[] _abilityLayouts;

	[SerializeField]
	private Vector2 _layoutMargin;

	private ListObjectPool<CharacterDerivedStatusWidget> _statusWidgets;

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("능력치"));
		GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += OnUpdateAbilities;
		TryClose();
	}

	private void OnUpdateAbilities()
	{
		if (base.IsOpened)
		{
			UpdateAbilities();
		}
	}

	public override bool Open()
	{
		bool result = base.Open();
		Init();
		UpdateAbilities();
		UpdateLayout();
		return result;
	}

	private void Init()
	{
		if (_statusWidgets == null)
		{
			_basicStatusWidget.Init();
			_statusWidgets = new ListObjectPool<CharacterDerivedStatusWidget>();
			_statusWidgets.BaseObject = _derivedStatusWidget;
			_statusWidgets.UseBase = true;
			_statusWidgets.Set(_abilityLayouts.Length);
			int i = 0;
			for (int count = _statusWidgets.Count; i < count; i++)
			{
				_statusWidgets[i].Init(_abilityLayouts[i].Title, _abilityLayouts[i].Abilities);
			}
		}
	}

	private void UpdateAbilities()
	{
		if (_statusWidgets != null)
		{
			_basicStatusWidget.Refresh();
			int i = 0;
			for (int count = _statusWidgets.Count; i < count; i++)
			{
				_statusWidgets[i].Refresh();
			}
		}
	}

	private void UpdateLayout()
	{
		if (_statusWidgets == null)
		{
			return;
		}
		_basicStatusWidget.UpdateLayout(_mainContainer.width, base.IsPortrait);
		int width = _mainContainer.width;
		int num = (base.IsPortrait ? 1 : 3);
		width -= (int)_layoutMargin.x * (num - 1);
		width /= num;
		int i = 0;
		for (int count = _statusWidgets.Count; i < count; i++)
		{
			_statusWidgets[i].UpdateLayout(width);
		}
		Vector3 zero = Vector3.zero;
		zero.y = -(_basicStatusWidget.height + 46);
		int num2 = 0;
		while (num2 < _statusWidgets.Count)
		{
			int num3 = 0;
			for (int j = 0; j < num; j++)
			{
				UIWidget widget = _statusWidgets[num2++].Widget;
				widget.SetPosition(zero, 0f, 1f);
				num3 = Mathf.Max(widget.height, num3);
				zero.x += (float)_mainContainer.width / (float)num;
				if (num2 >= _statusWidgets.Count)
				{
					break;
				}
			}
			zero.x = 0f;
			zero.y -= (float)num3 + _layoutMargin.y;
		}
	}
}
