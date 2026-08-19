using System;
using System.Collections;
using System.Linq;
using PitchDetector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PitchInput : MonoBehaviour
{
	private const int MaxDetectionsAllowed = 50;

	private const float RefValue = 0.1f;

	[SerializeField]
	private int _pitchTimeInterval = 100;

	[SerializeField]
	private int _cumulativeDetections = 5;

	[SerializeField]
	private float _minVolumeDB = -17f;

	[SerializeField]
	private int _midiThreshold = 1;

	[SerializeField]
	private int _detectMin = 21;

	[SerializeField]
	private int _detectMax = 108;

	private Detector _pitchDetector;

	private bool _isDeviceSelected;

	private bool _isStartingMicrophone;

	private float[] _data;

	private int _minFreq;

	private int _maxFreq;

	private int[] _detectionsMade;

	private int _detectionPointer;

	private bool _isDetectionClear;

	private int _prevMidi = -1;

	private Action<int> _onMidi;

	private string _selectedDevice;

	private bool _isListening;

	private void Update()
	{
		if (!_isDeviceSelected || !_isListening)
		{
			return;
		}
		((Component)this).GetComponent<AudioSource>().GetOutputData(_data, 0);
		float num = _data.Sum((float t) => t * t);
		float num2 = Mathf.Sqrt(num / (float)_data.Length);
		float num3 = 20f * Mathf.Log10(num2 / 0.1f);
		if (num3 < _minVolumeDB)
		{
			_prevMidi = -1;
			if (!_isDetectionClear)
			{
				_isDetectionClear = true;
				for (int i = 0; i < _cumulativeDetections; i++)
				{
					_detectionsMade[i] = 0;
				}
			}
			return;
		}
		if (_pitchDetector == null)
		{
			_pitchDetector = new Detector();
			_pitchDetector.setSampleRate(AudioSettings.outputSampleRate);
		}
		_pitchDetector.DetectPitch(_data);
		int num4 = _pitchDetector.lastMidiNote();
		_detectionsMade[_detectionPointer++] = num4;
		_detectionPointer %= _cumulativeDetections;
		_isDetectionClear = false;
		int num5 = FindMode();
		if (num5 >= _detectMin && num5 <= _detectMax && (_prevMidi == -1 || Mathf.Abs(_prevMidi - num5) > _midiThreshold))
		{
			_prevMidi = num5;
			if (_onMidi != null)
			{
				_onMidi(num5);
			}
		}
	}

	public void Listen(Action<int> onMidi)
	{
		_onMidi = onMidi;
		_isListening = true;
		((MonoBehaviour)this).StartCoroutine(StartMicrophone());
	}

	public void Stop()
	{
		_onMidi = null;
		_isListening = false;
		((MonoBehaviour)this).StartCoroutine(StopMicrophone());
	}

	private void GetMicCaps()
	{
		Microphone.GetDeviceCaps(_selectedDevice, ref _minFreq, ref _maxFreq);
		if (_minFreq + _maxFreq == 0)
		{
			_maxFreq = 44100;
		}
	}

	private IEnumerator StartMicrophone()
	{
		if (_isStartingMicrophone)
		{
			yield break;
		}
		_isStartingMicrophone = true;
		if (!Application.HasUserAuthorization((UserAuthorization)2))
		{
			yield return Application.RequestUserAuthorization((UserAuthorization)2);
		}
		if (Application.HasUserAuthorization((UserAuthorization)2) && !_isDeviceSelected)
		{
			SelectDevice();
		}
		if (_isDeviceSelected)
		{
			((Component)this).GetComponent<AudioSource>().volume = 0f;
			((Component)this).GetComponent<AudioSource>().clip = null;
			((Component)this).GetComponent<AudioSource>().loop = true;
			((Component)this).GetComponent<AudioSource>().mute = false;
			((Component)this).GetComponent<AudioSource>().clip = Microphone.Start(_selectedDevice, true, 10, _maxFreq);
			while (Microphone.GetPosition(_selectedDevice) <= 0)
			{
			}
			((Component)this).GetComponent<AudioSource>().Play();
		}
		_isStartingMicrophone = false;
	}

	private void SelectDevice()
	{
		string[] devices = Microphone.devices;
		if (devices != null && devices.Length != 0)
		{
			_selectedDevice = Microphone.devices[0];
			GetMicCaps();
			int num = (int)Mathf.Round((float)(AudioSettings.outputSampleRate * _pitchTimeInterval) / 1000f);
			_data = new float[num];
			_detectionsMade = new int[50];
			_isDeviceSelected = true;
		}
	}

	private IEnumerator StopMicrophone()
	{
		while (_isStartingMicrophone)
		{
			yield return null;
		}
		if (_isDeviceSelected)
		{
			((Component)this).GetComponent<AudioSource>().Stop();
			Microphone.End(_selectedDevice);
		}
	}

	private int Repetitions(int element)
	{
		int num = 0;
		int num2 = _detectionsMade[element];
		for (int i = 0; i < _cumulativeDetections; i++)
		{
			if (_detectionsMade[i] == num2)
			{
				num++;
			}
		}
		return num;
	}

	private int FindMode()
	{
		_cumulativeDetections = ((_cumulativeDetections <= 50) ? _cumulativeDetections : 50);
		int result = 0;
		int num = _cumulativeDetections / 2 + 1;
		for (int i = 0; i < _cumulativeDetections; i++)
		{
			int num2 = Repetitions(i);
			if (num2 > num)
			{
				result = _detectionsMade[i];
			}
		}
		return result;
	}
}
