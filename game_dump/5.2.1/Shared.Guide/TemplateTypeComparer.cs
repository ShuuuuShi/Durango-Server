using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Guide;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TemplateTypeComparer : IEqualityComparer<TemplateType>
{
	public bool Equals(TemplateType x, TemplateType y)
	{
		return x == y;
	}

	public int GetHashCode(TemplateType x)
	{
		return (int)x;
	}
}
