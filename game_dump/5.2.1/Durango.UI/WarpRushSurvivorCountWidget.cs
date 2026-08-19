using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class WarpRushSurvivorCountWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UISprite _progressSprite;

	private void Awake()
	{
		base.gameObject.SetActive(GameManager.Region.IsWarpRush());
		GameSystem<WarpRushSystem>.Instance().SurvivorRegionInfoUpdated += FillSurvivorCount;
	}

	public void FillSurvivorCount()
	{
		int totalPlayerCount = GameSystem<WarpRushSystem>.Instance().TotalPlayerCount;
		int num = totalPlayerCount - GameSystem<WarpRushSystem>.Instance().RetiredPlayerCount;
		_descriptionLabel.text = $"<em>{num}</em>/{totalPlayerCount}";
		_progressSprite.fillAmount = (float)num / (float)totalPlayerCount;
	}

	private void OnClick()
	{
	}
}
