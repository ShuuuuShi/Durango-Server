using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoPhoGames.USpeak.Core;
using MoPhoGames.USpeak.Core.Utils;
using MoPhoGames.USpeak.Interface;
using UnityEngine;

[AddComponentMenu("USpeak/USpeaker")]
public class USpeaker : MonoBehaviour
{
	public static float RemoteGain = 1f;

	public static float LocalGain = 1f;

	public static bool MuteAll = false;

	public static List<USpeaker> USpeakerList = new List<USpeaker>();

	private static int InputDeviceID = 0;

	private static string InputDeviceName = string.Empty;

	public SpeakerMode SpeakerMode;

	public BandMode BandwidthMode;

	public float SendRate = 16f;

	public SendBehavior SendingMode;

	public bool UseVAD;

	public ThreeDMode _3DMode;

	public bool DebugPlayback;

	public bool AskPermission = true;

	public bool Mute;

	public float SpeakerVolume = 1f;

	public float VolumeThreshold = 0.01f;

	public int Codec;

	private USpeakCodecManager codecMgr;

	private AudioClip recording;

	private int recFreq;

	private int lastReadPos;

	private float sendTimer;

	private float sendt = 1f;

	private string[] micDeviceList;

	private float lastDeviceUpdate;

	private List<USpeakFrameContainer> sendBuffer = new List<USpeakFrameContainer>();

	private List<byte> tempSendBytes = new List<byte>();

	private ISpeechDataHandler audioHandler;

	private IUSpeakTalkController talkController;

	private int overlap;

	private USpeakSettingsData settings;

	private string currentDeviceName = string.Empty;

	private float talkTimer;

	private float vadHangover = 0.5f;

	private float lastVTime;

	private List<float[]> pendingEncode = new List<float[]>();

	private double played;

	private int index;

	private double received;

	private float[] receivedData;

	private float playDelay;

	private bool shouldPlay;

	private float lastTime;

	private BandMode lastBandMode;

	private int lastCodec;

	private ThreeDMode last3DMode;

	private int recordedChunkCount;

	private int micFoundDelay;

	private bool waitingToStartRec;

	[Obsolete("Use USpeaker._3DMode instead")]
	public bool Is3D
	{
		get
		{
			return _3DMode == ThreeDMode.SpeakerPan;
		}
		set
		{
			if (value)
			{
				_3DMode = ThreeDMode.SpeakerPan;
			}
			else
			{
				_3DMode = ThreeDMode.None;
			}
		}
	}

	public bool IsTalking => talkTimer > 0f;

	private int audioFrequency
	{
		get
		{
			if (recFreq == 0)
			{
				switch (BandwidthMode)
				{
				case BandMode.Narrow:
					recFreq = 8000;
					break;
				case BandMode.Wide:
					recFreq = 16000;
					break;
				case BandMode.UltraWide:
					recFreq = 32000;
					break;
				default:
					recFreq = 8000;
					break;
				}
			}
			return recFreq;
		}
	}

	public static void SetInputDevice(int deviceID)
	{
		InputDeviceID = deviceID;
		InputDeviceName = Microphone.devices[InputDeviceID];
	}

	public static USpeaker Get(Object source)
	{
		if (source is GameObject)
		{
			return ((GameObject)((source is GameObject) ? source : null)).GetComponent<USpeaker>();
		}
		if (source is Transform)
		{
			return ((Component)((source is Transform) ? source : null)).GetComponent<USpeaker>();
		}
		if (source is Component)
		{
			return ((Component)((source is Component) ? source : null)).GetComponent<USpeaker>();
		}
		return null;
	}

	public void GetInputHandler()
	{
		talkController = (IUSpeakTalkController)FindInputHandler();
	}

	public void DrawTalkControllerUI()
	{
		if (talkController != null)
		{
			talkController.OnInspectorGUI();
		}
		else
		{
			GUILayout.Label("No component available which implements IUSpeakTalkController\nReverting to default behavior - data is always sent", (GUILayoutOption[])(object)new GUILayoutOption[0]);
		}
	}

