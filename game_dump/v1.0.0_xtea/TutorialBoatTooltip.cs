using System.Collections.Generic;
using UnityEngine;

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

	protected override void OnUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (base.State < VisibleState.FadeOut)
		{
			if ((Object)(object)_target == (Object)null)
			{
				Hide();
				return;
			}
			Vector3 localPosition = MainCamera.WorldToNGUIPos(_target.Center + _offset);
			localPosition.y += (float)base.Widget.height;
			((Component)this).transform.localPosition = localPosition;
		}
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_comments.Set((_commentList != null) ? _commentList.Count : 0);
		int i = 0;
		for (int count = _comments.Count; i < count; i++)
		{
			KeyValueLabel component = _comments[i].GetComponent<KeyValueLabel>();
			component.Set(_commentList[i].Key, _commentList[i].Value);
		}
	}

	protected override void UpdateLayout()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		int num = _minWidth;
		int i = 0;
		for (int count = _comments.Count; i < count; i++)
		{
			KeyValueLabel component = _comments[i].GetComponent<KeyValueLabel>();
			num = Mathf.Max(num, (int)component.GetPredictSize().x);
		}
		int num2 = _titleWidget.height;
		Vector3 localPosition = _comments.BaseObject.transform.localPosition;
		int j = 0;
		for (int count2 = _comments.Count; j < count2; j++)
		{
			KeyValueLabel component2 = _comments[j].GetComponent<KeyValueLabel>();
			component2.UpdateLayout(num);
			((Component)component2).transform.localPosition = localPosition;
			localPosition.y -= (float)component2.Widget.height;
			num2 += component2.Widget.height;
		}
		base.Widget.width = num;
		base.Widget.height = num2;
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
