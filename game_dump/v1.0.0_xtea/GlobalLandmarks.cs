using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TerrainData;
using UnityEngine;

public class GlobalLandmarks : MonoBehaviour
{
	private static readonly string[] LandmarkNames = new string[2] { "ST_train_wreckage_01", "ST_highway_01" };

	private readonly List<GameObject> _landmarks = new List<GameObject>();

	public static bool IsKindOfGlobalLandmark(string path)
	{
		int i = 0;
		for (int num = LandmarkNames.Length; i < num; i++)
		{
			string value = LandmarkNames[i];
			if (path.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(LoadGlobalLandmarks());
		KSingleton<GameManager>.Instance().PostReconnect += delegate
		{
			((MonoBehaviour)this).StartCoroutine(LoadGlobalLandmarks());
		};
	}

	private IEnumerator LoadGlobalLandmarks()
	{
		for (int j = 0; j < _landmarks.Count; j++)
		{
			Object.Destroy((Object)(object)_landmarks[j]);
		}
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return null;
		}
		List<LandmarkInfo> landmarks = TerrainMeta.GlobalLandmarks;
		int i = 0;
		for (int count = landmarks.Count; i < count; i++)
		{
			LandmarkInfo info = landmarks[i];
			AddLandmark(info);
		}
	}

	private void AddLandmark(LandmarkInfo info)
	{
		string prefabName = TerrainMeta.GetLandmarkPrefab(info.Id);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(prefabName, typeof(GameObject), delegate(Object asset)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				GameObject obj = (GameObject)Object.Instantiate(asset);
				SetLandmark(obj, info);
			}
		});
	}

	private void SetLandmark([NotNull] GameObject obj, LandmarkInfo info)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tilePosition = default(Vector2);
		((Vector2)(ref tilePosition))._002Ector((float)(int)info.X, (float)(int)info.Y);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector((float)info.OffsetX, (float)info.OffsetY, (float)info.OffsetZ);
		val.x += 100f;
		val.z += 100f;
		Vector3 val2 = TerrainA6.TilePositionToClientPosition(tilePosition);
		val2 += val;
		obj.transform.parent = ((Component)this).transform;
		obj.transform.localRotation = Quaternion.Euler(0f, (float)(info.Rotate * 2), 0f);
		obj.transform.position = val2;
		_landmarks.Add(obj);
	}
}
