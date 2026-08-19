using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Utils;
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
	private SoundEventType _spawnSound;

	private float _spawnBeginTime;

	private GameObject _cachedPrefab;

	private bool _isPlayed;

	private readonly List<ClientAnimalActor> _animals = new List<ClientAnimalActor>();

	private void Awake()
	{
		SoundManager.PrepareEvent(_spawnSound);
		Singleton<AssetBundleManager>.Instance().RequestAsset(_spawnPrefabName, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			_cachedPrefab = asset as GameObject;
			if (_cachedPrefab != null)
			{
				AnimationEventController component = _cachedPrefab.GetComponent<AnimationEventController>();
				if (component != null)
				{
					component.Load();
				}
				_cachedPrefab.SetActive(value: false);
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
			StartCoroutine(CoPlay());
		}
	}

	public IEnumerator CoPlay()
	{
		_spawnBeginTime = Time.time;
		SoundManager.PlayEvent(_spawnSound);
		while (_spawnList.Count > 0)
		{
			float timePassed = Time.time - _spawnBeginTime;
			if (_cachedPrefab != null)
			{
				for (int num = _spawnList.Count - 1; num >= 0; num--)
				{
					SpawnInfo spawnInfo = _spawnList[num];
					if (!(spawnInfo.SpawnTime > timePassed))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(_cachedPrefab);
						gameObject.SetActive(value: true);
						ClientAnimalActor component = gameObject.GetComponent<ClientAnimalActor>();
						if (!(component == null))
						{
							component.transform.position = spawnInfo.Transform.position;
							component.transform.rotation = spawnInfo.Transform.rotation;
							List<Vector3> list = new List<Vector3>();
							for (int i = 0; i < _moveList.Count; i++)
							{
								list.Add(_moveList[i].position + RandomRange(_moveRandomRadius));
							}
							component.MoveTo(list);
							_animals.Add(component);
							_spawnList.RemoveAt(num);
						}
					}
				}
			}
			yield return null;
		}
	}

	private static Vector3 RandomRange(float range)
	{
		float f = (float)Math.PI * 2f * UnityEngine.Random.value;
		float value = UnityEngine.Random.value;
		value *= value;
		value *= range;
		return new Vector3(Mathf.Cos(f) * value, 0f, Mathf.Sin(f) * value);
	}
}
