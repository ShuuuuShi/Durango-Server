using UnityEngine;

public class LineTooltipItem : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _keyLabel;

	[SerializeField]
	private UISpriteLabel _valueLabel;

	[SerializeField]
	private GameObject _splitLine;

	private UIWidget _widget;

	public UISpriteLabel KeyLabel => _keyLabel;

	public UISpriteLabel ValueLabel => _valueLabel;

	public string Key
	{
		get
		{
			return _keyLabel.text;
		}
		set
		{
			_keyLabel.text = value;
		}
	}

	public string Value
	{
		get
		{
			return _valueLabel.text;
		}
		set
		{
			_valueLabel.text = value;
		}
	}

	public int Index { get; set; }

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

	public void LineActive(bool active)
	{
		_splitLine.gameObject.SetActive(active);
	}

	public void UpdateLayout(float padding)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		int width = Widget.width;
		Vector3 localPosition = ((Component)KeyLabel).transform.localPosition;
		Vector3 localPosition2 = ((Component)ValueLabel).transform.localPosition;
		localPosition.x = ((float)width - padding) * -0.5f;
		localPosition2.x = ((float)width - padding) * 0.5f;
		((Component)KeyLabel).transform.localPosition = localPosition;
		((Component)ValueLabel).transform.localPosition = localPosition2;
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
	}
}
