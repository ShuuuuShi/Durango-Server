using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class FavoriteIslandsPage : MonoBehaviour, IUIInitializable
{
	private const int MaxCount = 20;

	[SerializeField]
	private UILabel _count;

	[SerializeField]
	private KGridScrollView _scrollView;

	void IUIInitializable.Init()
	{
		GameSystem<SocialSystem>.Instance().SocialUpdated += delegate
		{
			if (base.gameObject.activeSelf)
			{
				Refresh();
			}
		};
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Refresh();
	}

	private void Refresh()
	{
		string[] favoriteRegionOwners = GameSystem<SocialSystem>.Instance().Social.FavoriteRegionOwners;
		_count.text = $"<em>{KUtility.GetSize(favoriteRegionOwners)}</em> / {20}";
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		if (CalcRemain(favoriteRegionOwners) > 0)
		{
			nodes.GetNext().GetComponent<FavoriteIslandsNode>().Set(Node_Added);
		}
		for (int i = 0; i < KUtility.GetSize(favoriteRegionOwners); i++)
		{
			string entityId = favoriteRegionOwners[i];
			nodes.GetNext().GetComponent<FavoriteIslandsNode>().Set(entityId, delegate
			{
				UIBase.CloseAllUI();
				EstateSystem.VisitEstate(OwnerType.PersonalPlayer, entityId);
			}, delegate
			{
				string nameFreq = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId).GetNameFreq(24, string.Empty);
				UIManager.MessageBox.Show(T._("<em>{0}</em> 님의 섬을 즐겨찾기 목록에서 삭제합니다.", nameFreq), delegate(bool ok)
				{
					if (ok)
					{
						GameSystem<SocialSystem>.Instance().RemoveFavoriteRegionOwners(entityId);
					}
				});
			});
		}
		nodes.EndLoad();
		_scrollView.ResetPosition();
	}

	private static void Node_Added()
	{
		string[] favoriteRegionOwners = GameSystem<SocialSystem>.Instance().Social.FavoriteRegionOwners;
		int maxCount = CalcRemain(favoriteRegionOwners);
		UIManager.FindScript<PlayerSearchGroup>().OpenForMultiple(maxCount, T._("검색"), favoriteRegionOwners, delegate(IList<string> list)
		{
			if (KUtility.GetSize(list) > 0)
			{
				GameSystem<SocialSystem>.Instance().AddFavoriteRegionOwners(list);
			}
		}, T._("등록"), PlayerInfoWidget.Visible.PioneerGrade);
	}

	private static int CalcRemain(string[] entityIds)
	{
		return 20 - KUtility.GetSize(entityIds);
	}
}
