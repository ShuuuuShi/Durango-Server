using System;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class SubCommodityItem : MonoBehaviour
{
	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private SubCommodityRewards _rewards;

	[SerializeField]
	private SelectableButton _receiveButton;

	[SerializeField]
	private UILabel _receivedLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private LevelGaugeController _levelGaugeController;

	private string _id;

	private bool _isInit;

	public event Action<string> Received;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_receivedLabel.text = T._("받음");
		_receiveButton.Text = T._("받기");
		SelectableButton receiveButton = _receiveButton;
		receiveButton.Clicked = (Action)Delegate.Combine(receiveButton.Clicked, (Action)delegate
		{
			if (this.Received != null)
			{
				this.Received(_id);
			}
		});
	}

	public void Set(Commodity subCommodity)
	{
		Init();
		_id = subCommodity.Id;
		_nameLabel.text = subCommodity.Title;
		_rewards.Set(subCommodity.ContentDescriptions);
	}

	public void SetAccepted()
	{
		_receiveButton.gameObject.SetActive(value: false);
		_receivedLabel.gameObject.SetActive(value: true);
		_background.gameObject.SetActive(value: false);
		_levelGaugeController.SetAccepted();
	}

	public void SetFirstAcceptable()
	{
		_receiveButton.gameObject.SetActive(value: true);
		_receivedLabel.gameObject.SetActive(value: false);
		_receiveButton.Disabled = false;
		_receiveButton.SetEffect(PresetButton.Effect.Emphasis);
		_background.gameObject.SetActive(value: true);
		_levelGaugeController.SetFirstAcceptable();
	}

	public void SetAcceptable()
	{
		_receiveButton.gameObject.SetActive(value: true);
		_receivedLabel.gameObject.SetActive(value: false);
		_receiveButton.Disabled = true;
		_receiveButton.SetEffect(PresetButton.Effect.None);
		_background.gameObject.SetActive(value: false);
		_levelGaugeController.SetAcceptable();
	}

	public void SetNonAcceptable()
	{
		_receiveButton.gameObject.SetActive(value: true);
		_receivedLabel.gameObject.SetActive(value: false);
		_receiveButton.Disabled = true;
		_receiveButton.SetEffect(PresetButton.Effect.None);
		_background.gameObject.SetActive(value: false);
		_levelGaugeController.SetNonAcceptable();
	}

	public void UpdateLayout()
	{
		_layout.UpdateLayout(null, 0f);
	}

	public void SetGaugeHeight(float gaugeHeight)
	{
		_levelGaugeController.SetGaugeHeight(gaugeHeight);
	}
}
