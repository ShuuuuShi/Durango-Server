using UnityEngine;

namespace Durango.Model;

public interface IBoneMergedObserver
{
	void OnAttached(SkinnedMeshRenderer[] renderers);

	void OnDetached(SkinnedMeshRenderer[] renderers);
}
