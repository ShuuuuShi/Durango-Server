using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class PvpIslandGuidePopup : TooltipBase
{
	[SerializeField]
	private KWidgetScrollView _itemPool;

	protected override void FillData()
	{
		string[] array = new string[4]
		{
			T._("난투섬"),
			T._("난투섬의 날씨"),
			T._("폭풍우 대피소"),
			T._("저체온증")
		};
		string[] array2 = new string[4]
		{
			T._("플레이어끼리의 전투가 가능한 섬입니다.\n위원회의 스파이로 의심되는 다른 개척자들을 처치하고 최후의 1인이 될 때까지 생존하세요."),
			T._("난투섬의 날씨는 위원회의 실험으로 인해 불안정한 상태입니다.\n날씨는 시간의 흐름에 따라 안개, 비, 폭풍우의 순서로 변화합니다.\n\n<em>안개</em>\n전투모드에 진입할 수 없습니다. 장비를 정비하고 음식을 섭취하세요.\n\n<em>비</em>\n전투 모드로 변경되며 사망할 때까지 전투 모드를 해제할 수 없습니다.\n아이템 확인 및 사용 또한 불가능합니다.\n\n<em>폭풍우</em>\n지속적으로 건강이 감소합니다."),
			T._("난투섬에 지도에 표시되어 있는 유일한 건물로 내부로 진입 시 폭풍우 효과를 막을 수 있습니다."),
			T._("폭풍우 이후로 더 오랜 시간이 경과하면 저체온증 상태가 됩니다.\n저체온증은 건강을 감소시키고, 대피소를 통해 막을 수 없습니다.")
		};
		for (int i = 0; i < _itemPool.Widgets.Count; i++)
		{
			UIWidget uIWidget = _itemPool.Widgets[i];
			uIWidget.GetComponent<PvpIslandGuideItem>().Set(array[i], array2[i]);
		}
		_itemPool.UpdateLayout();
	}

	protected override void UpdateLayout()
	{
		_itemPool.Reposition();
	}
}
