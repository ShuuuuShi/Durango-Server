using System.Collections.Generic;
using Durango.Logic;
using Messages;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class AdvancedResearchInfoWidget : UIWidget
{
	[SerializeField]
	private ClanResearchIconWidget _baseIcon;

	private ListObjectPool<ClanResearchIconWidget> _iconList;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_iconList = new ListObjectPool<ClanResearchIconWidget>();
			_iconList.BaseObject = _baseIcon;
			_iconList.UseBase = true;
			_iconList.Clear();
		}
	}

	public void Refresh()
	{
		Init();
		GameSystem<ResearchSystem>.Instance().GetClanResearchList(OnClanResearchList, ignoreCache: false);
	}

	private void OnClanResearchList(ClanResearchList researchList)
	{
		_iconList.BeginLoad();
		foreach (KeyValuePair<string, Yaml.ClanResearch> item in SingletonDict<string, Yaml.ClanResearch>.Instance)
		{
			if (item.Value.Category != ResearchCategory.Advanced)
			{
				continue;
			}
			ClanResearchIconWidget next = _iconList.GetNext();
			int num = -1;
			int i = 0;
			for (int size = KUtility.GetSize(researchList.ResearchList); i < size; i++)
			{
				if (researchList.ResearchList[i].ResearchId == item.Key)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				next.Set(new Messages.ClanResearch
				{
					ResearchId = item.Key
				});
			}
			else
			{
				next.Set(researchList.ResearchList[num]);
			}
		}
		_iconList.EndLoad();
		Vector3[] array = localCorners;
		UIUtility.WidgetsReposition(_iconList, Vector3.left, Vector3.Lerp(array[2], array[3], 0.5f) + new Vector3(-40f, 0f), 35f);
	}
}
