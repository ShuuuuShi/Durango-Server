using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Particle;

public class ParticleManager : Singleton<ParticleManager>
{
	public class ParticleHelper : MonoBehaviour
	{
		public int Id;

		private ParticleSystem _particleSystem;

		private Transform _pool;

		private NcAutoDestruct _autoDestruct;

		private bool _willBeReturn;

		public bool IsAlive
		{
			get
			{
				if (_willBeReturn)
				{
					return false;
				}
				bool flag = base.gameObject.activeInHierarchy;
				if (_particleSystem != null)
				{
					flag &= _particleSystem.IsAlive(withChildren: true);
				}
				return flag;
			}
		}

		public void Initialize(Transform pool)
		{
			_pool = pool;
			_particleSystem = GetComponent<ParticleSystem>();
			_autoDestruct = GetComponent<NcAutoDestruct>();
			if (_autoDestruct != null)
			{
				NcEffectBehaviour[] componentsInChildren = base.gameObject.GetComponentsInChildren<NcEffectBehaviour>(includeInactive: true);
				NcEffectBehaviour[] array = componentsInChildren;
				foreach (NcEffectBehaviour ncEffectBehaviour in array)
				{
					ncEffectBehaviour.OnSetReplayState();
				}
			}
		}

		public void Play()
		{
			base.gameObject.SetActive(value: true);
			if (_particleSystem != null)
			{
				_particleSystem.Play(withChildren: true);
			}
			else if (_autoDestruct != null)
			{
				NcEffectBehaviour[] componentsInChildren = base.gameObject.GetComponentsInChildren<NcEffectBehaviour>(includeInactive: true);
				NcEffectBehaviour[] array = componentsInChildren;
				foreach (NcEffectBehaviour ncEffectBehaviour in array)
				{
					ncEffectBehaviour.OnResetReplayStage(bClearOldParticle: true);
				}
			}
		}

