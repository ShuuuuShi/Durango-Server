using System;
using UnityEngine;

public class TagFilterCard : MonoBehaviour
{
	private const int Margin = 10;

	public Action<GameObject> Removed;

	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UIWidget _removeBtn;

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

	private void Start()
	{
		if ((Object)(object)_removeBtn != (Object)null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_removeBtn).gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickRemoveButton));
		}
	}

	private void OnClickRemoveButton(GameObject go)
	{
		if (Removed != null)
		{
			Removed(((Component)this).gameObject);
		}
	}

	public void Set(string text)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = text;
		int num = (int)(_nameLabel.Label.GetPosition(0f, 0f).x - Widget.localCorners[0].x);
		int width = num + _nameLabel.Label.width + ((!((Object)(object)_removeBtn == (Object)null)) ? (10 + _removeBtn.width) : num);
		Widget.width = width;
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
