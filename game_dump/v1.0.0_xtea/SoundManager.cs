using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : KSingleton<SoundManager>
{
	public class PooledSound
	{
		public int Seq;

		public float LastUsedTime;

		public AudioSource Source;

		public bool IsAlive => Source.isPlaying;
	}

	public class SoundData
	{
		public AudioClip Clip;
	}

	public struct PitchRange
	{
		public float Min;

		public float Max;

		public bool IsValid => Min > Max;

		public PitchRange(float min, float max)
		{
			Min = min;
			Max = max;
		}
	}

	private const float ReserveCachePeriod = 0.2f;

	[SerializeField]
	private GameObject _2dSoundTemplate;

	[SerializeField]
	private GameObject _3dSoundTemplate;

	[SerializeField]
	private int _2dSoundMaxCount = 10;

	[SerializeField]
	private int _3dSoundMaxCount = 20;

	[SerializeField]
	private AudioMixerGroup _mixer;

	private readonly Dictionary<string, SoundData> _soundDataDict = new Dictionary<string, SoundData>();

	private Transform _2DSoundPoolParent;

	private Transform _3DSoundPoolParent;

	private readonly List<PooledSound> _2DSoundPool = new List<PooledSound>();

	private readonly List<PooledSound> _3DSoundPool = new List<PooledSound>();

	private int _playSeq = 1;

	private List<Tuple<string, Action<AudioClip>>> _reservedToCache = new List<Tuple<string, Action<AudioClip>>>();

	private float _nextCacheReservedTime = -1f;

	public static float Volume { get; private set; }

	private void Start()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		GameObject val = new GameObject("2DSoundPool");
		val.transform.parent = ((Component)this).transform;
		_2DSoundPoolParent = val.transform;
		val = new GameObject("3DSoundPool");
		val.transform.parent = ((Component)this).transform;
		_3DSoundPoolParent = val.transform;
		SetVolume(Volume);
	}

	private void Update()
	{
		ProcessReservedCache();
	}

	public static void Cache(string fullPath, bool delayedCache = false)
	{
		if (KSingleton<SoundManager>.HasInstance())
		{
			KSingleton<SoundManager>.Instance().CacheSound(fullPath, delayedCache);
		}
	}

	private void CacheSound(string fullPath, bool delayedCache = false, Action<AudioClip> callbackAfterLoaded = null)
	{
		if (string.IsNullOrEmpty(fullPath))
		{
			return;
		}
		if (_soundDataDict.TryGetValue(fullPath, out var value))
		{
			if ((Object)(object)value.Clip != (Object)null)
			{
				callbackAfterLoaded?.Invoke(value.Clip);
				return;
			}
		}
		else
		{
			_soundDataDict.Add(fullPath, new SoundData());
		}
		if (delayedCache)
		{
			if (_reservedToCache.FindIndex((Tuple<string, Action<AudioClip>> c) => c.Item1 == fullPath) == -1)
			{
				_reservedToCache.Add(new Tuple<string, Action<AudioClip>>(fullPath, callbackAfterLoaded));
			}
		}
		else
		{
			RequestAssetImmediately(fullPath, callbackAfterLoaded);
		}
	}

	private void ProcessReservedCache()
	{
		if (!(RealTime.time < _nextCacheReservedTime))
		{
			_nextCacheReservedTime = RealTime.time + 0.2f;
			if (_reservedToCache.Count > 0)
			{
				RequestAssetImmediately(_reservedToCache[0].Item1, _reservedToCache[0].Item2);
				_reservedToCache.RemoveAt(0);
			}
		}
	}

	private void RequestAssetImmediately(string fullPath, Action<AudioClip> callbackAfterLoaded)
	{
		SoundData soundData = _soundDataDict[fullPath];
		if ((Object)(object)soundData.Clip != (Object)null)
		{
			if (callbackAfterLoaded != null)
			{
				callbackAfterLoaded(soundData.Clip);
			}
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(fullPath, typeof(AudioClip), delegate(Object asset)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				AudioClip val = (AudioClip)asset;
				_soundDataDict[fullPath].Clip = val;
				if (callbackAfterLoaded != null)
				{
					callbackAfterLoaded(val);
				}
			}
		});
	}

	private void RequestSound(int seq, string fullPath, bool use3DSound, Action<PooledSound> callbackAfterLoaded)
	{
		if (string.IsNullOrEmpty(fullPath))
		{
			return;
		}
		if (_soundDataDict.TryGetValue(fullPath, out var value))
		{
			if ((Object)(object)value.Clip == (Object)null)
			{
				CacheSound(fullPath, delayedCache: false, delegate(AudioClip clip)
				{
					RequestSoundInternal(seq, clip, use3DSound, callbackAfterLoaded);
				});
			}
			else
			{
				RequestSoundInternal(seq, value.Clip, use3DSound, callbackAfterLoaded);
			}
		}
		else
		{
			CacheSound(fullPath, delayedCache: false, delegate(AudioClip clip)
			{
				RequestSoundInternal(seq, clip, use3DSound, callbackAfterLoaded);
			});
		}
	}

	private void RequestSoundInternal(int seq, AudioClip clip, bool use3DSound, Action<PooledSound> callbackAfterLoaded)
	{
		PooledSound pooledSound = PickSoundFromPool(clip, use3DSound);
		pooledSound.Seq = seq;
		callbackAfterLoaded?.Invoke(pooledSound);
	}

	private PooledSound PickSoundFromPool(AudioClip clip, bool use3DSound)
	{
		List<PooledSound> list = ((!use3DSound) ? _2DSoundPool : _3DSoundPool);
		int num = ((!use3DSound) ? _2dSoundMaxCount : _3dSoundMaxCount);
		int count = list.Count;
		bool flag = num != 0 && num <= count;
		PooledSound pooledSound = null;
		for (int i = 0; i < count; i++)
		{
			PooledSound pooledSound2 = list[i];
			if (pooledSound2.IsAlive)
			{
				if (flag && (pooledSound == null || pooledSound.LastUsedTime > pooledSound2.LastUsedTime))
				{
					pooledSound = pooledSound2;
				}
				continue;
			}
			pooledSound = pooledSound2;
			break;
		}
		if (pooledSound == null)
		{
			pooledSound = new PooledSound();
			list.Add(pooledSound);
		}
		if ((Object)(object)pooledSound.Source == (Object)null)
		{
			GameObject val = Object.Instantiate<GameObject>((!use3DSound) ? _2dSoundTemplate : _3dSoundTemplate);
			val.transform.parent = ((!use3DSound) ? _2DSoundPoolParent : _3DSoundPoolParent);
			pooledSound.Source = val.GetComponent<AudioSource>();
		}
		pooledSound.Source.clip = clip;
		pooledSound.LastUsedTime = Time.time;
		return pooledSound;
	}

	public static int Play(string fullPath, bool loop = false, [Optional] PitchRange pitchMinMax)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<SoundManager>.HasInstance())
		{
			return 0;
		}
		return KSingleton<SoundManager>.Instance().PlaySound(fullPath, Vector3.zero, null, loop, pitchMinMax, use3DSound: false);
	}

	public static int Play(string fullPath, Vector3 position, Transform parent = null, bool loop = false, [Optional] PitchRange pitchMinMax)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<SoundManager>.HasInstance())
		{
			return 0;
		}
		return KSingleton<SoundManager>.Instance().PlaySound(fullPath, position, parent, loop, pitchMinMax, use3DSound: true);
	}

	public static int Play(AudioClip cip, bool loop = false, [Optional] PitchRange pitchMinMax)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<SoundManager>.HasInstance())
		{
			return 0;
		}
		return KSingleton<SoundManager>.Instance().PlaySound(cip, Vector3.zero, null, loop, pitchMinMax, use3DSound: false);
	}

	public static int Play(AudioClip cip, Vector3 position, Transform parent = null, bool loop = false, [Optional] PitchRange pitchMinMax)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<SoundManager>.HasInstance())
		{
			return 0;
		}
		return KSingleton<SoundManager>.Instance().PlaySound(cip, position, parent, loop, pitchMinMax, use3DSound: true);
	}

	private int PlaySound(string fullPath, Vector3 position, Transform parent, bool loop, PitchRange pitchMinMax, bool use3DSound)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		int playSeq = GetPlaySeq();
		RequestSound(playSeq, fullPath, use3DSound, delegate(PooledSound pooledSound)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			DoPlaySound(pooledSound, position, parent, loop, pitchMinMax, use3DSound);
		});
		return playSeq;
	}

	private int PlaySound(AudioClip cip, Vector3 position, Transform parent, bool loop, PitchRange pitchMinMax, bool use3DSound)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		int playSeq = GetPlaySeq();
		RequestSoundInternal(playSeq, cip, use3DSound, delegate(PooledSound pooledSound)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			DoPlaySound(pooledSound, position, parent, loop, pitchMinMax, use3DSound);
		});
		return playSeq;
	}

	private int GetPlaySeq()
	{
		int playSeq = _playSeq;
		_playSeq = ((_playSeq == int.MaxValue) ? 1 : (_playSeq + 1));
		return playSeq;
	}

	private void DoPlaySound(PooledSound sound, Vector3 position, Transform parent, bool loop, PitchRange pitchMinMax, bool use3DSound)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (sound == null)
		{
			return;
		}
		if (use3DSound)
		{
			if ((Object)(object)parent == (Object)null)
			{
				((Component)sound.Source).transform.parent = _3DSoundPoolParent;
				((Component)sound.Source).transform.position = position;
			}
			else
			{
				((Component)sound.Source).transform.parent = parent;
				((Component)sound.Source).transform.localPosition = position;
			}
		}
		sound.Source.pitch = ((!pitchMinMax.IsValid) ? 1f : Random.Range(pitchMinMax.Min, pitchMinMax.Max));
		sound.Source.Play();
		sound.Source.volume = 1f;
		sound.Source.loop = loop;
	}

	public static void Stop(int playSeq)
	{
		if (KSingleton<SoundManager>.HasInstance())
		{
			KSingleton<SoundManager>.Instance().StopSound(playSeq);
		}
	}

	private void StopSound(int playSeq)
	{
		int num = -1;
		for (int i = 0; i < 2; i++)
		{
			List<PooledSound> list = ((i != 0) ? _3DSoundPool : _2DSoundPool);
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Seq == playSeq)
				{
					num = j;
				}
			}
			if (num != -1)
			{
				if (list[num].IsAlive)
				{
					list[num].Source.Stop();
				}
				break;
			}
		}
	}

	public static void SetVolume(float val)
	{
		Volume = Mathf.Clamp01(val);
		if (KSingleton<SoundManager>.HasInstance() && (Object)(object)KSingleton<SoundManager>.Instance()._mixer != (Object)null)
		{
			float num = ((Volume != 0f) ? (20f * Mathf.Log10(Volume)) : (-80f));
			KSingleton<SoundManager>.Instance()._mixer.audioMixer.SetFloat("Volume", num);
		}
	}
}
