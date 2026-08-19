using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class EmotionQuickslotWidget : MonoBehaviour
{
	private const int IconSplitCount = 12;

	private const int MotionSplitCount = 12;

	[SerializeField]
	private KScrollView _emoticonGridList;

	[SerializeField]
	private PageIndexSprite _emoticonPageIndexSprite;

	[SerializeField]
	private KScrollView _motionHolderList;

	[SerializeField]
	private PageIndexSprite _motionPageIndexSprite;

	[SerializeField]
	private UILabel _noMotionFavoritesAlram;

	public event Action<Emoticon> EmoticonClicked;

	private void Awake()
	{
		_motionHolderList.AttachPageIndexSprite(_motionPageIndexSprite);
		_emoticonGridList.AttachPageIndexSprite(_emoticonPageIndexSprite);
	}

	public void Refersh()
	{
		UpdateEmoticons();
		UpdateMotions();
	}

	private void UpdateEmoticons()
	{
		List<List<Emoticon>> list = GameSystem<SocialSystem>.Instance().Emotional.Emoticons.Where((Emoticon elem) => elem.IsSubscribe() && elem.Available).ToList().Split(12);
		_emoticonGridList.Nodes.BeginLoad();
		for (int i = 0; i < list.Count; i++)
		{
			List<Emoticon> dataToOrganizeGrid = list[i].Fill(null, 12);
			VerticalLayoutWidget component = _emoticonGridList.Nodes.GetNext().GetComponent<VerticalLayoutWidget>();
			component.SetGrids(dataToOrganizeGrid, delegate(Emoticon emoticon, EmoticonWidget widget, int index)
			{
				widget.Set(emoticon, delegate
				{
					GameSystem<SocialSystem>.Instance().PlayEmoticon(emoticon);
					emoticon.ClearNotification();
					if (this.EmoticonClicked != null)
					{
						this.EmoticonClicked(emoticon);
					}
				});
			});
			component.UpdateLayout();
		}
		_emoticonGridList.Nodes.EndLoad();
		_emoticonGridList.ResetPosition();
		_emoticonPageIndexSprite.Make(list.Count);
	}

	private void UpdateMotions()
	{
		List<List<Durango.Logic.Social.Motion>> list = GameSystem<SocialSystem>.Instance().Emotional.Motions.Where((Durango.Logic.Social.Motion elem) => elem.IsSubscribe() && elem.Available).ToList().Split(12);
		_motionHolderList.Nodes.BeginLoad();
		for (int i = 0; i < list.Count; i++)
		{
			List<Durango.Logic.Social.Motion> dataToOrganizeGrid = list[i].Fill(null, 12);
			VerticalLayoutWidget component = _motionHolderList.Nodes.GetNext().GetComponent<VerticalLayoutWidget>();
			component.SetGrids(dataToOrganizeGrid, delegate(Durango.Logic.Social.Motion data, MotionWidget widget, int index)
			{
				widget.Set(data, delegate
				{
					GameSystem<SocialSystem>.Instance().PlayMotion(data);
					data.ClearNotification();
				});
			});
			component.UpdateLayout();
		}
		_motionHolderList.Nodes.EndLoad();
		_motionHolderList.ResetPosition();
		_motionPageIndexSprite.Make(list.Count);
		bool flag = list.Count == 0;
		_noMotionFavoritesAlram.gameObject.SetActive(flag && EmotionSelector.CanModify);
	}

	public EmoticonWidget FindEmoticonWidget(string key)
	{
		for (int i = 0; i < _emoticonGridList.Nodes.Count; i++)
		{
			EmoticonWidget[] componentsInChildren = _emoticonGridList.Nodes[i].GetComponentsInChildren<EmoticonWidget>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if ((bool)componentsInChildren[i] && componentsInChildren[i].Key == key)
				{
					return componentsInChildren[i];
				}
			}
		}
		return null;
	}
}
