using System;

namespace Durango.UI.Control;

[Flags]
public enum VisibleType
{
	Base = 1,
	HideOnBattle = 2,
	VisibleOnCutScene = 4,
	HideToHover = 8,
	HideOnRightSide = 0x10,
	HideOnLeftMenu = 0x20,
	VisibleOnFullScreen = 0x40,
	HideOnWorldMap = 0x80
}
