using System;
using L10N;
using Messages;
using Ticket;
using UnityEngine;

public class TicketGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _title;

	[SerializeField]
	private TicketInfoWidget _ticketInfo;

	[SerializeField]
	private TicketListWidget _ticketList;

	[SerializeField]
	private TierMeta[] _tierMetaList;

	public static TierMeta[] TierMetas { get; private set; }

	private void Awake()
	{
		TierMetas = _tierMetaList;
		base.OnClose();
	}

	private void Start()
	{
		TicketSystem ticketSystem = GameSystem<TicketSystem>.Instance();
		ticketSystem.TicketSalesUpdated = (Action)Delegate.Combine(ticketSystem.TicketSalesUpdated, new Action(OnUpdateTicketSales));
		_title.OnBack += Close;
		_title.OnClose += base.ForceClose;
		_ticketInfo.Reticketed += OnReticket;
		base.OnOpenSucceed += OnOpenSuccess;
	}

	private void OnReticket()
	{
		GameSystem<TicketSystem>.Instance().Reticket(delegate(bool success)
		{
			if (success)
			{
				UIManager.SystemMsg(T._("추가 베타키가 발급되었습니다"));
			}
		});
	}

	private void OnUpdateTicketSales()
	{
		if (base.IsOpen)
		{
			TicketSales ticketSales = GameSystem<TicketSystem>.Instance().TicketSales;
			Set(ticketSales);
		}
	}

	private void OnOpenSuccess()
	{
		OnUpdateTicketSales();
		GameSystem<TicketSystem>.Instance().RequestTicketSales();
	}

	private void Set(TicketSales sales)
	{
		_ticketInfo.Set(sales);
		_ticketList.Set(sales.Tickets);
	}

	public static TierMeta GetTierMeta(int tier)
	{
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(TierMetas); i < size; i++)
		{
			if (TierMetas[i].IsValidTier(tier))
			{
				num = i;
				break;
			}
		}
		return (num != -1) ? TierMetas[num] : default(TierMeta);
	}

	public static void SetTierIcon(UISprite sprite, int tier)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		TierMeta tierMeta = GetTierMeta(tier);
		tierMeta.Icon.Set(sprite);
		int num = tierMeta.MaxRank - tierMeta.MinRank;
		int num2 = ((num > 0) ? (tier - tierMeta.MinRank + 1) : 0);
		Transform transform = ((Component)sprite).transform;
		if (transform.childCount < num2)
		{
			for (int i = transform.childCount; i < num2; i++)
			{
				((Component)transform).gameObject.AddChild<UISprite>();
			}
		}
		else if (transform.childCount > num2)
		{
			for (int num3 = transform.childCount - 1; num3 >= num2; num3--)
			{
				Object.Destroy((Object)(object)((Component)transform.GetChild(num3)).gameObject);
			}
		}
		int num4 = sprite.width / 5;
		int num5 = num4 / 2;
		Vector3 localPosition = Vector3.Lerp(sprite.localCorners[1], sprite.localCorners[2], 0.5f);
		localPosition.y += (float)num4 * 0.5f;
		localPosition.x -= (float)((num4 + num5) * (num2 - 1)) * 0.5f;
		for (int j = 0; j < num2; j++)
		{
			UISprite component = ((Component)transform.GetChild(j)).GetComponent<UISprite>();
			component.width = num4;
			component.height = num4;
			((Component)component).transform.localPosition = localPosition;
			component.atlas = KSingleton<UIManager>.Instance().UIAtlas;
			component.spriteName = "bg_circle_small";
			component.color = tierMeta.Color;
			localPosition.x += (float)(num4 + num5);
		}
	}
}
