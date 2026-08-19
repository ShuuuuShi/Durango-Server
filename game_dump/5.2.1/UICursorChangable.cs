using Durango.UI;
using UnityEngine;

public class UICursorChangable : MonoBehaviour, IUICursorChangable
{
	[SerializeField]
	private bool _isCursorChangable;

	[SerializeField]
	private bool _isCursorSpecified;

	[SerializeField]
	private GameCursorType _specifiedCursorType;

	bool IUICursorChangable.IsCursorChangable()
	{
		return _isCursorChangable;
	}

	bool IUICursorChangable.IsCursorSpecified(ref GameCursorType cursorType)
	{
		cursorType = _specifiedCursorType;
		return _isCursorSpecified;
	}
}
