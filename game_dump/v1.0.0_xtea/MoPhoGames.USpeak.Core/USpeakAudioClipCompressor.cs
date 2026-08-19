using System.Collections.Generic;
using MoPhoGames.USpeak.Codec;
using MoPhoGames.USpeak.Core.Utils;
using UnityEngine;

namespace MoPhoGames.USpeak.Core;

public class USpeakAudioClipCompressor : MonoBehaviour
{
	private static List<byte> data = new List<byte>();

	private static List<short> tmp = new List<short>();

	public static byte[] CompressAudioData(float[] samples, int channels, out int sample_count, BandMode mode, ICodec Codec, float gain = 1f)
	{
		data.Clear();
		sample_count = 0;
		short[] d = USpeakAudioClipConverter.AudioDataToShorts(samples, channels, gain);
		byte[] array = Codec.Encode(d, mode);
		USpeakPoolUtils.Return(d);
		data.AddRange(array);
		USpeakPoolUtils.Return(array);
		return data.ToArray();
	}

	public static float[] DecompressAudio(byte[] data, int samples, int channels, bool threeD, BandMode mode, ICodec Codec, float gain)
	{
		int frequency = 4000;
		switch (mode)
		{
		case BandMode.Narrow:
			frequency = 8000;
			break;
		case BandMode.Wide:
			frequency = 16000;
			break;
		}
		short[] array = Codec.Decode(data, mode);
		tmp.Clear();
		tmp.AddRange(array);
		USpeakPoolUtils.Return(array);
		return USpeakAudioClipConverter.ShortsToAudioData(tmp.ToArray(), channels, frequency, threeD, gain);
	}
}
