using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Render.Camera;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using Shared.Quest;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ClientInteractionQuest : SelectableObject
{
	[Serializable]
	private class ActionElem
	{
		[LocalizableString]
		public string Name = string.Empty;

		public Interaction Action = InteractionData.Interaction.ClientSidePropAction;

		public string MotionName = string.Empty;

		public float Duration = 3f;

		public string Icon = string.Empty;

		public string Quest;
	}

	[SerializeField]
	private List<ActionElem> _actionList = new List<ActionElem>();

	[SerializeField]
	private EpicNPCType _type;

	[LocalizableString]
	[SerializeField]
	private string _targetName = string.Empty;

	[NonSerialized]
	public Pair<string, int>? Need;

	[NonSerialized]
	public bool Interaction;

	public override void InteractionTouched()
	{
		KUtility.DelayedCall(this, MakeInteractionMenuList, 0.1f);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		foreach (ActionElem action in _actionList)
		{
			string quest = action.Quest;
			if (menu.Id != quest)
			{
				continue;
			}
			SelectableObject.PlayMotion(action.MotionName, action.Duration);
			KUtility.DelayedCall(this, delegate
			{
				SelectableObject.OnPlayMotionFinished();
				if (quest == "need" && Need.HasValue)
				{
					PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
					selector.Filter((ItemData data) => data.PrototypeId == Need.Value.Item1).SelectableCount(Need.Value.Item2).OnConfirmed(delegate(IList<ItemData> items)
					{
						if (items != null && items.Count == Need.Value.Item2)
						{
							InteractWithEpicNPC msg = default(InteractWithEpicNPC);
							msg.Npc = _type;
							msg.ItemIds = items.Select((ItemData x) => x.Id).ToArray();
							Connections.Frontend.Send(msg);
						}
					})
						.OnChanged(delegate(IList<ItemData> items)
						{
							selector.Title(T._("건네주기 {0}/{1}", items.Count, Need.Value.Item2));
						})
						.Title(T._("건네주기 {0}/{1}", 0, Need.Value.Item2));
					selector.Show(3600f);
				}
				else
				{
					Connections.Frontend.Send(new InteractWithEpicNPC
					{
						Npc = _type,
						ItemIds = null
					});
				}
			}, action.Duration);
			return true;
		}
		return false;
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		if (Interaction)
		{
			foreach (ActionElem action in _actionList)
			{
				if (Need.HasValue != (action.Quest == "need"))
				{
					continue;
				}
				InteractionMenuData data = new InteractionMenuData(action.Action);
				data.Name = T._(action.Name);
				data.Icon = action.Icon;
				if (Need.HasValue)
				{
					List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(Need.Value.Item1);
					if (KUtility.GetSize(list) > 0)
					{
						data.Icon = list[0].Icon;
					}
				}
				data.Id = action.Quest;
				menuList.Add(data);
			}
		}
		string text = GetName();
		menuList.Name = text;
		menuList.Apply();
		Durango.Utils.Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
	}

	public override string GetName()
	{
		return T._(_targetName);
	}
}
