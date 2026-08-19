using Durango.UI.Control;
using L10N;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class ResistanceInfo : MonoBehaviour
{
	[SerializeField]
	private KeyValueLabel _labels;

	[SerializeField]
	private UISprite _weatherIcon;

	[SerializeField]
	private GameObject _iconHightlight;

	[SerializeField]
	private GameObject _backgroundHighlight;

	[SerializeField]
	private UISprite _progressBar;

	[SerializeField]
	private UILabel _progressLabel;

	public void Set(Biome biome, string typeName, string iconName, int level, int currentExp, int totalExp, float expRate, bool isHighlighted)
	{
		float num = ((totalExp > 0) ? ((float)currentExp / (float)totalExp) : 0f);
		if (level >= Singleton<Constants>.Instance.MaxLevels.Resistance)
		{
			_progressBar.color = PresetColor.UIYellow;
			_progressBar.fillAmount = 1f;
			_progressLabel.text = $"{1:P0}";
		}
		else
		{
			_progressBar.color = PresetColor.ProgressBarBlue;
			_progressBar.fillAmount = num;
			_progressLabel.text = $"{num:P0}";
		}
		_weatherIcon.spriteName = iconName;
		_iconHightlight.SetActive(isHighlighted);
		_backgroundHighlight.SetActive(isHighlighted);
		string arg = $"<em>{typeName}</em>";
		string text = T._("<em>{0} 기후</em>에서 피로도 소모량이 감소합니다. <em>{0} 기후</em>에서만 <em>경험치</em>를 얻을 수 있습니다.", LocalizeUtil.Get(biome));
		string text2 = T._("다음 레벨까지 필요한 경험치");
		string text3 = T._("현재 경험치 획득 비율");
		string text4 = T._("경험치 <em>{0:P0}</em> 획득중", expRate);
		string text5 = string.Format(T.Culture, "({0:N0}/{1:N0})", currentExp, totalExp);
		string arg2 = $"{text}\n\n[size=20][D4CEBEBE]{text2}\n{text5}[-][/size]\n\n{text3}\n{text4}";
		string arg3 = ((!isHighlighted) ? typeName : $"<em>{typeName}</em>");
		_labels.SetKey($"{arg3}  <help>title={arg},comment={arg2},width=340,sign=1,direction=horizontal</help>");
		_labels.SetValue($"<em>{LocalizeUtil.FormatLevel(level)}</em>");
	}
}
