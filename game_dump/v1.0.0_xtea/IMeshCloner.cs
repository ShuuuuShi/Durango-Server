using UnityEngine;

public interface IMeshCloner
{
	void AddMeshCloners(SkinnedMeshRenderer[] renderers);

	void RemoveMeshCloners(SkinnedMeshRenderer[] renderers);
}
