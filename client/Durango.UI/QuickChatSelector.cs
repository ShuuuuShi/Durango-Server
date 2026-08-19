using System;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class QuickChatSelector : TooltipBase
{
	private static readonly string[] QuickChats = new string[13]
	{
		T.N_("네"),
		T.N_("아니오"),
		T.N_("죄송합니다"),
		T.N_("감사합니다"),
		T.N_("도와줘요"),
		T.N_("좋아요"),
		T.N_("물러나세요"),
		T.N_("따라오세요"),
		T.N_("가는중이에요"),
		T.N_("제 위치 {0}에요"),
		T.N_("어디에요?"),
		T.N_("축하축하"),
		T.N_("다음에 봐요")
	};

	[SerializeField]
	private UIWidget _parentTarget;

	[SerializeField]
	private ListObjectPool _buttons;

	[SerializeField]
	private int _minWidth;

	public event Action<string> QuickChatClicked;

	protected override void Start()
	{
		base.Start();
		_buttons.Set(QuickChats.Length);
		int num = _minWidth;
		for (int i = 0; i < QuickChats.Length; i++)
		{
			GameObject gameObject = _buttons[i];
			gameObject.name = "QuickChat_" + i;
			UIEventListener uIEventListener = UIEventListener.Get(gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickQuickChatButton));
			UILabel component = gameObject.transform.Find("text").GetComponent<UILabel>();
			component.text = T._(QuickChats[i], "(X Y)");
			UIWidget component2 = gameObject.GetComponent<UIWidget>();
			component2.width = component.width + 22;
			num = Mathf.Max(num, component2.width);
		}
		Vector3 vector = base.Widget.localCorners[0] + new Vector3(10f, 10f);
		int height = _buttons.BaseObject.GetComponent<UIWidget>().height;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < _buttons.Count; j++)
		{
			UIWidget component3 = _buttons[j].GetComponent<UIWidget>();
			if (num3 + component3.width > num)
			{
				num2 += height + 5;
				num4 = Mathf.Max(num4, num3);
				num3 = component3.width + 5;
				component3.SetPosition(vector + Vector3.up * num2, 0f, 0f);
			}
			else
			{
				component3.SetPosition(vector + Vector3.right * num3 + Vector3.up * num2, 0f, 0f);
				num3 += component3.width + 5;
			}
		}
		base.Widget.width = num4 + 20;
		base.Widget.height = num2 + height + 20;
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnClickQuickChatButton(GameObject obj)
	{
		int num = _buttons.IndexOf(obj);
		if (num != -1 && this.QuickChatClicked != null)
		{
			this.QuickChatClicked(QuickChats[num]);
		}
		Hide();
	}

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
	}

	protected override void FillData()
	{
	}

	protected override void UpdateLayout()
	{
		Vector3 position = _parentTarget.worldCorners[1];
		position = base.transform.parent.InverseTransformPoint(position);
		position.x += 10f;
		base.transform.localPosition = position;
	}
}
