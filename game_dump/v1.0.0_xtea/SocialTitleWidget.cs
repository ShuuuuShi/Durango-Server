using System;
using L10N;
using UnityEngine;

public class SocialTitleWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _countTextLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private DefaultSelectableButton _searchButton;

	private float _searchLockTime;

	public event Action<string> OnSearchPlayer;

	private void Start()
	{
		EventDelegate.Add(_searchInput.onSubmit, OnSubmitSearchInput);
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_searchInput).gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnSelectSearchButton));
		DefaultSelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, new Action(OnSubmitSearchInput));
	}

	private void OnEnable()
	{
		_searchInput.value = string.Empty;
	}

	public void SetCount(string labelText, int count)
	{
		_countTextLabel.text = labelText;
		_countLabel.text = T._("{0}", count.ToString());
	}

	private void OnSubmitSearchInput()
	{
		float time = Time.time;
		if (!(time < _searchLockTime))
		{
			_searchLockTime = time + 0.5f;
			string value = _searchInput.value;
			if (this.OnSearchPlayer != null)
			{
				this.OnSearchPlayer(value);
			}
		}
	}

	private void OnSelectSearchButton(GameObject obj, bool select)
	{
		if (!select)
		{
			OnSubmitSearchInput();
		}
	}
}
