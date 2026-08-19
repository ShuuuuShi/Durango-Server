using System;
using Durango.UI.Control;
using L10N;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class EmptyUrbanEstatePage : MonoBehaviour
{
	[SerializeField]
	private UISprite _sprite;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private SelectableButton _declareButton;

	public Transform DeclareButton => _declareButton.transform;

	public event Action DeclareButtonClicked;

	private void Start()
	{
		SelectableButton declareButton = _declareButton;
		declareButton.Clicked = (Action)Delegate.Combine(declareButton.Clicked, (Action)delegate
		{
			if (_declareButton.Disabled)
			{
				UIManager.SystemMsg(T._("도시섬 사유지는 도시섬에서만 선언할 수 있습니다."));
			}
			else if (this.DeclareButtonClicked != null)
			{
				this.DeclareButtonClicked();
			}
		});
		_declareButton.CanClickWhenDisabled = true;
	}

	public void Refresh()
	{
		if (GameSystem<StatisticsSystem>.Instance().Level < 30)
		{
			_commentLabel.text = T._("[size=30][ffffffcc][icon=icon_item_lock] {0} 부터 가능합니다[-][/size]<br>8</br>[size=22][ffffff64]도시섬에서 부족원이나 다른 개척자와 함께할 수 있습니다.[-][/size]", LocalizeUtil.FormatLevel(30));
			_declareButton.gameObject.SetActive(value: false);
		}
		else
		{
			_commentLabel.text = T._("[size=30][ffffffcc]이제 도시섬에 사유지 선언이 가능합니다![-][/size]<br>8</br>[size=22][ffffff64]도시섬에서 부족원이나 다른 개척자와 함께할 수 있습니다.[-][/size]");
			_declareButton.gameObject.SetActive(value: true);
			_declareButton.Disabled = GameManager.Region.Role() != Role.Urban;
		}
		UIUtility.WidgetsReposition(new UIWidget[3] { _sprite, _commentLabel, _declareButton.Widget }, new Vector3(0f, -1f), Vector3.zero, 18f, 0.5f);
	}
}
