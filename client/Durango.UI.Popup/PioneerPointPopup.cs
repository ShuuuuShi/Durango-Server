using System;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class PioneerPointPopup : TooltipBase
{
	[SerializeField]
	private UILabel _gradeLabel;

	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	private GameObject _amplifierOn;

	[SerializeField]
	private GameObject _amplifierOff;

	[SerializeField]
	private GameObject _amplifierOffCover;

	[SerializeField]
	private PresetButton _shopButton;

	[SerializeField]
	private UILabel _paidRemainTime;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		PresetButton shopButton = _shopButton;
		shopButton.Clicked = (Action)Delegate.Combine(shopButton.Clicked, (Action)delegate
		{
			Hide();
			UIManager.FindScript<ShopGroup>().Open("signal_amplifier_package", select: true);
		});
	}

	protected override void FillData()
	{
		PioneerGradeInfo info = GameSystem<EstateSystem>.Instance().PioneerGradeInfo;
		int grade = info.Grade;
		_gradeLabel.text = T._("<em>{0}등급</em> 개척 재료 전송 효율 <help>{1}</help>", grade, T._("comment='<li>각 구간마다 획득 가능한 개척포인트의 양이 다릅니다</li>\n<li>신호증폭기 구입시 추가 포인트 획득이 가능합니다</li>',width=0"));
		bool flag = info.IsPaid();
		_amplifierOn.SetActive(flag);
		_amplifierOff.SetActive(!flag);
		_amplifierOffCover.SetActive(!flag);
		double? paymentEndsAt = info.PaymentEndsAt;
		if (paymentEndsAt.HasValue)
		{
			_paidRemainTime.SetText(new SyncString(delegate(out string text, out float period)
			{
				SyncString.UpdateRemainTimeMsg(info.PaymentEndsAt.Value, T._("{0} 남음"), out text, out period, string.Empty);
			}));
		}
		else
		{
			_paidRemainTime.text = string.Empty;
		}
		PioneerCostExchangeRate pioneerCostExchangeRate = Singleton<Pioneer>.Instance.GetPioneerCostExchangeRate(grade);
		if (pioneerCostExchangeRate != null)
		{
			_nodes.BeginLoad();
			for (int i = 0; i < KUtility.GetSize(pioneerCostExchangeRate.Rates); i++)
			{
				PioneerRate pioneerRate = pioneerCostExchangeRate.Rates[i];
				GameObject next = _nodes.GetNext();
				UILabel uILabel = next.FindComponent<UILabel>("Section");
				uILabel.text = ((!pioneerRate.Paid) ? T._("{0:P0} 구간", pioneerRate.Rate) : T._("<em>[icon=icon_amplifier]</em> {0:P0} 구간", pioneerRate.Rate));
				float num = info.DailyExchangedPoints.Get(pioneerRate.Rate, 0f);
				UILabel uILabel2 = next.FindComponent<UILabel>("Points");
				uILabel2.text = string.Format("{0:0.#} / {1}", num, (pioneerRate.Point < 0) ? "∞" : pioneerRate.Point.ToString());
				float fillAmount = ((pioneerRate.Point <= 0) ? 0f : (num / (float)pioneerRate.Point));
				UISprite uISprite = next.FindComponent<UISprite>("Bar");
				uISprite.fillAmount = fillAmount;
			}
			_nodes.EndLoad();
			base.Widget.height = ((_nodes.Count > 4) ? 638 : 560);
			base.Widget.UpdateAnchors();
			UIUtility.WidgetsReposition(_nodes, new Vector3(0f, -1f), new Vector3(0f, -108f), 30f);
		}
	}
}
