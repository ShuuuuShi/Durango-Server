using UnityEngine;

namespace Durango.Utils;

public static class Prefabs
{
	public static bool IsPrefab(GameObject gameObject)
	{
		return false;
	}

	public static bool UseSharedMaterial(GameObject gameObject)
	{
		return IsPrefab(gameObject) || !Application.isPlaying;
	}

	public static Material[] GetMaterials(Renderer renderer)
	{
		return (!UseSharedMaterial(renderer.gameObject)) ? renderer.materials : renderer.sharedMaterials;
	}

	public static Material GetMaterial(Renderer renderer)
	{
		return (!UseSharedMaterial(renderer.gameObject)) ? renderer.material : renderer.sharedMaterial;
	}

	public static void SaveToPrefab<T>(T obj) where T : MonoBehaviour
	{
	}

	public static Transform[] MappingBones(Transform[] dstBones, Transform[] srcBones)
	{
		if (KUtility.GetSize(dstBones) == 0 || KUtility.GetSize(srcBones) == 0)
		{
			return null;
		}
		Transform[] array = new Transform[srcBones.Length];
		for (int i = 0; i < srcBones.Length; i++)
		{
			Transform transform = srcBones[i];
			array[i] = dstBones[0];
			foreach (Transform transform2 in dstBones)
			{
				if (transform2.gameObject.activeSelf && transform2.name == transform.name)
				{
					array[i] = transform2;
					break;
				}
			}
		}
		return array;
	}
}
