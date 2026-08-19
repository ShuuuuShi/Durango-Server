using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendAddPage : MonoBehaviour
{
	private const char Separator = '#';

	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private UIInput _freqInput;

	[SerializeField]
	private GameObject _searchClearButton;

	[SerializeField]
	private GameObject _freqClearButton;

	[SerializeField]
	private FriendBeRequestedList _beRequestedList;

	[SerializeField]
	private FriendWaitAcceptList _waitAcceptList;

	[SerializeField]
	private FriendSearchResultList _searchList;

	private SocialGroup _parent;

	private Social _social;

	private void Awake()
	{
		EventDelegate.Add(_searchInput.onSubmit, SearchInput_Submitted);
		EventDelegate.Add(_freqInput.onSubmit, FreqInput_Submitted);
		EventDelegate.Add(_searchInput.onChange, SearchInput_Changed);
		EventDelegate.Add(_freqInput.onChange, FreqInput_Changed);
		UIEventListener.Get(_searchClearButton).onClick = SearchClearButton_Clicked;
		UIEventListener.Get(_freqClearButton).onClick = FreqClearButton_Clicked;
		_searchInput.defaultText = T._("친구 추가 할 캐릭터명 입력");
		_freqInput.defaultText = T._("# 주파수");
		_beRequestedList.WaitAcceptListClicked += ShowWaitAcceptList;
		_parent = GetComponentInParent<SocialGroup>();
		_parent.AddOnUpdated(Refresh);
	}

	private void OnEnable()
	{
		ShowRequestedList();
	}

	private void Refresh(Social social)
	{
		_social = social;
		if (_beRequestedList.gameObject.activeSelf)
		{
			_beRequestedList.Set(social);
		}
		if (_waitAcceptList.gameObject.activeSelf)
		{
			_waitAcceptList.Set(_social);
		}
	}

	private void SearchInput_Submitted()
	{
		string value = _searchInput.value;
		string value2 = _freqInput.value;
		if (string.IsNullOrEmpty(value))
		{
			ShowRequestedList();
			return;
		}
		string[] array = value.Split('#');
		if (array.Length > 1)
		{
			ShowSearchList(array[0], array[1]);
			SearchInput_Changed();
			FreqInput_Changed();
		}
		else
		{
			ShowSearchList(value, value2);
		}
	}

	private void FreqInput_Submitted()
	{
		SearchInput_Submitted();
	}

	private void SearchInput_Changed()
	{
		bool flag = string.IsNullOrEmpty(_searchInput.value);
		_searchClearButton.SetActive(!flag);
		if (flag)
		{
			ShowRequestedList();
		}
	}

	private void FreqInput_Changed()
	{
		bool flag = string.IsNullOrEmpty(_freqInput.value);
		_freqClearButton.SetActive(!flag);
		if (flag)
		{
			SearchInput_Submitted();
		}
	}

	private void SearchClearButton_Clicked(GameObject go)
	{
		ShowRequestedList();
	}

	private void FreqClearButton_Clicked(GameObject go)
	{
		_freqInput.value = string.Empty;
	}

	private void ShowRequestedList()
	{
		_searchInput.value = string.Empty;
		_freqInput.value = string.Empty;
		_parent.RemoveCloseStack("search");
		_parent.RemoveCloseStack("wait_accept");
		_searchList.gameObject.SetActive(value: false);
		_beRequestedList.gameObject.SetActive(value: true);
		_waitAcceptList.gameObject.SetActive(value: false);
		Refresh(_social);
	}

	private void ShowWaitAcceptList()
	{
		_searchInput.value = string.Empty;
		_freqInput.value = string.Empty;
		_parent.RemoveCloseStack("search");
		_parent.AddCloseStack("wait_accept", ShowRequestedList);
		_searchList.gameObject.SetActive(value: false);
		_beRequestedList.gameObject.SetActive(value: false);
		_waitAcceptList.gameObject.SetActive(value: true);
		Refresh(_social);
	}

	private void ShowSearchList(string key, string freq)
	{
		_searchInput.value = key;
		_freqInput.value = freq;
		_searchList.gameObject.SetActive(value: true);
		_beRequestedList.gameObject.SetActive(value: false);
		_waitAcceptList.gameObject.SetActive(value: false);
		_searchList.Search(key, freq);
		_parent.RemoveCloseStack("wait_accept");
		_parent.AddCloseStack("search", ShowRequestedList);
		Refresh(_social);
	}
}
