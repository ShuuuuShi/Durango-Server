using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ParticleManager : KSingleton<ParticleManager>
{
	public class PooledParticle
	{
		public float LastUsedTime;

		public GameObject Particle;

		public ParticleSystem ParticleSystem;

		public bool IsAlive => (Object)(object)ParticleSystem != (Object)null && ParticleSystem.IsAlive(true);
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

		public Quaternion Rotation;

		public Transform FollowingParent;

		public bool UseLocalPosition;

		public bool ComeForwardToCamera;

		public bool GroundDecal;

		public bool Reusable;

		public bool Sync;
	}

	private readonly Dictionary<string, ParticlePool> _particlePoolDict = new Dictionary<string, ParticlePool>();

	private Vector3 _camForward;

	protected override void OnAwake()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		SetPoolSize("Particle/FX_Prop_FireFly_01.prefab", 100u);
		SetPoolSize("Particle/FX_WaterRipple_standing_ancora_s.prefab", 30u);
		SetPoolSize("Particle/FX_WaterRipple_standing_ancora.prefab", 30u);
		_camForward = ((Component)KSingleton<MainCamera>.Instance()).transform.forward;
	}

	public static void Cache(string assetPath)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			ParticleManager particleManager = KSingleton<ParticleManager>.Instance();
			particleManager.CacheParticle(assetPath);
		}
	}

	[CanBeNull]
	public static GameObject EmitSync(string assetPath, Vector3 pos, Quaternion rotation, Transform followingParent = null, bool useLocalPosition = true, bool comeForwardToCamera = false, bool groundDecal = false, bool reusable = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		ParticleEmitParam param = default(ParticleEmitParam);
		param.Path = assetPath;
		param.Position = pos;
		param.Rotation = rotation;
		param.FollowingParent = followingParent;
		param.UseLocalPosition = useLocalPosition;
		param.ComeForwardToCamera = comeForwardToCamera;
		param.GroundDecal = groundDecal;
		param.Reusable = reusable;
		param.Sync = true;
		return KSingleton<ParticleManager>.Instance().EmitParticle(param);
	}

	public static void Emit(string assetPath, Vector3 pos, Quaternion rotation, Transform followingParent = null, bool useLocalPosition = true, bool comeForwardToCamera = false, bool groundDecal = false, bool reusable = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		ParticleEmitParam param = default(ParticleEmitParam);
		param.Path = assetPath;
		param.Position = pos;
		param.Rotation = rotation;
		param.FollowingParent = followingParent;
		param.UseLocalPosition = useLocalPosition;
		param.ComeForwardToCamera = comeForwardToCamera;
		param.GroundDecal = groundDecal;
		param.Reusable = reusable;
		param.Sync = false;
		KSingleton<ParticleManager>.Instance().EmitParticle(param);
	}

	public static void Stop(GameObject particle, bool immediately = true)
	{
		if (!((Object)(object)particle == (Object)null))
		{
			ParticleSystem component = particle.GetComponent<ParticleSystem>();
			component.Stop(true);
			if (immediately)
			{
				component.Clear();
			}
		}
	}

	private void CacheParticle(string assetPath)
	{
		ParticlePool pool = GetOrCreateParticlePool(assetPath);
		if ((Object)(object)pool.Prefab != (Object)null)
		{
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(Object asset)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			pool.Prefab = (GameObject)asset;
			if ((Object)(object)pool.Prefab == (Object)null)
			{
			}
			pool.PooledParticles = new List<PooledParticle>();
		});
	}

	[NotNull]
	private ParticlePool GetOrCreateParticlePool(string assetPath)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		ParticlePool particlePool = _particlePoolDict.Get(assetPath);
		if (particlePool == null)
		{
			particlePool = new ParticlePool();
			GameObject val = new GameObject(assetPath);
			val.transform.parent = ((Component)this).transform;
			particlePool.Parent = val.transform;
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

	[CanBeNull]
	private GameObject EmitParticle(ParticleEmitParam param)
	{
		if (string.IsNullOrEmpty(param.Path))
		{
			return null;
		}
		ParticlePool pool = GetOrCreateParticlePool(param.Path);
		bool flag = (Object)(object)pool.Prefab != (Object)null;
		if (!flag)
		{
			KSingleton<AssetBundleManager>.Instance().RequestAsset(param.Path, typeof(GameObject), delegate(Object asset)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				pool.Prefab = (GameObject)asset;
				if (!((Object)(object)pool.Prefab == (Object)null))
				{
					pool.PooledParticles = new List<PooledParticle>();
					if (!param.Sync)
					{
						EmitParticleInternal(param, pool);
					}
				}
			}, param.Sync);
		}
		return (!((Object)(object)pool.Prefab != (Object)null) || (!param.Sync && !flag)) ? null : EmitParticleInternal(param, pool);
	}

	[NotNull]
	private static GameObject RequestParticleInternal(ParticleEmitParam param, ParticlePool pool)
	{
		if (!param.Reusable)
		{
			return Object.Instantiate<GameObject>(pool.Prefab);
		}
		int count = pool.PooledParticles.Count;
		bool flag = pool.MaxCount != 0 && pool.MaxCount <= count;
		PooledParticle pooledParticle = null;
		for (int i = 0; i < count; i++)
		{
			PooledParticle pooledParticle2 = pool.PooledParticles[i];
			if (pooledParticle2.IsAlive)
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
		if (pooledParticle != null)
		{
			if (((Object)(object)pooledParticle.Particle == (Object)null) | ((Object)(object)pooledParticle.ParticleSystem == (Object)null))
			{
				pooledParticle.Particle = Object.Instantiate<GameObject>(pool.Prefab);
				pooledParticle.ParticleSystem = pooledParticle.Particle.GetComponent<ParticleSystem>();
			}
			else if (pooledParticle.ParticleSystem.playOnAwake)
			{
				pooledParticle.ParticleSystem.Play(true);
			}
		}
		else
		{
			pooledParticle = new PooledParticle();
			pooledParticle.Particle = Object.Instantiate<GameObject>(pool.Prefab);
			pooledParticle.ParticleSystem = pooledParticle.Particle.GetComponent<ParticleSystem>();
			pool.PooledParticles.Add(pooledParticle);
		}
		pooledParticle.Particle.transform.parent = pool.Parent;
		pooledParticle.LastUsedTime = Time.time;
		return pooledParticle.Particle;
	}

	[CanBeNull]
	private GameObject EmitParticleInternal(ParticleEmitParam param, ParticlePool pool)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = RequestParticleInternal(param, pool);
		if ((Object)(object)val.GetComponent<ParticleController>() == (Object)null)
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
		val.transform.position = param.Position;
		val.transform.rotation = param.Rotation;
		if ((Object)(object)param.FollowingParent != (Object)null)
		{
			val.transform.parent = param.FollowingParent;
			val.transform.localRotation = param.Rotation;
			if (param.UseLocalPosition)
			{
				val.transform.localPosition = param.Position;
			}
		}
		return val;
	}
}
