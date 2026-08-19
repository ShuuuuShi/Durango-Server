using System;
using Durango.Logic.Item;
using Durango.Logic.Social;
using Durango.UI.Control;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class PurchasedWidget : UIWidget
{
	[Serializable]
	private struct WidgetColor
	{
		public Color Icon;

		public Color Background;
	}

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private GameObject _newObject;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _itemTexture;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private UILabel _mileageLabel;

	[SerializeField]
	private TweenerPlayer _mileageTweener;

	[SerializeField]
	private WidgetColor _colorNormal;

	[SerializeField]
	private WidgetColor _colorRare;

	public void Set(Purchase purchase)
	{
		_textLabel.text = purchase.CommodityId;
		bool flag = false;
		if (purchase.Content is ItemPurchaseContent)
		{
			Item item = ((ItemPurchaseContent)purchase.Content).Item;
			_textLabel.text = item.Name;
			_iconSprite.gameObject.SetActive(value: false);
			_itemTexture.gameObject.SetActive(value: true);
			_itemTexture.SetIcon(item.Icon, new ItemColor(item.ColorR, item.ColorG, item.ColorB));
			_levelLabel.gameObject.SetActive(value: true);
			_levelLabel.text = LocalizeUtil.FormatLevel(item.Level);
			_newObject.gameObject.SetActive(value: true);
			_mileageTweener.gameObject.SetActive(value: false);
		}
		else if (purchase.Content is EmotionPurchaseContent)
		{
			string emotion = ((EmotionPurchaseContent)purchase.Content).Emotion;
			Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(emotion);
			_textLabel.text = motion.Name;
			_iconSprite.gameObject.SetActive(value: true);
			_itemTexture.gameObject.SetActive(value: false);
			_iconSprite.spriteName = "icon_emotionbook";
			UIUtility.ResizeToSquare(_iconSprite);
			_levelLabel.gameObject.SetActive(value: false);
			if (motion.Available)
			{
				_newObject.gameObject.SetActive(value: false);
				if (motion.PaybackMileage > 0)
				{
					_mileageLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(motion.PaybackMileage, Currency.CashshopMileage);
					_mileageTweener.gameObject.SetActive(value: true);
				}
				else
				{
					_mileageTweener.gameObject.SetActive(value: false);
				}
			}
			else
			{
				_newObject.gameObject.SetActive(value: true);
				_mileageTweener.gameObject.SetActive(value: false);
			}
			flag = motion.IsRare;
		}
		else
		{
			_textLabel.text = purchase.Id;
			_newObject.gameObject.SetActive(value: false);
			_iconSprite.gameObject.SetActive(value: false);
			_itemTexture.gameObject.SetActive(value: false);
			_levelLabel.gameObject.SetActive(value: false);
			_mileageTweener.gameObject.SetActive(value: false);
		}
		if (flag)
		{
			_bgSprite.color = _colorRare.Background;
			_iconSprite.color = _colorRare.Icon;
			Glitter.On(_iconSprite);
		}
		else
		{
			_bgSprite.color = _colorNormal.Background;
			_iconSprite.color = _colorNormal.Icon;
			Glitter.Off(_iconSprite);
		}
	}

	public void PlayAnimation(float delay)
	{
		_tweenerPlayer.Play(delay);
	}

	public void PlayPaybackAnimation(float delay)
	{
		if (_mileageTweener.gameObject.activeSelf)
		{
			_mileageTweener.Play(delay);
		}
	}
}
