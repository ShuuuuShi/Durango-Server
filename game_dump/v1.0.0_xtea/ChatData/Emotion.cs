using System;

namespace ChatData;

[Flags]
public enum Emotion
{
	None = 0,
	Smile = 1,
	Sad = 2,
	Yes = 4,
	No = 8,
	Question = 0x10
}
