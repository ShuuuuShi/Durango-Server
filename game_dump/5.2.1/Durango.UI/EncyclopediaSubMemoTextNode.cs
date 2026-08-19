using Durango.Logic.Encyclopedia;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaSubMemoTextNode : MonoBehaviour
{
	private const int InvisibleMemoFontSize = 4;

	[SerializeField]
	private UILabel _textLabel;

	private UIWidget _widget;

	private int _defaultFontSize;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_widget = GetComponent<UIWidget>();
			_defaultFontSize = _textLabel.fontSize;
		}
	}

	public void Set(MemoType memoType, int memoId, float number, bool available)
	{
		Init();
		string memoText = MemoSystem.GetMemoText(memoType, memoId);
		string text = $"#{number}\n{memoText}";
		Vector3 localPosition = _widget.localCorners[1] + new Vector3(30f, -20f);
		_textLabel.transform.localPosition = localPosition;
		_textLabel.transform.localScale = Vector3.one;
		_textLabel.fontSize = _defaultFontSize;
		_textLabel.width = _widget.width - 60;
		_textLabel.text = text;
		Vector2 vector = _textLabel.localSize;
		if (!available)
		{
			_textLabel.fontSize = 4;
			float num = (float)_defaultFontSize / 4f;
			_textLabel.width = (int)(vector.x / num);
			_textLabel.transform.localScale = Vector3.one * num;
			vector = _textLabel.localSize * num;
		}
		_widget.height = (int)vector.y + 40;
	}
}
