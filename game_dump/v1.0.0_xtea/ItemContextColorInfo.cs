using System;
using L10N;
using UnityEngine;

public class ItemContextColorInfo : ItemContextBase
{
	[SerializeField]
	private ListObjectPool _colors;

	protected override void OnInit()
	{
		base.Id = "item_color";
		base.HeaderText = T._("색상");
		_colors.Init(OnInitColorObject);
		_body.height = _colors.BaseObject.GetComponent<UIWidget>().height;
	}

	private void OnInitColorObject(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickColorObject));
	}

	private void OnClickColorObject(GameObject obj)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		UISprite component = ((Component)obj.transform.FindChild("upper")).GetComponent<UISprite>();
		Color32 val = Color32.op_Implicit(component.color);
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, $"R: {val.r}\nG: {val.g}\nB: {val.b}");
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Sign = 1;
		widgetTooltipControl.Show((UIWidget)component, Vector2.zero, 3600f);
	}

	public void Set(ItemColor colors)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		_colors.Clear();
		Vector3 val = _body.localCorners[1];
		Vector3 val2 = _body.localCorners[2];
		for (int i = 0; i < colors.Count; i++)
		{
			Color val3 = colors[i];
			if (!(val3 == Color.clear))
			{
				UIWidget uIWidget = ((ListObjectPoolBase<GameObject>)_colors).Add<UIWidget>();
				switch (_colors.Count)
				{
				case 1:
					uIWidget.SetPosition(val, 0f, 1f);
					break;
				case 2:
					uIWidget.SetPosition(Vector3.Lerp(val, val2, 0.5f), 0.5f, 1f);
					break;
				case 3:
					uIWidget.SetPosition(val2, 1f, 1f);
					break;
				}
				UISprite component = ((Component)((Component)uIWidget).transform.FindChild("upper")).GetComponent<UISprite>();
				component.color = val3;
			}
		}
	}
}
