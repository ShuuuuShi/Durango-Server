using System.Collections.Generic;
using Messages;
using UnityEngine;

public class TicketTierItem : MonoBehaviour
{
	[SerializeField]
	private UILabel _roundLabel;

	[SerializeField]
	private UISprite _roundColor;

	[SerializeField]
	private ListObjectPool _tickets;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public void Set(int round, IList<Messages.Ticket> tickets, bool enableRoundColor)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		_tickets.Clear();
		_roundLabel.text = (round + 1).ToString();
		if (enableRoundColor)
		{
			Color uIYellow = PresetColor.UIYellow;
			_roundLabel.color = uIYellow;
			_roundColor.color = uIYellow;
		}
		else
		{
			_roundLabel.color = Color.white;
			_roundColor.alpha = 0f;
		}
		int i = 0;
		for (int size = KUtility.GetSize(tickets); i < size; i++)
		{
			Messages.Ticket ticket = tickets[i];
			if (ticket.Round == round)
			{
				TicketListItem ticketListItem = ((ListObjectPoolBase<GameObject>)_tickets).Add<TicketListItem>();
				ticketListItem.Set(ticket);
				ticketListItem.SetActiveSplitLine(i < size - 1);
			}
		}
		float num = _tickets.Reposition(Vector3.down);
		Widget.height = Mathf.CeilToInt(num);
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
