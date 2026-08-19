using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class StatusEffectGroupPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private KScrollView _scrollList;

	[SerializeField]
	private RectLayout _layout;

	private readonly List<StatusEffect> _statusEffects = new List<StatusEffect>();

	protected override void OnAwake()
	{
		base.OnAwake();
		_titleLabel.text = T._("적용중인 효과");
	}

	public void Set(IEnumerable<StatusEffect> list)
	{
		_statusEffects.AddRange(list);
	}

	protected override void FillData()
	{
		_countLabel.text = _statusEffects.Count.ToString();
		ListObjectPool nodes = _scrollList.Nodes;
		nodes.BeginLoad();
		foreach (StatusEffect statusEffect in _statusEffects)
		{
			StatusEffectGroupItemWidget component = nodes.GetNext().GetComponent<StatusEffectGroupItemWidget>();
			component.Set(statusEffect);
		}
		nodes.EndLoad();
		_scrollList.ResetPosition();
	}

	protected override void UpdateLayout()
	{
		base.transform.localPosition = Vector3.zero;
		int safeHeight = UIManager.SafeHeight;
		_layout.UpdateLayout(null, Mathf.Min((int)((float)safeHeight * 0.8f), safeHeight - 160));
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_statusEffects.Clear();
	}
}
