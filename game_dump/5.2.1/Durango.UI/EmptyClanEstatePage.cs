using System;
using Durango.Logic.Clan;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EmptyClanEstatePage : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private SelectableButton _contextButton;

	public Transform DeclareButton => _contextButton.transform;

	public event Action DeclareButtonClicked;

	public void Refresh()
	{
		_contextButton.gameObject.SetActive(value: true);
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			_commentLabel.text = T._("[size=30][ffffffcc][icon=icon_item_lock] 부족에 가입하면 이용 가능합니다![-][/size]<br>8</br>[size=22]<weak>부족에 가입하고 부족 영토와 거점을 이용하세요.</weak>[/size]");
			_contextButton.Text = T._("부족 찾기");
			_contextButton.Clicked = delegate
			{
				UIManager.FindScript<ClanGroup>().Open(ClanGroup.ClanMenus.ClanList);
			};
			_warningLabel.text = T._("[icon=icon_make_alert] {0} 이상 부족이 부족 영토를 가질 수 있습니다.", LocalizeUtil.FormatLevel(5));
		}
		else if (playerClan.Level < 5)
		{
			_commentLabel.text = T._("[size=30][ffffffcc][icon=icon_item_lock] 부족 {0} 이상부터 가능합니다.[-][/size]<br>8</br>[size=22]<weak>부족 레벨을 올리면 부족 영토 선언이 가능합니다.</weak>[/size]", LocalizeUtil.FormatLevel(5));
			_contextButton.Text = T._("부족 레벨 보기");
			_contextButton.Clicked = delegate
			{
				UIManager.FindScript<ClanGroup>().Open(ClanGroup.ClanMenus.Level);
			};
			_warningLabel.text = T._("[icon=icon_make_alert] 사유지는 한 곳에만 선언할 수 있습니다.");
		}
		else
		{
			if (PlayerBehavior.LocalPlayer.IsClanOwner)
			{
				_commentLabel.text = T._("[size=30][ffffffcc]부족 영토를 선언해 보세요.[-][/size]<br>8</br>[size=22]<weak>마음에 드는 도시섬에서 부족 영토 선언 버튼을 누르면\n부족만의 영토가 생깁니다!</weak>[/size]");
				_contextButton.Text = T._("부족 영토 선언");
				_contextButton.Clicked = delegate
				{
					if (this.DeclareButtonClicked != null)
					{
						this.DeclareButtonClicked();
					}
				};
			}
			else
			{
				_commentLabel.text = T._("[size=30][ffffffcc]부족 영토가 없습니다.[-][/size]<br>8</br>[size=22]<weak>아직 부족 영토가 없습니다. 부족 관리자에게 문의 하세요</weak>[/size]");
				_contextButton.Text = T._("부족 관리자 보기");
				_contextButton.Clicked = delegate
				{
					UIManager.FindScript<ClanGroup>().Open(ClanGroup.ClanMenus.Members);
				};
			}
			_warningLabel.text = T._("[icon=icon_make_alert] 부족 관리자가 부족 영토를 선언할 수 있습니다.");
		}
		_contextButton.SetStyle((!PlayerBehavior.LocalPlayer.IsClanOwner) ? PresetButton.Style.Border : PresetButton.Style.Solid);
		UIUtility.WidgetsReposition(new UIWidget[3] { _iconSprite, _commentLabel, _contextButton.Widget }, new Vector3(0f, -1f), Vector3.zero, 18f, 0.5f);
	}
}
