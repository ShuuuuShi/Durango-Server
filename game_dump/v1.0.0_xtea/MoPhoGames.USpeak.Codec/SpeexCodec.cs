using System;
using MoPhoGames.USpeak.Core.Utils;
using NSpeex;

namespace MoPhoGames.USpeak.Codec;

public class SpeexCodec : ICodec
{
	private SpeexDecoder m_ultrawide_dec = new SpeexDecoder((BandMode)2, true);

	private SpeexEncoder m_ultrawide_enc = new SpeexEncoder((BandMode)2);

	private SpeexDecoder m_wide_dec = new SpeexDecoder((BandMode)1, true);

	private SpeexEncoder m_wide_enc = new SpeexEncoder((BandMode)1);

	private SpeexDecoder m_narrow_dec = new SpeexDecoder((BandMode)0, true);

	private SpeexEncoder m_narrow_enc = new SpeexEncoder((BandMode)0);

	public SpeexCodec()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		m_wide_enc.Quality = 5;
		m_narrow_enc.Quality = 5;
		m_ultrawide_enc.Quality = 5;
	}

	private byte[] SpeexEncode(short[] input, BandMode mode)
	{
		SpeexEncoder val = null;
		int num = 320;
		switch (mode)
		{
		case BandMode.Narrow:
			val = m_narrow_enc;
			num = 320;
			break;
		case BandMode.Wide:
			val = m_wide_enc;
			num = 640;
			break;
		case BandMode.UltraWide:
			val = m_ultrawide_enc;
			num = 1280;
			break;
		}
		byte[] @byte = USpeakPoolUtils.GetByte(num + 4);
		int value = val.Encode(input, 0, input.Length, @byte, 4, @byte.Length);
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, @byte, 4);
		return @byte;
	}

	private short[] SpeexDecode(byte[] input, BandMode mode)
	{
		SpeexDecoder val = null;
		int length = 320;
		switch (mode)
		{
		case BandMode.Narrow:
			val = m_narrow_dec;
			length = 320;
			break;
		case BandMode.Wide:
			val = m_wide_dec;
			length = 640;
			break;
		case BandMode.UltraWide:
			val = m_ultrawide_dec;
			length = 1280;
			break;
		}
		byte[] @byte = USpeakPoolUtils.GetByte(4);
		Array.Copy(input, @byte, 4);
		int num = BitConverter.ToInt32(@byte, 0);
		USpeakPoolUtils.Return(@byte);
		byte[] byte2 = USpeakPoolUtils.GetByte(input.Length - 4);
		Buffer.BlockCopy(input, 4, byte2, 0, input.Length - 4);
		short[] @short = USpeakPoolUtils.GetShort(length);
		val.Decode(byte2, 0, num, @short, 0, false);
		USpeakPoolUtils.Return(byte2);
		return @short;
	}

	public byte[] Encode(short[] data, BandMode mode)
	{
		return SpeexEncode(data, mode);
	}

	public short[] Decode(byte[] data, BandMode mode)
	{
		return SpeexDecode(data, mode);
	}

	public int GetSampleSize(int recordingFrequency)
	{
		return recordingFrequency switch
		{
			8000 => 320, 
			16000 => 640, 
			32000 => 1280, 
			_ => 320, 
		};
	}
}
