using System.Runtime.CompilerServices;
using Durango.Logic;
using Messages;
using Shared.Ability;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Control;

public class UITitleWidget_PC : UITitleWidget
{
	[SerializeField]
	private UIWidget _borderWidget;

	[SerializeField]
	[EnumList(typeof(UITitle.TitleCurrencyType), false, 0, -1)]
	public GameObject[] _currencies;

	[SerializeField]
	private UILabel _skillPointLabel;

	[SerializeField]
	private UILabel _petCountLabel;

	[CompilerGenerated]
	private static UIEventListener.VoidDelegate cache0;

	[CompilerGenerated]
	private static UIEventListener.VoidDelegate cache1;

	[SerializeField]
	private UILabel _grazedPetCountLabel;

	[SerializeField]
	private UILabel _tStoneLabel;

	[SerializeField]
	private UILabel _warpGemLabel;

	protected override void Awake()
	{
		base.Awake();
		GameObject[] currencies = _currencies;
		for (int i = 0; i < currencies.Length; i++)
		{
			currencies[i].SetActive(value: false);
		}
		UpdateLayout();
	}

	protected override void OnStart()
	{
		GameObject[] currencies = _currencies;
		if (currencies != null && (nint)currencies.LongLength >= 4)
		{
			base.OnStart();
			if (Application.isPlaying)
			{
				UIEventListener.Get(_currencies[1].gameObject).onClick = PetGroup.OnClickPetCountButton;
				UIEventListener.Get(_currencies[2].gameObject).onClick = PetGroup.OnClickPetVoucherButton;
			}
		}
	}

	protected override void OnEnable()
	{
		GameObject[] currencies = _currencies;
		if (currencies == null || (nint)currencies.LongLength < 4)
		{
			return;
		}
		base.OnEnable();
		if (Application.isPlaying)
		{
			if (_currencies[0].activeInHierarchy)
			{
				UpdateSkillPoint();
			}
			if (_currencies[1].activeInHierarchy)
			{
				UpdatePetCount();
			}
			if (_currencies[3].activeInHierarchy)
			{
				UpdateWallet();
			}
		}
	}

	protected override void UpdateLayout()
	{
		if (base.Parent != null)
		{
			Background.leftAnchor.Set(base.transform, 0f, 0f);
			Background.rightAnchor.Set(base.transform, 1f, 0f);
			if (base.Parent.Anchor == UIBase.AnchorType.FullscreenMobileOnly)
			{
				Background.topAnchor.Set(base.transform, 1f, 1f);
				Background.rightAnchor.SetScreen(1f, 0f);
			}
			else
			{
				Background.topAnchor.Set(base.transform, 1f, 0f);
			}
			Background.ResetAnchors();
			_borderWidget.leftAnchor.Set(base.Parent.transform, 0f, -1f);
			_borderWidget.rightAnchor.Set(base.Parent.transform, 1f, 1f);
			_borderWidget.bottomAnchor.Set(base.Parent.transform, 0f, -1f);
			_borderWidget.topAnchor.Set(base.Parent.transform, 1f, 1f);
			_borderWidget.ResetAnchors();
			if (base.Parent.Anchor == UIBase.AnchorType.Fullscreen && TitleLabel != null)
			{
				TitleBarMenuGroup titleBarMenuGroup = UIManager.FindScript<TitleBarMenuGroup>();
				if (titleBarMenuGroup != null)
				{
					Transform titleBarRightAnchor = titleBarMenuGroup.TitleBarRightAnchor;
					if (titleBarRightAnchor != null)
					{
						TitleLabel.rightAnchor.Set(titleBarRightAnchor, 0f, -10f);
					}
				}
			}
		}
		Layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		RefreshTitleNextContainer();
	}

	public void ShowBorder(bool show)
	{
		_borderWidget.gameObject.SetActive(show);
	}

	private void EnableCurrency(UITitle.TitleCurrencyType type, bool enable)
	{
		GameObject gameObject = _currencies[(int)type];
		if (gameObject.activeSelf == enable)
		{
			return;
		}
		gameObject.SetActive(enable);
		switch (type)
		{
		case UITitle.TitleCurrencyType.Wallet:
			if (enable)
			{
				GameSystem<InventorySystem>.Instance().WalletUpdated += UpdateWallet;
				UpdatePetCount();
			}
			else
			{
				GameSystem<InventorySystem>.Instance().WalletUpdated -= UpdateWallet;
			}
			break;
		case UITitle.TitleCurrencyType.PetCount:
			if (enable)
			{
				GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += UpdatePetCount;
				UpdatePetCount();
			}
			else
			{
				GameSystem<StatisticsSystem>.Instance().StatisticsUpdated -= UpdatePetCount;
			}
			break;
		case UITitle.TitleCurrencyType.SkillPoint:
			if (enable)
			{
				GameSystem<SkillSystem>.Instance().SkillListUpdated += UpdateSkillPoint;
				UpdateSkillPoint();
			}
			else
			{
				GameSystem<SkillSystem>.Instance().SkillListUpdated -= UpdateSkillPoint;
			}
			break;
		case UITitle.TitleCurrencyType.PetVoucher:
			break;
		}
	}

	private void UpdateSkillPoint()
	{
		if (_currencies[0].activeInHierarchy)
		{
			_skillPointLabel.text = $"<em>{GameSystem<SkillSystem>.Instance().RemainSkillPoint}</em> <weak>/ {GameSystem<SkillSystem>.Instance().SkillPoint}</weak>";
		}
	}

	public void UpdatePetCount()
	{
		if (_currencies[1].activeInHierarchy)
		{
			PetManager.GetPetList(delegate(PetsInfo? petsInfo)
			{
				int num = (petsInfo.HasValue ? KUtility.GetSize(petsInfo.Value.Pets.Data) : 0);
				int num2 = (int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.MaxTamingPet, 99f);
				_petCountLabel.text = $"<em>{num}</em> <weak>/ {num2}</weak>";
				int num3 = (petsInfo.HasValue ? KUtility.GetSize(petsInfo.Value.GrazedPets.Data) : 0);
				int num4 = (petsInfo.HasValue ? petsInfo.Value.GrazableCount : 0);
				_grazedPetCountLabel.text = $"<em>{num3}</em> <weak>/ {num4}</weak>";
			});
		}
	}

	public void SetTitleCurrencies(UITitle.TitleCurrencyType[] titleCurrencies)
	{
		if (titleCurrencies != null)
		{
			foreach (UITitle.TitleCurrencyType type in titleCurrencies)
			{
				EnableCurrency(type, enable: true);
			}
			UpdateLayout();
		}
	}

	private void UpdateWallet()
	{
		if (_currencies[3].activeInHierarchy)
		{
			_tStoneLabel.text = $"<em>{InventorySystem.Wallet.GetBalance(Currency.TStone)}</em>";
			_warpGemLabel.text = $"<em>{InventorySystem.Wallet.GetBalance(Currency.Gem)}</em>";
		}
	}
}
