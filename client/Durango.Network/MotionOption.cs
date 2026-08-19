using System;

namespace Durango.Network;

[Flags]
public enum MotionOption
{
	NORMAL = 0,
	LOOPING = 1,
	ALIGN_TO_PATH = 2,
	SNAP_ANGLE_BEGIN = 4,
	IN_PLACE_MOTION = 8,
	REVERSE = 0x10,
	PC_CLIENTSIDE_MOTION = 0x20,
	PHYSICAL_FORCED = 0x40,
	USE_LOCAL_ROOT_YAW = 0x80
}
