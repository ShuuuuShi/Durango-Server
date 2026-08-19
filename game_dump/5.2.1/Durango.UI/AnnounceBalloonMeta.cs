using System;

namespace Durango.UI;

[Serializable]
public struct AnnounceBalloonMeta
{
	public SpriteData _icon;

	public SpriteData _iconEffect;

	public float _showDuration;

	public float _blinkDuration;

	public BalloonDuplicateType _duplicateType;
}