		public void Stop(bool immediately)
		{
			if (_particleSystem != null && !immediately)
			{
				_particleSystem.Stop(withChildren: true);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public void ReturnToPool()
		{
			_willBeReturn = false;
			base.transform.parent = _pool;
			Stop(immediately: true);
		}

		private void OnDisable()
		{
			if (!(base.transform.parent == _pool) && Singleton<ParticleManager>.HasInstance())
			{
				_willBeReturn = true;
				Singleton<ParticleManager>.Instance().RegisterDisabled(this);
			}
		}
	}

	public class PooledParticle
	{
		public float LastUsedTime;

		public ParticleHelper Helper;
	}

	private class ParticlePool
	{
		public Transform Parent;

		public GameObject Prefab;

		public List<PooledParticle> PooledParticles;

		public uint MaxCount;
	}

	private struct ParticleEmitParam
	{
		public string Path;

		public Vector3 Position;

		public Vector3 Scale;

		public Quaternion Rotation;

		public Transform FollowingParent;

		public bool UseLocalPosition;

		public bool ComeForwardToCamera;

		public bool GroundDecal;

		public Transform ChasingTarget;

		public bool Reusable;

		public bool Limit;
	}

	public static class ParticleId
	{
		public const int Invalid = 0;

		private static int _id = 1;

		public static int Generate()
		{
			return _id++;
		}
	}

	private class ParticleObject
	{
		public enum LoadingStatus
		{
			NotYetLoaded,
			WillBeStopped,
			Loaded
		}

		public LoadingStatus Status;

		private GameObject _particle;

		public GameObject Particle
		{
			get
			{
				return _particle;
			}
			set
			{
				if (Status == LoadingStatus.NotYetLoaded)
				{
					_particle = value;
					if (this.Loaded != null)
					{
						this.Loaded(_particle);
						this.Loaded = null;
					}
					Status = LoadingStatus.Loaded;
				}
			}
		}

		public event Action<GameObject> Loaded;
	}

	private class CameraTransformProvider : NcBillboard.ICameraTransformProvider
	{
		public Transform GetTransform()
		{
			return (!Singleton<MainCamera>.HasInstance()) ? null : Singleton<MainCamera>.Instance().transform;
		}
	}

	private readonly Dictionary<string, ParticlePool> _particlePoolDict = new Dictionary<string, ParticlePool>();

	private readonly List<ParticleHelper> _disabledParticles = new List<ParticleHelper>();

	private readonly Dictionary<int, ParticleObject> _particles = new Dictionary<int, ParticleObject>();

	private readonly HashSet<string> _cached = new HashSet<string>();

	private Vector3 _camForward;

	protected override void OnAwake()
	{
		NcBillboard.CameraTransformProvider = new CameraTransformProvider();
		SetPoolSize("Particle/FX_WaterRipple_standing_ancora_s.prefab", 30u);
		SetPoolSize("Particle/FX_WaterRipple_standing_ancora.prefab", 30u);
		_camForward = Singleton<MainCamera>.Instance().transform.forward;
	}

	private void Update()
	{
		foreach (ParticleHelper disabledParticle in _disabledParticles)
		{
			if (disabledParticle != null)
			{
				disabledParticle.ReturnToPool();
			}
		}
		_disabledParticles.Clear();
	}

	public void RegisterDisabled(ParticleHelper helper)
	{
		_particles.Remove(helper.Id);
		_disabledParticles.Add(helper);
	}

	public static void Cache(string assetPath)
	{
		if (!string.IsNullOrEmpty(assetPath) && Singleton<ParticleManager>.HasInstance())
		{
			ParticleManager particleManager = Singleton<ParticleManager>.Instance();
			particleManager.CacheParticle(assetPath);
		}
	}

	public static int Emit(GameObject obj, string assetPath, string bone = null, bool follow = true)
	{
		Transform transform = obj.transform;
		if (!string.IsNullOrEmpty(bone))
		{
			Transform transform2 = KUtility.FindTransformByName(transform.gameObject, bone);
			if (transform2 != null)
			{
				transform = transform2;
			}
		}
		return (!follow) ? Emit(assetPath, transform.position, Quaternion.identity) : EmitFollow(assetPath, Vector3.zero, Quaternion.identity, transform);
	}

	public static int Emit(string assetPath, Vector3 pos, Quaternion rotation, bool comeForwardToCamera = false, bool groundDecal = false, Vector3 scale = default(Vector3), bool reusable = true, bool limit = true)
	{
		return DoEmit(assetPath, pos, rotation, scale, null, useLocalPosition: false, comeForwardToCamera, groundDecal, null, reusable, limit);
	}

	public static int EmitFollow(string assetPath, Vector3 pos, Quaternion rotation, Transform followingParent, bool useLocalPosition = true, bool comeForwardToCamera = false, bool groundDecal = false, Vector3 scale = default(Vector3), Transform chasingTarget = null, bool reusable = true, bool limit = true)
	{
		return DoEmit(assetPath, pos, rotation, scale, followingParent, useLocalPosition, comeForwardToCamera, groundDecal, chasingTarget, reusable, limit);
	}

	private static int DoEmit(string assetPath, Vector3 pos, Quaternion rotation, Vector3 scale, Transform followingParent, bool useLocalPosition, bool comeForwardToCamera, bool groundDecal, Transform chasingTarget, bool reusable, bool limit)
	{
		ParticleEmitParam param = default(ParticleEmitParam);
		param.Path = assetPath;
		param.Position = pos;
		param.Scale = scale;
		param.Rotation = rotation;
		param.FollowingParent = followingParent;
		param.ComeForwardToCamera = comeForwardToCamera;
		param.GroundDecal = groundDecal;
		param.UseLocalPosition = useLocalPosition;
		param.ChasingTarget = chasingTarget;
		param.Reusable = reusable;
		param.Limit = limit;
		return Singleton<ParticleManager>.Instance().EmitParticle(param);
	}

	public static void Stop(int particleId, bool immediately = true)
	{
		if (!Singleton<ParticleManager>.HasInstance())
		{
			return;
		}
		ParticleObject particleObject = Singleton<ParticleManager>.Instance()._particles.Get(particleId);
		if (particleObject != null)
		{
			switch (particleObject.Status)
			{
			case ParticleObject.LoadingStatus.NotYetLoaded:
				particleObject.Status = ParticleObject.LoadingStatus.WillBeStopped;
				break;
			case ParticleObject.LoadingStatus.Loaded:
				Singleton<ParticleManager>.Instance().Stop(particleId, particleObject.Particle, immediately);
				break;
			}
		}
	}

	private void Stop(int id, GameObject particle, bool immediately)
	{
		if (!(particle == null))
		{
			ParticleHelper component = particle.GetComponent<ParticleHelper>();
			if (component != null)
			{
				component.Stop(immediately);
			}
			else
			{
				UnityEngine.Object.Destroy(particle);
			}
			_particles.Remove(id);
		}
	}

	public GameObject GetParticleIfLoaded(int particleId)
	{
		if (particleId == 0)
		{
			return null;
		}
		return _particles.Get(particleId)?.Particle;
	}

	public void RegisterAction(int particleId, Action<GameObject> action)
	{
		ParticleObject particleObject = _particles.Get(particleId);
		if (particleObject != null)
		{
			switch (particleObject.Status)
			{
			case ParticleObject.LoadingStatus.NotYetLoaded:
				particleObject.Loaded += action;
				break;
			case ParticleObject.LoadingStatus.Loaded:
				action(particleObject.Particle);
				break;
			}
		}
	}

	private void CacheParticle(string assetPath)
	{
		if (_cached.Contains(assetPath))
		{
			return;
		}
		_cached.Add(assetPath);
		ParticlePool pool = GetOrCreateParticlePool(assetPath);
		if (pool.Prefab != null)
		{
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			pool.Prefab = (GameObject)asset;
			if (!(pool.Prefab == null))
			{
				pool.PooledParticles = new List<PooledParticle>();
			}
		});
	}

