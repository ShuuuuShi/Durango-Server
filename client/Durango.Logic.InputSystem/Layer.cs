using System;

namespace Durango.Logic.InputSystem;

[Flags]
public enum Layer
{
	None = 0,
	GamePlay = 1,
	FullscreenUI = 2,
	InteractionUI = 4,
	InventoryUI = 8,
	BuildGridUI = 0x10,
	InputText = 0x20,
	TitleUI = 0x40,
	GuideUI = 0x80,
	ModalPopupUI = 0x100,
	WebBrowsing = 0x200,
	Prologue = 0x400,
	MiniGame = 0x800,
	All = -1,
	Default = 0xF,
	AllButText = -33,
	Menu = -929,
	Controllable = -2977
}
