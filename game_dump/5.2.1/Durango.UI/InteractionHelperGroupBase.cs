using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperGroupBase : UIBase
{
	private const string PlayerPrefKey = "show_interaction_helper";

	[SerializeField]
	protected Selectable _searchButton;

	[SerializeField]
	protected InteractionHelperList _helperList;

	protected virtual void Start()
	{
		_helperList.ShowStateChanged += OnHelperShow;
		_searchButton.Clicked = ToggleHelperListVisible;
		_searchButton.SetClickSound(UISound.ClickType.ButtonDefault);
		if (Preferences.GetBool("show_interaction_helper"))
		{
			_helperList.Show();
		}
	}

	protected virtual void ToggleHelperListVisible()
	{
		if (_helperList.IsShow || GameSystem<CombatSystem>.Instance().CombatMode)
		{
			_helperList.Hide();
		}
		else
		{
			_helperList.Show();
		}
	}

	protected virtual void OnHelperShow()
	{
		Preferences.SetBool("show_interaction_helper", _helperList.IsShow);
		_searchButton.Selected = _helperList.IsShow;
	}
}