	public void ReceiveAudio(byte[] data)
	{
		if (settings == null || MuteAll || Mute || (SpeakerMode == SpeakerMode.Local && !DebugPlayback))
		{
			return;
		}
		if (SpeakerMode == SpeakerMode.Remote)
		{
			talkTimer = 1f;
		}
		byte[] @byte;
		for (int i = 0; i < data.Length; i += @byte.Length)
		{
			int num = BitConverter.ToInt32(data, i);
			@byte = USpeakPoolUtils.GetByte(num + 6);
			Array.Copy(data, i, @byte, 0, @byte.Length);
			USpeakFrameContainer uSpeakFrameContainer = default(USpeakFrameContainer);
			uSpeakFrameContainer.LoadFrom(@byte);
			USpeakPoolUtils.Return(@byte);
			float[] array = USpeakAudioClipCompressor.DecompressAudio(uSpeakFrameContainer.encodedData, uSpeakFrameContainer.Samples, 1, threeD: false, settings.bandMode, codecMgr.Codecs[Codec], RemoteGain);
			float num2 = (float)array.Length / (float)audioFrequency;
			received += num2;
			Array.Copy(array, 0, receivedData, index, array.Length);
			USpeakPoolUtils.Return(array);
			index += array.Length;
			if (index >= ((Component)this).GetComponent<AudioSource>().clip.samples)
			{
				index = 0;
			}
			((Component)this).GetComponent<AudioSource>().clip.SetData(receivedData, 0);
			if (!((Component)this).GetComponent<AudioSource>().isPlaying)
			{
				shouldPlay = true;
				if (playDelay <= 0f)
				{
					playDelay = num2 * 5f;
				}
			}
		}
	}

	public void InitializeSettings(int data)
	{
		MonoBehaviour.print((object)"Settings changed");
		settings = new USpeakSettingsData((byte)data);
		Codec = settings.Codec;
	}

	private void Awake()
	{
		USpeakerList.Add(this);
		if ((Object)(object)((Component)this).GetComponent<AudioSource>() == (Object)null)
		{
			((Component)this).gameObject.AddComponent<AudioSource>();
		}
		((Component)this).GetComponent<AudioSource>().clip = AudioClip.Create("vc", audioFrequency * 10, 1, audioFrequency, false);
		((Component)this).GetComponent<AudioSource>().spatialBlend = ((_3DMode != ThreeDMode.Full3D) ? 0f : 1f);
		((Component)this).GetComponent<AudioSource>().loop = true;
		receivedData = new float[audioFrequency * 10];
		codecMgr = USpeakCodecManager.Instance;
		lastBandMode = BandwidthMode;
		lastCodec = Codec;
		last3DMode = _3DMode;
	}

	private void OnDestroy()
	{
		USpeakerList.Remove(this);
	}

	private IEnumerator Start()
	{
		yield return null;
		audioHandler = (ISpeechDataHandler)FindSpeechHandler();
		talkController = (IUSpeakTalkController)FindInputHandler();
		if (audioHandler == null)
		{
			Debug.LogError((object)"USpeaker requires a component which implements the ISpeechDataHandler interface");
		}
		else if (SpeakerMode != SpeakerMode.Remote)
		{
			if (AskPermission && !Application.HasUserAuthorization((UserAuthorization)2))
			{
				yield return Application.RequestUserAuthorization((UserAuthorization)2);
			}
			if (!Application.HasUserAuthorization((UserAuthorization)2))
			{
				Debug.LogError((object)"Failed to start recording - user has denied microphone access");
			}
			else if (Microphone.devices.Length != 0)
			{
				UpdateSettings();
				sendt = 1f / SendRate;
				recording = Microphone.Start(currentDeviceName, true, 5, audioFrequency);
				MonoBehaviour.print((object)Microphone.devices[InputDeviceID]);
				currentDeviceName = Microphone.devices[InputDeviceID];
				micDeviceList = Microphone.devices;
			}
		}
	}

