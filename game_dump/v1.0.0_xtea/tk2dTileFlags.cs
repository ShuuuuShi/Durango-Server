using System;

[Flags]
public enum tk2dTileFlags
{
	None = 0,
	FlipX = 0x1000000,
	FlipY = 0x2000000,
	Rot90 = 0x4000000
}
