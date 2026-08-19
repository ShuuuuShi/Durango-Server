using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicKeyboard : MonoBehaviour
{
	public Action<int, bool> KeyboardPressed;

	[SerializeField]
	private ListObjectPool _whites;

	[SerializeField]
	private ListObjectPool _blacks;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private float _pressLimitTime = 1f;

	private static readonly bool[] WhiteFlag = new bool[12]
	{
		true, false, true, false, true, true, false, true, false, true,
		false, true
	};

	private int _min;

	private List<UISprite> _keyboards = new List<UISprite>();

	private List<KeyValuePair<int, float>> _pressKeys = new List<KeyValuePair<int, float>>();

	private Dictionary<int, AudioSource> _keyboardSound = new Dictionary<int, AudioSource>();

	private List<AudioSource> _fadeOut = new List<AudioSource>();

	private List<AudioSource> _audioSourcePool = new List<AudioSource>();

	private int _keyPressFrame;

	public string Instrument { get; set; }

	public bool IsScrolledKeyboard { get; private set; }

	public bool Disable { get; set; }

	private void OnEnable()
	{
		_scrollView.ResetPosition();
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<int, AudioSource> item in _keyboardSound)
		{
			Object.Destroy((Object)(object)item.Value);
		}
		_keyboardSound.Clear();
		for (int i = 0; i < _fadeOut.Count; i++)
		{
			Object.Destroy((Object)(object)_fadeOut[i]);
		}
		_fadeOut.Clear();
		for (int j = 0; j < _audioSourcePool.Count; j++)
		{
			Object.Destroy((Object)(object)_audioSourcePool[j]);
		}
		_audioSourcePool.Clear();
	}

	private void Update()
	{
		if (_pressKeys.Count > 0)
		{
			float time = Time.time;
			for (int num = _pressKeys.Count - 1; num >= 0; num--)
			{
				if (_pressKeys[num].Value < time)
				{
					UnpressKey(_pressKeys[num].Key);
				}
			}
		}
		for (int num2 = _fadeOut.Count - 1; num2 >= 0; num2--)
		{
			float volume = _fadeOut[num2].volume;
			if (volume > 0f && _fadeOut[num2].isPlaying)
			{
				volume -= Time.deltaTime / 0.5f;
				_fadeOut[num2].volume = Mathf.Clamp01(volume);
			}
			else
			{
				AudioSource val = _fadeOut[num2];
				_fadeOut.RemoveAt(num2);
				val.Stop();
				_audioSourcePool.Add(val);
			}
		}
	}

	public void Init(int min, int max)
	{
		_whites.Set(0);
		_blacks.Set(0);
		_min = min;
		_keyboards.Clear();
		int num = WhiteFlag.Length;
		for (int i = min; i <= max; i++)
		{
			int num2 = i % num;
			ListObjectPool listObjectPool = ((!WhiteFlag[num2]) ? _blacks : _whites);
			UISprite uISprite = ((ListObjectPoolBase<GameObject>)listObjectPool).Add<UISprite>();
			UIEventListener uIEventListener = UIEventListener.Get(((Component)uISprite).gameObject);
			uIEventListener.onPress = OnPressKey;
			_keyboards.Add(uISprite);
		}
		UIScrollView scrollView = _scrollView;
		scrollView.onDragStarted = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView.onDragStarted, new UIScrollView.OnDragNotification(OnDragStarted));
		Reposition();
	}

	private void OnDragStarted()
	{
		IsScrolledKeyboard = true;
	}

	public void Reposition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _whites.BaseObject.transform.localPosition;
		int num = _whites.BaseObject.GetComponent<UISprite>().width / 2;
		int num2 = WhiteFlag.Length;
		int i = 0;
		for (int count = _keyboards.Count; i < count; i++)
		{
			UISprite uISprite = _keyboards[i];
			((Component)uISprite).transform.localPosition = val;
			val = ((WhiteFlag[(_min + i) % num2] != WhiteFlag[(_min + i + 1) % num2]) ? (val + Vector3.right * (float)num) : (val + Vector3.right * (float)num * 2f));
		}
		_scrollView.ResetPosition();
	}

	private void OnPressKey(GameObject go, bool press)
	{
		if (Disable)
		{
			return;
		}
		int num = -1;
		int i = 0;
		for (int count = _keyboards.Count; i < count; i++)
		{
			if ((Object)(object)((Component)_keyboards[i]).gameObject == (Object)(object)go)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		int num2 = num + _min;
		if (press)
		{
			PressKey(num2, -1f);
			IsScrolledKeyboard = false;
			if (!string.IsNullOrEmpty(Instrument))
			{
				KSingleton<MusicManager>.Instance().InstrumentSound(Instrument, num2, out var clip, out var pitch, out var mixer);
				AudioSource audioSource = GetAudioSource();
				audioSource.clip = clip;
				audioSource.pitch = pitch;
				audioSource.outputAudioMixerGroup = mixer;
				audioSource.volume = 1f;
				audioSource.Play();
				_keyboardSound[num2] = audioSource;
			}
		}
		else
		{
			UnpressKey(num2);
			AudioSource val = _keyboardSound.Get(num2);
			_keyboardSound.Remove(num2);
			if ((Object)(object)val != (Object)null)
			{
				if (val.isPlaying)
				{
					_fadeOut.Add(val);
				}
				else
				{
					_audioSourcePool.Add(val);
				}
			}
		}
		if (KeyboardPressed != null)
		{
			KeyboardPressed(num2, press);
		}
	}

	public void PressKey(int midi)
	{
		PressKey(midi, _pressLimitTime);
	}

	public void PressKey(int midi, float limitTime)
	{
		int index = midi - _min;
		Press(index);
		index = -1;
		int i = 0;
		for (int count = _pressKeys.Count; i < count; i++)
		{
			if (_pressKeys[i].Key == midi)
			{
				index = i;
				break;
			}
		}
		float value = Time.time + limitTime;
		if (limitTime < 0f)
		{
			value = float.PositiveInfinity;
		}
		KeyValuePair<int, float> keyValuePair = new KeyValuePair<int, float>(midi, value);
		if (index == -1)
		{
			_pressKeys.Add(keyValuePair);
		}
		else
		{
			_pressKeys[index] = keyValuePair;
		}
	}

	public void UnpressKey(int midi)
	{
		int index = midi - _min;
		Unpress(index);
		index = -1;
		int i = 0;
		for (int count = _pressKeys.Count; i < count; i++)
		{
			if (_pressKeys[i].Key == midi)
			{
				index = i;
				break;
			}
		}
		if (index != -1)
		{
			_pressKeys.RemoveAt(index);
		}
	}

	private void Press(int index)
	{
		int num = index + _min;
		bool flag = WhiteFlag[num % WhiteFlag.Length];
		if (index >= 0 && index < _keyboards.Count)
		{
			_keyboards[index].spriteName = ((!flag) ? "img_black_keyboard_release" : "img_white_keyboard_release");
		}
	}

	private void Unpress(int index)
	{
		int num = index + _min;
		bool flag = WhiteFlag[num % WhiteFlag.Length];
		if (index >= 0 && index < _keyboards.Count)
		{
			_keyboards[index].spriteName = ((!flag) ? "img_black_keyboard" : "img_white_keyboard");
		}
	}

	public void AllUnpress()
	{
		int i = 0;
		for (int count = _pressKeys.Count; i < count; i++)
		{
			Unpress(_pressKeys[i].Key - _min);
		}
		_pressKeys.Clear();
	}

	private AudioSource GetAudioSource()
	{
		AudioSource val = null;
		if (_audioSourcePool.Count > 0)
		{
			val = _audioSourcePool[_audioSourcePool.Count - 1];
			_audioSourcePool.RemoveAt(_audioSourcePool.Count - 1);
		}
		else
		{
			val = ((Component)this).gameObject.AddComponent<AudioSource>();
		}
		return val;
	}
}
