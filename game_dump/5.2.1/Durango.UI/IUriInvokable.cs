using System.Collections.Generic;
using JetBrains.Annotations;

namespace Durango.UI;

public interface IUriInvokable
{
	int InvokeUri([NotNull] string[] tokens, int index);

	IEnumerable<string> CollectUri();
}
