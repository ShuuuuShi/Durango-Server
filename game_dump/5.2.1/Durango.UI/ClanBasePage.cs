using Durango.UI.Popup;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ClanBasePage : MonoBehaviour
{
	[SerializeField]
	private UIWidget _emptyPage;

	[SerializeField]
	private ClanBaseWidget _mainPage;

	[SerializeField]
	private ClanBaseLostWidget _lostPage;

	private EstateLicenses _estateLicenses;

	public void Show(EstateLicenses estateLicenses, bool moveToMain)
	{
		_estateLicenses = estateLicenses;
		if (moveToMain)
		{
			ShowMainPage();
		}
		Refresh(estateLicenses);
		base.gameObject.SetActive(value: true);
	}

	private void Refresh(EstateLicenses estateLicenses)
	{
		_estateLicenses = estateLicenses;
		ClanCargoWarphole? clanCargoWarphole = _estateLicenses.ClanCargoWarphole;
		if (clanCargoWarphole.HasValue)
		{
			_mainPage.Set(estateLicenses);
			_lostPage.Set(estateLicenses);
		}
	}

	private void ShowMainPage()
	{
		int num = 0;
		if (_estateLicenses.ClanCargoWarphole.HasValue && _estateLicenses.ClanCargoWarphole.Value.Location.HasValue)
		{
			ClanCargoWarphole value = _estateLicenses.ClanCargoWarphole.Value;
			num = (value.BattleInfo.HasValue ? 1 : ((value.OccupiedAt.HasValue && value.OccupiedUntil.HasValue) ? 2 : 0));
		}
		_emptyPage.gameObject.SetActive(num == 0);
		_mainPage.gameObject.SetActive(num == 1);
		_lostPage.gameObject.SetActive(num == 2);
	}

	public static void ShowHelpTitle(GameObject parent, string text)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, text, 400);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		Transform transform = parent.transform.Find("Sprite");
		UISprite uISprite = ((!(transform == null)) ? transform.GetComponent<UISprite>() : null);
		if (uISprite == null)
		{
			widgetTooltipControl.Show(parent, Vector2.zero, 30f);
		}
		else
		{
			widgetTooltipControl.Show(uISprite, Vector2.up * 5f, 30f);
		}
	}
}
