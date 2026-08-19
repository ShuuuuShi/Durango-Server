using System.Collections;
using ExploreData;
using Shared.Region;
using UnityEngine;
using UnityEngine.Audio;

public class BGMManager : KSingleton<BGMManager>
{
	private static float _volume;

	[SerializeField]
	private GameObject _bgmTemplete;

	[SerializeField]
	private AudioClip _battleBgm;

	[SerializeField]
	private float _bgmDelayAfterBattle = 30f;

	[EnumList(typeof(Role), true, 6)]
	[SerializeField]
	private AudioClip[] _islandBgmList;

	[SerializeField]
	private float _bgmLoopDelay = 90f;

	[SerializeField]
	private float _fadeRate = 1f;

	[SerializeField]
	private float _maxVolume = 0.7f;

	private AudioMixerGroup _mixer;

	private AudioSource _audioSource;

	private float _fadeBeginTime;

	private float _islandBgmAllowTime = -1f;

	private float _muteEndTime;

	private float _muteTweenDuration;

	public void AllowlslandBGM(bool allow)
	{
		_islandBgmAllowTime = ((!allow) ? (-1f) : Time.time);
	}

	public void Mute(float time)
	{
		if (_muteTweenDuration <= 0f && _muteEndTime <= 0f)
		{
			_muteTweenDuration += 1f;
		}
		_muteEndTime = Mathf.Max(_muteEndTime, Time.time + time);
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		GameObject val = ((Component)this).gameObject.AddChild(_bgmTemplete);
		_audioSource = val.GetComponent<AudioSource>();
		_mixer = _audioSource.outputAudioMixerGroup;
		_islandBgmAllowTime = Time.time + 5f;
	}

	private void Start()
	{
		SetVolume(_volume);
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += CombatSystem_ChangedCombatMode;
	}

	private void CombatSystem_ChangedCombatMode(bool combatMode)
	{
		if (combatMode)
		{
			_islandBgmAllowTime = -1f;
			_audioSource.loop = true;
			((MonoBehaviour)this).StartCoroutine(CoChangeTunes(_battleBgm, _fadeRate));
		}
		else
		{
			_islandBgmAllowTime = Time.time + _bgmDelayAfterBattle;
			((MonoBehaviour)this).StartCoroutine(CoFadeOut(_fadeRate));
		}
	}

	private void Update()
	{
		UpdateMuteTween();
		if (_islandBgmAllowTime < 0f || _islandBgmAllowTime >= Time.time)
		{
			return;
		}
		Region region = KSingleton<GameManager>.Instance().Region;
		if (region != null)
		{
			AudioClip val = null;
			int num = (int)region.Role();
			if (num >= 0 && num < _islandBgmList.Length)
			{
				val = _islandBgmList[num];
			}
			if (!((Object)(object)val == (Object)null) && (!_audioSource.isPlaying || !((Object)(object)_audioSource.clip == (Object)(object)val)))
			{
				_audioSource.loop = false;
				((MonoBehaviour)this).StartCoroutine(CoChangeTunes(val, _fadeRate));
				_islandBgmAllowTime = Time.time + val.length + _bgmLoopDelay;
			}
		}
	}

	private void UpdateMuteTween()
	{
		float deltaTime = Time.deltaTime;
		float num = 0f;
		if (_muteTweenDuration > 0f)
		{
			_muteTweenDuration -= deltaTime;
			_muteTweenDuration = Mathf.Max(_muteTweenDuration, 0f);
			num = _muteTweenDuration;
		}
		else if (_muteEndTime > 0f && _muteEndTime <= Time.time)
		{
			_muteTweenDuration = -1f;
			_muteEndTime = 0f;
		}
		else
		{
			if (!(_muteTweenDuration < 0f))
			{
				return;
			}
			_muteTweenDuration += deltaTime;
			_muteTweenDuration = Mathf.Min(_muteTweenDuration, 0f);
			num = 1f + _muteTweenDuration;
		}
		ChangeMixerVolume(_volume * num);
	}

	private IEnumerator CoChangeTunes(AudioClip clip, float fadeRate)
	{
		if (!((Object)(object)clip == (Object)null))
		{
			yield return ((MonoBehaviour)this).StartCoroutine(CoFadeOut(fadeRate));
			_audioSource.clip = clip;
			_audioSource.Play();
			yield return ((MonoBehaviour)this).StartCoroutine(CoFadeIn(fadeRate));
		}
	}

	private IEnumerator CoFadeOut(float fadeRate)
	{
		_fadeBeginTime = Time.time;
		float beginVolume = _audioSource.volume;
		while (_audioSource.volume > 0.1f)
		{
			_audioSource.volume = Mathf.SmoothStep(beginVolume, 0f, (Time.time - _fadeBeginTime) * fadeRate);
			yield return null;
		}
		_audioSource.volume = 0f;
		_audioSource.Stop();
		_audioSource.clip = null;
	}

	private IEnumerator CoFadeIn(float fadeRate)
	{
		_fadeBeginTime = Time.time;
		float beginVolume = _audioSource.volume;
		while (_audioSource.volume < _maxVolume - 0.1f)
		{
			_audioSource.volume = Mathf.SmoothStep(beginVolume, _maxVolume, (Time.time - _fadeBeginTime) * fadeRate);
			yield return null;
		}
		_audioSource.volume = _maxVolume;
	}

	public static void SetVolume(float val)
	{
		_volume = Mathf.Clamp01(val);
		if (KSingleton<BGMManager>.HasInstance())
		{
			KSingleton<BGMManager>.Instance().ChangeMixerVolume(_volume);
		}
	}

	private void ChangeMixerVolume(float volume)
	{
		if (!((Object)(object)_mixer == (Object)null))
		{
			float num = ((!(volume <= 0f)) ? (20f * Mathf.Log10(volume)) : (-80f));
			_mixer.audioMixer.SetFloat("Volume", num);
		}
	}
}
