using System;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class EmptyPersonalEstatePage : MonoBehaviour
{
	[SerializeField]
	private SelectableButton _contextButton;

	[SerializeField]
	private GameObject _hasPersonalRegion;

	[SerializeField]
	private GameObject _selectPersonalRegion;

	public Transform DeclareButton => _contextButton.transform;

	public event Action DeclareButtonClicked;

	public void Refresh()
	{
		_hasPersonalRegion.gameObject.SetActive(value: false);
		_selectPersonalRegion.SetActive(value: false);
		EstateSystem.GetPersonalRegionInfo(OnPersonalRegionInfo);
		UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
	}

	private void OnPersonalRegionInfo(PersonalRegionInfo msg)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		PersonalRegion? personalRegion = msg.PersonalRegion;
		if (personalRegion.HasValue)
		{
			_hasPersonalRegion.SetActive(value: true);
			_selectPersonalRegion.SetActive(value: false);
			if (GameManager.Region.Role() != Role.Personal || GameManager.Region.Id != msg.PersonalRegion.Value.Region.Id)
			{
				_contextButton.Text = T._("개인섬 이동");
				_contextButton.Clicked = delegate
				{
					UIBase.CloseAllUI();
					EstateSystem.ReturnToEstate(OwnerType.PersonalPlayer);
				};
				return;
			}
			_contextButton.Text = T._("사유지 선언");
			_contextButton.Clicked = delegate
			{
				if (this.DeclareButtonClicked != null)
				{
					this.DeclareButtonClicked();
				}
			};
		}
		else
		{
			_hasPersonalRegion.SetActive(value: false);
			_selectPersonalRegion.SetActive(value: true);
		}
	}
}