	[NotNull]
	private ParticlePool GetOrCreateParticlePool(string assetPath)
	{
		ParticlePool particlePool = _particlePoolDict.Get(assetPath);
		if (particlePool == null)
		{
			particlePool = new ParticlePool();
			GameObject gameObject = new GameObject(assetPath);
			gameObject.transform.parent = base.transform;
			particlePool.Parent = gameObject.transform;
			particlePool.MaxCount = 10u;
			_particlePoolDict.Add(assetPath, particlePool);
		}
		return particlePool;
	}

	private void SetPoolSize(string assetPath, uint count)
	{
		ParticlePool orCreateParticlePool = GetOrCreateParticlePool(assetPath);
		orCreateParticlePool.MaxCount = count;
	}

	private int EmitParticle(ParticleEmitParam param)
	{
		if (string.IsNullOrEmpty(param.Path))
		{
			return 0;
		}
		ParticlePool pool = GetOrCreateParticlePool(param.Path);
		bool flag = pool.Prefab != null;
		int id = ParticleId.Generate();
		_particles.Add(id, new ParticleObject());
		if (flag)
		{
			EmitParticleInternal(param, pool, id);
		}
		else
		{
			Singleton<AssetBundleManager>.Instance().RequestAsset(param.Path, typeof(GameObject), delegate(UnityEngine.Object asset)
			{
				pool.Prefab = (GameObject)asset;
				if (!(pool.Prefab == null))
				{
					pool.PooledParticles = new List<PooledParticle>();
					ParticleObject particleObject = _particles.Get(id);
					if (particleObject != null)
					{
						if (particleObject.Status == ParticleObject.LoadingStatus.WillBeStopped)
						{
							_particles.Remove(id);
						}
						else
						{
							EmitParticleInternal(param, pool, id);
						}
					}
				}
			});
		}
		return id;
	}

	[NotNull]
	private GameObject RequestParticleInternal(ParticleEmitParam param, ParticlePool pool, int id)
	{
		if (!param.Reusable)
		{
			return UnityEngine.Object.Instantiate(pool.Prefab);
		}
		int count = pool.PooledParticles.Count;
		bool flag = pool.MaxCount != 0 && pool.MaxCount <= count && param.Limit;
		PooledParticle pooledParticle = null;
		for (int i = 0; i < count; i++)
		{
			PooledParticle pooledParticle2 = pool.PooledParticles[i];
			if (pooledParticle2.Helper != null && pooledParticle2.Helper.IsAlive)
			{
				if (flag && (pooledParticle == null || pooledParticle.LastUsedTime > pooledParticle2.LastUsedTime))
				{
					pooledParticle = pooledParticle2;
				}
				continue;
			}
			pooledParticle = pooledParticle2;
			break;
		}
		if (pooledParticle == null)
		{
			pooledParticle = new PooledParticle();
			pool.PooledParticles.Add(pooledParticle);
		}
		if (pooledParticle.Helper == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(pool.Prefab, pool.Parent);
			ParticleHelper particleHelper = gameObject.AddComponent<ParticleHelper>();
			particleHelper.Initialize(pool.Parent);
			pooledParticle.Helper = particleHelper;
		}
		else
		{
			pooledParticle.Helper.transform.parent = pool.Parent;
		}
		_particles.Remove(pooledParticle.Helper.Id);
		pooledParticle.Helper.Id = id;
		pooledParticle.LastUsedTime = Time.time;
		pooledParticle.Helper.Play();
		return pooledParticle.Helper.gameObject;
	}

	private void EmitParticleInternal(ParticleEmitParam param, ParticlePool pool, int id)
	{
		GameObject gameObject = RequestParticleInternal(param, pool, id);
		if (param.ChasingTarget != null)
		{
			gameObject.transform.parent = param.FollowingParent;
			ChasingParticleUpdater chasingParticleUpdater = gameObject.AddMissingComponent<ChasingParticleUpdater>();
			chasingParticleUpdater.enabled = true;
			chasingParticleUpdater.ChasingTarget = param.ChasingTarget;
			chasingParticleUpdater.FollowingOffset = param.Position;
			chasingParticleUpdater.ToGround = param.GroundDecal;
			gameObject.transform.localRotation = param.Rotation;
		}
		else
		{
			if (!(gameObject.GetComponent<ParticleController>() != null))
			{
				if (param.GroundDecal)
				{
					param.Position.y = 5f;
					param.ComeForwardToCamera = false;
				}
				if (param.ComeForwardToCamera)
				{
					param.Position -= _camForward * 500f;
				}
			}
			if (param.FollowingParent != null)
			{
				gameObject.transform.parent = param.FollowingParent;
				gameObject.transform.localRotation = param.Rotation;
				if (param.UseLocalPosition)
				{
					gameObject.transform.localPosition = param.Position;
				}
				else
				{
					gameObject.transform.position = param.Position;
				}
			}
			else
			{
				gameObject.transform.position = param.Position;
				gameObject.transform.rotation = param.Rotation;
			}
		}
		if (param.Scale != default(Vector3))
		{
			gameObject.transform.localScale = param.Scale;
		}
		ParticleObject particleObject = _particles.Get(id);
		if (particleObject != null)
		{
			particleObject.Particle = gameObject;
		}
	}
}
