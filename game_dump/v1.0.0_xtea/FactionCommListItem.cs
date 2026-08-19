using Messages;
using UnityEngine;

public class FactionCommListItem : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconWalkie;

	[SerializeField]
	private UILabel _textCommFirstLine;

	[SerializeField]
	private UILabel _textCommTime;

	[SerializeField]
	private UISprite _iconArrow;

	[SerializeField]
	private Color _colorNormal;

	[SerializeField]
	private Color _colorPressed;

	public int Index { get; private set; }

	public void SetRecord(int index, FactionRadioRecord record)
	{
		Index = index;
		_textCommFirstLine.text = ((record.Messages.Length <= 0) ? string.Empty : record.Messages[0]);
		_textCommTime.text = TimerSystem.Timeago(record.ReceivedAt);
	}

	private void OnPress(bool press)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((!press) ? _colorNormal : _colorPressed);
		_iconWalkie.color = color;
		_textCommFirstLine.color = color;
		_textCommTime.color = color;
		_iconArrow.color = color;
	}
}
