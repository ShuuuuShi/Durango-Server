using Durango.Logic.Clan;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EmptyBaseEstatePage : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private SelectableButton _contextButton;

	public void Refresh()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null || playerClan.Level < 5)
		{
			_commentLabel.text = T._("[size=30][ffffffcc][icon=icon_item_lock] 부족 {0} 이상부터 가능합니다.[-][/size]<br>8</br>[size=22]<weak>부족 레벨을 올리면 거점 점령이 가능합니다.</weak>[/size]", LocalizeUtil.FormatLevel(5));
			_contextButton.gameObject.SetActive(value: true);
			_contextButton.Text = T._("부족 레벨 보기");
			_contextButton.Clicked = delegate
			{
				UIManager.FindScript<ClanGroup>().Open(ClanGroup.ClanMenus.Level);
			};
			_warningLabel.text = T._("[icon=icon_make_alert] 거점은 {0} 무법섬에 있습니다.", LocalizeUtil.FormatLevel(60));
		}
		else
		{
			_commentLabel.text = T._("[size=30][ffffffcc]부족 영토가 없습니다.[-][/size]<br>8</br>[size=22]<weak>아직 부족 영토가 없습니다. 부족 관리자에게 문의 하세요</weak>[/size]");
			_contextButton.gameObject.SetActive(value: false);
			_warningLabel.text = T._("[icon=icon_make_alert] 부족 관리자가 부족 영토를 선언할 수 있습니다.");
		}
		UIUtility.WidgetsReposition(new UIWidget[3] { _iconSprite, _commentLabel, _contextButton.Widget }, new Vector3(0f, -1f), Vector3.zero, 18f, 0.5f);
	}
}
