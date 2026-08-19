using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Render.Camera;
using Durango.UI;
using Durango.Utils;
using InteractionData;
using L10N;
using UnityEngine;

public class ClientRemovableProp : SelectableObject
{
	[Serializable]
	public class ActionElem
	{
		[LocalizableString]
		public string Name = string.Empty;

		public Interaction Action = Interaction.ClientSidePropAction;

		public string RequireTag = string.Empty;

		[LocalizableString]
		public string ErrorMsg = string.Empty;

		public string MotionName = string.Empty;

		public float Duration = 3f;

		public string Icon = string.Empty;
	}

	[LocalizableString]
	[SerializeField]
	private string _propName = string.Empty;

	[SerializeField]
	private List<ActionElem> _actionList = new List<ActionElem>();

	public event Action<string> ClientPropDestructed;

	public override void InteractionTouched()
	{
		KUtility.DelayedCall(this, MakeInteractionMenuList, 0.1f);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		for (int i = 0; i < _actionList.Count; i++)
		{
			ActionElem actionElem = _actionList[i];
			if (menu.Name != T._(actionElem.Name))
			{
				continue;
			}
			ItemData itemData = null;
			if (!string.IsNullOrEmpty(actionElem.RequireTag))
			{
				itemData = GetRequiredItem(actionElem.RequireTag);
				if (itemData == null)
				{
					UIManager.SystemMsg(T._(actionElem.ErrorMsg));
					return false;
				}
			}
			string model = itemData.GetModel(PlayerBehavior.LocalPlayer.IsMale);
			SelectableObject.PlayMotion(actionElem.MotionName, actionElem.Duration, model, itemData?.Icon.Colors ?? default(ItemColor));
			UIManager.FindScript<PlayGuideHelperGroupBase>().SetVisible(visible: false, "removing_prop", 0.3f);
			KUtility.DelayedCall(this, delegate
			{
				SelectableObject.OnPlayMotionFinished();
				UIManager.FindScript<PlayGuideHelperGroupBase>().SetVisible(visible: true, "removing_prop");
				if (this.ClientPropDestructed != null)
				{
					this.ClientPropDestructed(base.EntityId);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}, actionElem.Duration);
			return true;
		}
		return false;
	}

	private static ItemData GetRequiredItem(string tag)
	{
		List<ItemData> list = GameSystem<InventorySystem>.Instance().FilteringByTag(tag);
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		int count = _actionList.Count;
		for (int i = 0; i < count; i++)
		{
			InteractionMenuData data = new InteractionMenuData(_actionList[i].Action);
			data.Name = T._(_actionList[i].Name);
			data.Icon = _actionList[i].Icon;
			menuList.Add(data);
		}
		string text = GetName();
		menuList.Name = text;
		menuList.Apply();
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
	}

	public override string GetName()
	{
		return T._(_propName);
	}
}
