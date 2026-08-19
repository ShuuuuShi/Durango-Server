using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class FriendManagePage : MonoBehaviour
{
	private enum ManageType
	{
		Follow,
		Block
	}

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[EnumList(typeof(ManageType), false, 0, -1)]
	[SerializeField]
	private GameObject[] _pages;

	private ManageType _selectedType;

	private HorizontalTabList _tabList;

	private SocialGroup _parent;

	private void Awake()
	{
		_tabList = _tabLinker.Object.GetComponent<HorizontalTabList>();
		_tabList.BeginLoad();
		ManageType[] array = Enums<ManageType>.All();
		foreach (ManageType manageType in array)
		{
			string text = null;
			switch (manageType)
			{
			case ManageType.Follow:
				text = T._("즐겨찾기");
				break;
			case ManageType.Block:
				text = T._("차단한 사람");
				break;
			}
			_tabList.AddText(text, string.Empty);
		}
		_tabList.EndLoadByFixedSize(260);
		_tabList.Clicked += delegate(int index)
		{
			SelectType(Enums<ManageType>.All()[index]);
		};
		_parent = GetComponentInParent<SocialGroup>();
		_parent.AddOnUpdated(Refresh);
	}

	private void Refresh(Social social)
	{
		ManageType[] array = Enums<ManageType>.All();
		for (int i = 0; i < array.Length; i++)
		{
			ManageType manageType = array[i];
			string text = null;
			switch (manageType)
			{
			case ManageType.Follow:
				text = KUtility.GetSize(social.FollowingEntityIds).ToString();
				break;
			case ManageType.Block:
				text = KUtility.GetSize(social.BlockedEntityIds).ToString();
				break;
			}
			_tabList.Get(i).SetValue(text);
		}
		SelectType(_selectedType);
	}

	private void SelectType(ManageType type)
	{
		_selectedType = type;
		int num = Enums<ManageType>.All().IndexOf(type);
		_tabList.Select(num);
		for (int i = 0; i < _pages.Length; i++)
		{
			_pages[i].SetActive(i == num);
		}
	}
}
