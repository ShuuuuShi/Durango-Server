using System;
using K1Network;
using Messages;

public class TicketSystem : GameSystem<TicketSystem>
{
	public Action TicketSalesUpdated;

	public TicketSales TicketSales { get; private set; }

	private void Awake()
	{
		Connections.Frontend.On<TicketSales>(OnTicketSales);
	}

	public void RequestTicketSales()
	{
		Connections.Frontend.Send(default(GetTicketSales));
	}

	public void Reticket(Action<bool> onResult = null)
	{
		Connections.Frontend.Send(default(RequestTickets)).On<OK>(delegate
		{
			RequestTicketSales();
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	private void OnTicketSales(TicketSales msg, PacketHeader header)
	{
		TicketSales = msg;
		if (TicketSalesUpdated != null)
		{
			TicketSalesUpdated();
		}
	}
}
