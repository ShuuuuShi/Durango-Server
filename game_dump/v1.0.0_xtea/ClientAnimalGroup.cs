using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientAnimalGroup : MonoBehaviour
{
	[Serializable]
	private class SpawnInfo
	{
		public Transform Transform;

		public float SpawnTime;
	}

	[SerializeField]
	private string _spawnPrefabName;

	[SerializeField]
	private List<SpawnInfo> _spawnList;

	[SerializeField]
	private List<Transform> _moveList;

	[SerializeField]
	private float _moveRandomRadius;

	[SerializeField]
	private float _randomRadius;

	[SerializeField]
	private float _randomTime;

	[SerializeField]
	private float _randomYawMin;

	[SerializeField]
	private float _randomYawMax;

	[SerializeField]
	private AudioClipType _spawnSound;

	private float _spawnBeginTime;

	private GameObject _cachedPrefab;

	private bool _isPlayed;

	private readonly List<ClientAnimalActor> _animals = new List<ClientAnimalActor>();

	private void Awake()
	{
		SoundManager.Cache(_spawnSound);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(_spawnPrefabName, typeof(GameObject), delegate(Object asset)
		{
			_cachedPrefab = (GameObject)(object)((asset is GameObject) ? asset : null);
			if ((Object)(object)_cachedPrefab != (Object)null)
			{
				AnimationEventController component = _cachedPrefab.GetComponent<AnimationEventController>();
				if ((Object)(object)component != (Object)null)
				{
					component.Load();
				}
				_cachedPrefab.SetActive(false);
			}
		});
	}

	private void Update()
	{
		for (int num = _animals.Count - 1; num >= 0; num--)
		{
			if (!_animals[num].HasMovingPath())
			{
				_animals[num].Suicide();
				_animals.RemoveAt(num);
			}
		}
	}

	public void Play()
	{
		if (!_isPlayed)
		{
			_isPlayed = true;
			((MonoBehaviour)this).StartCoroutine(CoPlay());
		}
	}

	public IEnumerator CoPlay()
	{
		_spawnBeginTime = Time.time;
		SoundManager.Play((string)_spawnSound, loop: false, default(SoundManager.PitchRange));
		while (_spawnList.Count > 0)
		{
			float timePassed = Time.time - _spawnBeginTime;
			if ((Object)(object)_cachedPrefab != (Object)null)
			{
				for (int i = _spawnList.Count - 1; i >= 0; i--)
				{
					SpawnInfo info = _spawnList[i];
					if (!(info.SpawnTime > timePassed))
					{
						GameObject obj = Object.Instantiate<GameObject>(_cachedPrefab);
						obj.SetActive(true);
						ClientAnimalActor animal = obj.GetComponent<ClientAnimalActor>();
						if (!((Object)(object)animal == (Object)null))
						{
							((Component)animal).transform.position = info.Transform.position;
							((Component)animal).transform.rotation = info.Transform.rotation;
							List<Vector3> moveTargt = new List<Vector3>();
							for (int j = 0; j < _moveList.Count; j++)
							{
								moveTargt.Add(_moveList[j].position + RandomRange(_moveRandomRadius));
							}
							animal.MoveTo(moveTargt);
							_animals.Add(animal);
							_spawnList.RemoveAt(i);
						}
					}
				}
			}
			yield return null;
		}
	}

	private static Vector3 RandomRange(float range)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI * 2f * Random.value;
		float value = Random.value;
		value *= value;
		value *= range;
		return new Vector3(Mathf.Cos(num) * value, 0f, Mathf.Sin(num) * value);
	}
}
