using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class RecentlyVisitItem : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _iconBackground;

	[SerializeField]
	private UILabel _label;

	public RegionTemplate Template { get; private set; }

	public void Set([NotNull] RegionTemplate template)
	{
		Template = template;
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(Template.MajorBiome());
		regionLayout.Sprite.Set(_icon);
		_iconBackground.color = regionLayout.Color;
		string arg = T._("{0} 해역", Template.ApparentClimate);
		string arg2 = LocalizeUtil.FormatLevel(Template.Level);
		_label.text = $"{arg}\n{arg2}";
	}
}
