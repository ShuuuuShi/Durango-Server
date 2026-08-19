using System;
using MoPhoGames.USpeak.Core.Utils;

namespace MoPhoGames.USpeak.Codec;

[Serializable]
public class MuLawCodec : ICodec
{
	private class MuLawEncoder
	{
		public const int BIAS = 132;

		public const int MAX = 32635;

		private static byte[] pcmToMuLawMap;

		public static bool ZeroTrap
		{
			get
			{
				return pcmToMuLawMap[33000] != 0;
			}
			set
			{
				byte b = (byte)(value ? 2u : 0u);
				for (int i = 32768; i <= 33924; i++)
				{
					pcmToMuLawMap[i] = b;
				}
			}
		}

		static MuLawEncoder()
		{
			pcmToMuLawMap = new byte[65536];
			for (int i = -32768; i <= 32767; i++)
			{
				pcmToMuLawMap[i & 0xFFFF] = encode(i);
			}
		}

		public static byte MuLawEncode(int pcm)
		{
			return pcmToMuLawMap[pcm & 0xFFFF];
		}

		public static byte MuLawEncode(short pcm)
		{
			return pcmToMuLawMap[pcm & 0xFFFF];
		}

		public static byte[] MuLawEncode(int[] pcm)
		{
			int num = pcm.Length;
			byte[] @byte = USpeakPoolUtils.GetByte(num);
			for (int i = 0; i < num; i++)
			{
				@byte[i] = MuLawEncode(pcm[i]);
			}
			return @byte;
		}

		public static byte[] MuLawEncode(short[] pcm)
		{
			int num = pcm.Length;
			byte[] @byte = USpeakPoolUtils.GetByte(num);
			for (int i = 0; i < num; i++)
			{
				@byte[i] = MuLawEncode(pcm[i]);
			}
			return @byte;
		}

		private static byte encode(int pcm)
		{
			int num = (pcm & 0x8000) >> 8;
			if (num != 0)
			{
				pcm = -pcm;
			}
			if (pcm > 32635)
			{
				pcm = 32635;
			}
			pcm += 132;
			int num2 = 7;
			int num3 = 16384;
			while ((pcm & num3) == 0)
			{
				num2--;
				num3 >>= 1;
			}
			int num4 = (pcm >> num2 + 3) & 0xF;
			byte b = (byte)(num | (num2 << 4) | num4);
			return (byte)(~b);
		}
	}

	private class MuLawDecoder
	{
		private static readonly short[] muLawToPcmMap;

		static MuLawDecoder()
		{
			muLawToPcmMap = new short[256];
			for (byte b = 0; b < byte.MaxValue; b++)
			{
				muLawToPcmMap[b] = Decode(b);
			}
		}

		public static short[] MuLawDecode(byte[] data)
		{
			int num = data.Length;
			short[] @short = USpeakPoolUtils.GetShort(num);
			for (int i = 0; i < num; i++)
			{
				@short[i] = muLawToPcmMap[data[i]];
			}
			return @short;
		}

		private static short Decode(byte mulaw)
		{
			mulaw = (byte)(~mulaw);
			int num = mulaw & 0x80;
			int num2 = (mulaw & 0x70) >> 4;
			int num3 = mulaw & 0xF;
			num3 |= 0x10;
			num3 <<= 1;
			num3++;
			num3 <<= num2 + 2;
			num3 -= 132;
			return (short)((num != 0) ? (-num3) : num3);
		}
	}

	public byte[] Encode(short[] data, BandMode mode)
	{
		return MuLawEncoder.MuLawEncode(data);
	}

	public short[] Decode(byte[] data, BandMode mode)
	{
		return MuLawDecoder.MuLawDecode(data);
	}

	public int GetSampleSize(int recordingFrequency)
	{
		return 0;
	}
}
