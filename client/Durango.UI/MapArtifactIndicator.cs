using Building;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class MapArtifactIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private float _visibleZoom;

	public override float VisibleZoom => _visibleZoom;

	public void SetArtifact([NotNull] GameObject go, [NotNull] ArtifactIndicatorData indicatorData)
	{
		SetTarget(go);
		_visibleZoom = indicatorData.VisibleZoom;
		_sprite.spriteName = indicatorData.Icon;
		_sprite.color = indicatorData.Color;
		UIUtility.ResizeToSquare(_sprite, indicatorData.Size);
		_sprite.depth = 10;
	}
}
