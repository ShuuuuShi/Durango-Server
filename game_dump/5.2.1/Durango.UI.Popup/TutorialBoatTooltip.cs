using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class TutorialBoatTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private ListObjectPool _comments;

	[SerializeField]
	private int _minWidth = 300;

	[SerializeField]
	private Vector3 _offset;

	private string _title;

	private IList<KeyValuePair<string, string>> _commentList;

	private Artifact _target;

	public void Set(Artifact target, string title, IList<KeyValuePair<string, string>> list)
	{
		_target = target;
		_title = title;
		_commentList = list;
	}

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
	}

	protected override void OnUpdate()
	{
		if (base.State < VisibleState.Hide)
		{
			if (_target == null)
			{
				Hide();
				return;
			}
			Vector3 localPosition = MainCamera.WorldToNGUIPos(_target.Center + _offset);
			localPosition.y += base.Widget.height;
			base.transform.localPosition = localPosition;
		}
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_comments.Set((_commentList != null) ? _commentList.Count : 0);
		int i = 0;
		for (int count = _comments.Count; i < count; i++)
		{
			_comments[i].GetComponent<KeyValueLabel>().Set(_commentList[i].Key, _commentList[i].Value);
		}
	}

	protected override void UpdateLayout()
	{
		int num = _minWidth;
		int i = 0;
		for (int count = _comments.Count; i < count; i++)
		{
			KeyValueLabel component = _comments[i].GetComponent<KeyValueLabel>();
			num = Mathf.Max(num, (int)component.GetPreferredSize().x);
		}
		int num2 = _titleWidget.height;
		Vector3 localPosition = _comments.BaseObject.transform.localPosition;
		int j = 0;
		for (int count2 = _comments.Count; j < count2; j++)
		{
			KeyValueLabel component2 = _comments[j].GetComponent<KeyValueLabel>();
			component2.UpdateLayout(num);
			component2.transform.localPosition = localPosition;
			localPosition.y -= component2.Widget.height;
			num2 += component2.Widget.height;
		}
		base.Widget.width = num;
		base.Widget.height = num2;
		UIUtility.UpdateAnchors(base.transform);
	}
}
