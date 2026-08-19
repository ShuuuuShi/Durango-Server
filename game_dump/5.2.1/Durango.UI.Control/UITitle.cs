using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI.Control;

public class UITitle : NestedPrefabLinker<UITitleWidget>
{
	public enum TitleCurrencyType
	{
		SkillPoint,
		PetCount,
		PetVoucher,
		Wallet
	}

	[LocalizableString]
	[SerializeField]
	private string _titleName;

	[SerializeField]
	private bool _hideCloseButton;

	[SerializeField]
	private bool _hideBackButton;

	[Header("PC Only Settings")]
	[SerializeField]
	private bool _hideBorder;

	[SerializeField]
	private TitleCurrencyType[] _titleCurrencies;

	[SerializeField]
	private GameObject[] _inactiveOnPC;

	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!string.IsNullOrEmpty(_titleName))
		{
			base.Object.SetTitle(T._(_titleName));
		}
		base.Object.ShowCloseButton(!_hideCloseButton);
		base.Object.ShowBackButton(!_hideBackButton);
		UITitleWidget_PC uITitleWidget_PC = base.Object as UITitleWidget_PC;
		if (uITitleWidget_PC != null)
		{
			uITitleWidget_PC.ShowBorder(!_hideBorder);
			uITitleWidget_PC.SetTitleCurrencies(_titleCurrencies);
			GameObject[] inactiveOnPC = _inactiveOnPC;
			for (int i = 0; i < inactiveOnPC.Length; i++)
			{
				inactiveOnPC[i].SetActive(value: false);
			}
		}
	}

	public void ShowCloseButton(bool show)
	{
		_hideCloseButton = !show;
		if (base.Object != null)
		{
			base.Object.ShowCloseButton(show);
		}
	}

	public void ShowBackButton(bool show)
	{
		_hideBackButton = !show;
		if (base.Object != null)
		{
			base.Object.ShowBackButton(show);
		}
	}
}
