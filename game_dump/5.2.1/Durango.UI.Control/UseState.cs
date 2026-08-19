using System;

namespace Durango.UI.Control;

[Flags]
public enum UseState
{
	Color = 1,
	Scale = 2,
	Tweener = 4,
	TweenerMod = 8,
	Active = 0x10
}
