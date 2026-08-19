using System.Collections.Generic;
using Messages;
using UnityEngine;

public class TicketListWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _ticketList;

	public void Set(IList<Messages.Ticket> tickets)
	{
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(tickets); i < size; i++)
		{
			num = Mathf.Max(num, tickets[i].Round);
		}
		ListObjectPool nodes = _ticketList.Nodes;
		nodes.Clear();
		for (int num2 = num; num2 >= 0; num2--)
		{
			TicketTierItem ticketTierItem = ((ListObjectPoolBase<GameObject>)nodes).Add<TicketTierItem>();
			ticketTierItem.Set(num2, tickets, num2 == num);
		}
		_ticketList.ResetPosition();
	}
}
