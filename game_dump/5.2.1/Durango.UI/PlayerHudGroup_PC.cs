using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class PlayerHudGroup_PC : PlayerHudGroupBase
{
	[SerializeField]
	private UILabel _playerLevel;

	[SerializeField]
	private SelectableWidget _levelBackground;

	protected override void Start()
	{
		GameObject[] array = new GameObject[3] { _lifeViewer.gameObject, _energyViewer.gameObject, _fatigueGauge.gameObject };
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UIEventListener.Get(array[i]).onHover = OnHoverHudGauge;
		}
		_levelBackground.Clicked = delegate
		{
			CharacterInfoGroup characterInfoGroup = UIManager.FindScript<CharacterInfoGroup>();
			if (characterInfoGroup != null)
			{
				if (characterInfoGroup.IsOpened)
				{
					characterInfoGroup.Close();
				}
				else
				{
					characterInfoGroup.Open();
				}
			}
		};
		GameSystem<StatisticsSystem>.Instance().LevelChanged += OnPlayerLevelChanged;
		base.Start();
	}

	protected void OnHoverHudGauge(GameObject go, bool state)
	{
		ShowGaugeTooltip(go, (!state) ? 0f : float.MaxValue);
	}

	protected void OnPlayerLevelChanged(int prevLevel, int newLevel)
	{
		_playerLevel.text = $"{newLevel}";
	}
}