	private void Update()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		if (SpeakerMode == SpeakerMode.Local && Time.time >= lastDeviceUpdate)
		{
			lastDeviceUpdate = Time.time + 2f;
			micDeviceList = Microphone.devices;
		}
		talkTimer -= Time.deltaTime;
		((Component)this).GetComponent<AudioSource>().volume = SpeakerVolume;
		if (last3DMode != _3DMode)
		{
			last3DMode = _3DMode;
			StopPlaying();
			((Component)this).GetComponent<AudioSource>().clip = AudioClip.Create("vc", audioFrequency * 10, 1, audioFrequency, false);
			((Component)this).GetComponent<AudioSource>().spatialBlend = ((_3DMode != ThreeDMode.Full3D) ? 0f : 1f);
			((Component)this).GetComponent<AudioSource>().loop = true;
		}
		if (_3DMode == ThreeDMode.SpeakerPan)
		{
			Transform transform = ((Component)Camera.main).transform;
			Vector3 val = Vector3.Cross(transform.up, transform.forward);
			((Vector3)(ref val)).Normalize();
			float num = Vector3.Dot(((Component)this).transform.position - transform.position, val);
			float num2 = Vector3.Dot(((Component)this).transform.position - transform.position, transform.forward);
			float num3 = Mathf.Atan2(num, num2);
			float panStereo = Mathf.Sin(num3);
			((Component)this).GetComponent<AudioSource>().panStereo = panStereo;
		}
		if (((Component)this).GetComponent<AudioSource>().isPlaying)
		{
			if (lastTime > ((Component)this).GetComponent<AudioSource>().time)
			{
				played += ((Component)this).GetComponent<AudioSource>().clip.length;
			}
			lastTime = ((Component)this).GetComponent<AudioSource>().time;
			if (played + (double)((Component)this).GetComponent<AudioSource>().time >= received)
			{
				StopPlaying();
				shouldPlay = false;
			}
		}
		else if (shouldPlay)
		{
			playDelay -= Time.deltaTime;
			if (playDelay <= 0f)
			{
				((Component)this).GetComponent<AudioSource>().Play();
			}
		}
		if (SpeakerMode == SpeakerMode.Remote || audioHandler == null || micDeviceList.Length == 0)
		{
			return;
		}
		if (string.IsNullOrEmpty(InputDeviceName))
		{
			InputDeviceName = currentDeviceName;
		}
		if (string.IsNullOrEmpty(currentDeviceName) && micDeviceList[0] != currentDeviceName)
		{
			if (waitingToStartRec)
			{
				micFoundDelay--;
				if (micFoundDelay <= 0)
				{
					micFoundDelay = 0;
					waitingToStartRec = false;
					MonoBehaviour.print((object)("New device found: " + currentDeviceName));
					InputDeviceID = 0;
					InputDeviceName = micDeviceList[0];
					currentDeviceName = micDeviceList[0];
					StartRecording();
					UpdateSettings();
				}
			}
			else
			{
				waitingToStartRec = true;
				micFoundDelay = 5;
			}
		}
		else
		{
			if (InputDeviceName != currentDeviceName)
			{
				StopRecording();
				MonoBehaviour.print((object)("Using input device: " + InputDeviceName));
				currentDeviceName = InputDeviceName;
				StartRecording();
			}
			if (micDeviceList[Mathf.Min(InputDeviceID, micDeviceList.Length - 1)] != currentDeviceName)
			{
				bool flag = false;
				for (int i = 0; i < Microphone.devices.Length; i++)
				{
					if (micDeviceList[i] == currentDeviceName)
					{
						InputDeviceID = i;
						flag = true;
					}
				}
				if (!flag)
				{
					InputDeviceID = 0;
					InputDeviceName = micDeviceList[0];
					currentDeviceName = micDeviceList[0];
					MonoBehaviour.print((object)("Device unplugged, switching to: " + currentDeviceName));
					StartRecording();
				}
			}
		}
		if (lastBandMode != BandwidthMode || lastCodec != Codec)
		{
			UpdateSettings();
			lastBandMode = BandwidthMode;
			lastCodec = Codec;
		}
		bool flag2 = talkController == null || talkController.ShouldSend();
		if (flag2 && !Microphone.IsRecording(currentDeviceName))
		{
			StartRecording();
		}
		else if (!flag2 && Microphone.IsRecording(currentDeviceName))
		{
			StopRecording();
		}
		if ((Object)(object)recording == (Object)null)
		{
			return;
		}
		int position = Microphone.GetPosition(currentDeviceName);
		int num4 = position + recording.samples * recordedChunkCount;
		if (num4 < lastReadPos)
		{
			recordedChunkCount++;
		}
		position += recording.samples * recordedChunkCount;
		if (position <= overlap)
		{
			return;
		}
		try
		{
			int num5 = position - lastReadPos;
			int num6 = codecMgr.Codecs[Codec].GetSampleSize(audioFrequency);
			if (num6 == 0)
			{
				num6 = 100;
			}
			int num7 = lastReadPos;
			int num8 = Mathf.FloorToInt((float)(num5 / num6));
			for (int j = 0; j < num8; j++)
			{
				float[] @float = USpeakPoolUtils.GetFloat(num6);
				recording.GetData(@float, num7 % recording.samples);
				if (flag2)
				{
					talkTimer = 1f;
					OnAudioAvailable(@float);
				}
				USpeakPoolUtils.Return(@float);
				num7 += num6;
			}
			lastReadPos = num7;
		}
		catch (Exception)
		{
		}
		ProcessPendingEncodeBuffer();
		bool flag3 = true;
		if (SendingMode == SendBehavior.RecordThenSend && talkController != null)
		{
			flag3 = !flag2;
		}
		sendTimer += Time.deltaTime;
		if (!(sendTimer >= sendt) || !flag3)
		{
			return;
		}
		sendTimer = 0f;
		tempSendBytes.Clear();
		foreach (USpeakFrameContainer item in sendBuffer)
		{
			tempSendBytes.AddRange(item.ToByteArray());
		}
		sendBuffer.Clear();
		if (tempSendBytes.Count > 0)
		{
			audioHandler.USpeakOnSerializeAudio(tempSendBytes.ToArray());
		}
	}

	private void StartRecording()
	{
		recording = Microphone.Start(currentDeviceName, true, 5, audioFrequency);
		lastReadPos = 0;
		sendBuffer.Clear();
		recordedChunkCount = 0;
	}

	private void StopRecording()
	{
		Microphone.End(currentDeviceName);
		recording = null;
	}

	private void StopPlaying()
	{
		((Component)this).GetComponent<AudioSource>().Stop();
		((Component)this).GetComponent<AudioSource>().time = 0f;
		index = 0;
		played = 0.0;
		received = 0.0;
		lastTime = 0f;
	}

	private void UpdateSettings()
	{
		if (Application.isPlaying)
		{
			settings = new USpeakSettingsData();
			settings.bandMode = BandwidthMode;
			settings.Codec = Codec;
			audioHandler.USpeakInitializeSettings(settings.ToByte());
		}
	}

	private Component FindSpeechHandler()
	{
		Component[] components = ((Component)this).GetComponents<Component>();
		Component[] array = components;
		foreach (Component val in array)
		{
			if (val is ISpeechDataHandler)
			{
				return val;
			}
		}
		return null;
	}

	private Component FindInputHandler()
	{
		Component[] components = ((Component)this).GetComponents<Component>();
		Component[] array = components;
		foreach (Component val in array)
		{
			if (val is IUSpeakTalkController)
			{
				return val;
			}
		}
		return null;
	}

	private void OnAudioAvailable(float[] pcmData)
	{
		if (UseVAD && !CheckVAD(pcmData))
		{
			return;
		}
		int size = 1280;
		List<float[]> list = SplitArray(pcmData, size);
		foreach (float[] item in list)
		{
			pendingEncode.Add(item);
		}
	}

	private List<float[]> SplitArray(float[] array, int size)
	{
		List<float[]> list = new List<float[]>();
		float[] array2;
		for (int i = 0; i < array.Length; i += array2.Length)
		{
			array2 = array.Skip(i).Take(size).ToArray();
			list.Add(array2);
		}
		return list;
	}

	private void ProcessPendingEncodeBuffer()
	{
		int num = 100;
		float num2 = (float)num / 1000f;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup <= realtimeSinceStartup + num2 && pendingEncode.Count > 0)
		{
			float[] pcm = pendingEncode[0];
			pendingEncode.RemoveAt(0);
			ProcessPendingEncode(pcm);
		}
	}

	private void ProcessPendingEncode(float[] pcm)
	{
		int sample_count;
		byte[] encodedData = USpeakAudioClipCompressor.CompressAudioData(pcm, 1, out sample_count, lastBandMode, codecMgr.Codecs[lastCodec], LocalGain);
		USpeakFrameContainer item = default(USpeakFrameContainer);
		item.Samples = (ushort)sample_count;
		item.encodedData = encodedData;
		sendBuffer.Add(item);
	}

	private int CalculateSamplesRead(int readPos)
	{
		if (readPos >= lastReadPos)
		{
			return readPos - lastReadPos;
		}
		return audioFrequency * 10 - lastReadPos + readPos;
	}

	private float[] normalize(float[] samples, float magnitude)
	{
		float[] array = new float[samples.Length];
		for (int i = 0; i < samples.Length; i++)
		{
			array[i] = samples[i] / magnitude;
		}
		return array;
	}

	private float amplitude(float[] x)
	{
		float num = 0f;
		for (int i = 0; i < x.Length; i++)
		{
			num = Mathf.Max(num, Mathf.Abs(x[i]));
		}
		return num;
	}

	private bool CheckVAD(float[] samples)
	{
		if (Time.realtimeSinceStartup < lastVTime + vadHangover)
		{
			return true;
		}
		float num = 0f;
		foreach (float num2 in samples)
		{
			num = Mathf.Max(num, Mathf.Abs(num2));
		}
		bool flag = num >= VolumeThreshold;
		if (flag)
		{
			lastVTime = Time.realtimeSinceStartup;
		}
		return flag;
	}
}
