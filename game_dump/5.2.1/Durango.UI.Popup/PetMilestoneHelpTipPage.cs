using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class PetMilestoneHelpTipPage : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KWidgetScrollView _items;

	[SerializeField]
	private ListObjectPool _titleWidgets;

	[SerializeField]
	private ListObjectPool _commentWidgets;

	void IUIInitializable.Init()
	{
		_items.Widgets.Clear();
		_titleWidgets.BeginLoad();
		_commentWidgets.BeginLoad();
		AddTitle(T._("동물의 성장이란?"));
		AddComment(T._("길들인 동물은 플레이어와 같이 다니면 플레이어가 경험치를 얻을 때 함께 성장할 수 있습니다. 또한 축사에서 홀로 훈련하거나 자원을 생산하며 성장할 수도 있습니다."));
		AddTitle(T._("속성 발견이란?"));
		AddComment(T._("동물은 성장하며 특정 레벨이 될 때마다 새로운 속성을 발견할 수 있습니다. 어떤 먹이를 줬는지에 따라 발견하는 속성이 달라질 수 있습니다. 같은 동물이라도 등급이 높으면, 성장 과정 중 더 자주 속성을 발견할 수 있습니다."));
		AddTitle(T._("특수 행동이란?"));
		AddComment(T._("동물은 60레벨까지 자라면 여러 특수 행동 중 한 가지 특수 행동을 익힙니다. 동물이 어떤 종류인지, 그리고 어떤 속성들을 갖고 있는지에 따라 익힐 수 있는 특수 행동이 달라집니다."));
		_titleWidgets.EndLoad();
		_commentWidgets.EndLoad();
	}

	private void OnEnable()
	{
		_items.ResetPosition();
	}

	private void AddTitle(string text)
	{
		GameObject next = _titleWidgets.GetNext();
		SetText(next, text);
		_items.Widgets.Add(next.GetComponent<UIWidget>());
	}

	private void AddComment(string text)
	{
		GameObject next = _commentWidgets.GetNext();
		SetText(next, text);
		_items.Widgets.Add(next.GetComponent<UIWidget>());
	}

	private void SetText(GameObject obj, string text)
	{
		obj.transform.Find("Text").GetComponent<UILabel>().text = text;
		obj.GetComponent<RectLayoutComponent>().UpdateLayout();
	}
}
