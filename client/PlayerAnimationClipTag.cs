using System;

[Flags]
public enum PlayerAnimationClipTag
{
	Default = 0,
	Irrevocable = 1,
	Run = 2,
	Dead = 8,
	RootMotion = 0x10,
	Once = 0x20,
	WaterFlowResist = 0x40,
	Riding = 0x80,
	BushResist = 0x200,
	SpineStabilize = 0x400
}
