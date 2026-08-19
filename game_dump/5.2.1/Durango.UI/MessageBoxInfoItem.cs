using UnityEngine;

namespace Durango.UI;

public class MessageBoxInfoItem : MonoBehaviour
{
	[SerializeField]
	private UILabel _keyLabel;

	[SerializeField]
	private UISprite _separator;

	[SerializeField]
	private UILabel _valueLabel;

	public float KeyWidth { get; private set; }

	public float TotalWidth { get; private set; }

	public void Set(SyncString key, SyncString value)
	{
		_keyLabel.SetText(key);
		_valueLabel.SetText(value);
		float num = 0f;
		_keyLabel.SetPosition(num * Vector3.right, 0f, 0.5f);
		float num3 = (KeyWidth = num + _keyLabel.printedSize.x);
		num = num3;
		if (value.HasText())
		{
			_separator.gameObject.SetActive(value: true);
			num += 20f;
			_separator.SetPosition(num * Vector3.right, 0f, 0.5f);
			num += 30f;
			_valueLabel.SetPosition(num * Vector3.right, 0f, 0.5f);
			num += _valueLabel.printedSize.x;
		}
		else
		{
			_separator.gameObject.SetActive(value: false);
		}
		TotalWidth = num;
	}
}
