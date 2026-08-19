using System.Collections.Generic;

namespace Shared.Guide;

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
