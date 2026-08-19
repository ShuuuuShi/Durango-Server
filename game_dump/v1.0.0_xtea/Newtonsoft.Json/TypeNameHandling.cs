using System;

namespace Newtonsoft.Json;

[Flags]
public enum TypeNameHandling
{
	None = 0,
	Objects = 1,
	Arrays = 2,
	Auto = 4,
	All = 3
}
