using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoPlay_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ClientAnimalGroup _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoPlay_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ClientAnimalGroup clientAnimalGroup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				clientAnimalGroup._spawnBeginTime = Time.time;
				SoundManager.PlayEvent(clientAnimalGroup._spawnSound);
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (clientAnimalGroup._spawnList.Count > 0)
			{
				float num2 = Time.time - clientAnimalGroup._spawnBeginTime;
				if (clientAnimalGroup._cachedPrefab != null)
				{
					for (int num3 = clientAnimalGroup._spawnList.Count - 1; num3 >= 0; num3--)
					{
						SpawnInfo spawnInfo = clientAnimalGroup._spawnList[num3];
						if (!(spawnInfo.SpawnTime > num2))
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(clientAnimalGroup._cachedPrefab);
							gameObject.SetActive(value: true);
							ClientAnimalActor component = gameObject.GetComponent<ClientAnimalActor>();
							if (!(component == null))
							{
								component.transform.position = spawnInfo.Transform.position;
								component.transform.rotation = spawnInfo.Transform.rotation;
								List<Vector3> list = new List<Vector3>();
								for (int i = 0; i < clientAnimalGroup._moveList.Count; i++)
								{
									list.Add(clientAnimalGroup._moveList[i].position + RandomRange(clientAnimalGroup._moveRandomRadius));
								}
								component.MoveTo(list);
								clientAnimalGroup._animals.Add(component);
								clientAnimalGroup._spawnList.RemoveAt(num3);
							}
						}
					}
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoPlay_003Ed__17(0)
		{
			_003C_003E4__this = this
		};
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
