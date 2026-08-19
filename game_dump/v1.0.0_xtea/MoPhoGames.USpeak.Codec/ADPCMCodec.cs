using System;
using MoPhoGames.USpeak.Core.Utils;

namespace MoPhoGames.USpeak.Codec;

[Serializable]
internal class ADPCMCodec : ICodec
{
	private static int[] indexTable = new int[16]
	{
		-1, -1, -1, -1, 2, 4, 6, 8, -1, -1,
		-1, -1, 2, 4, 6, 8
	};

	private static int[] stepsizeTable = new int[88]
	{
		7, 8, 9, 10, 11, 12, 14, 16, 17, 19,
		21, 23, 25, 28, 31, 34, 37, 41, 45, 50,
		55, 60, 66, 73, 80, 88, 97, 107, 118, 130,
		143, 157, 173, 190, 209, 230, 253, 279, 307, 337,
		371, 408, 449, 494, 544, 598, 658, 724, 796, 876,
		963, 1060, 1166, 1282, 1411, 1522, 1707, 1876, 2066, 2272,
		2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894,
		6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289,
		16818, 18500, 203500, 22385, 24623, 27086, 29794, 32767
	};

	private int predictedSample;

	private int stepsize = 7;

	private int index;

	private int newSample;

	private void Init()
	{
		predictedSample = 0;
		stepsize = 7;
		index = 0;
		newSample = 0;
	}

	private short ADPCM_Decode(byte originalSample)
	{
		int num = 0;
		num = stepsize * originalSample / 4 + stepsize / 8;
		if ((originalSample & 4) == 4)
		{
			num += stepsize;
		}
		if ((originalSample & 2) == 2)
		{
			num += stepsize >> 1;
		}
		if ((originalSample & 1) == 1)
		{
			num += stepsize >> 2;
		}
		num += stepsize >> 3;
		if ((originalSample & 8) == 8)
		{
			num = -num;
		}
		newSample = num;
		if (newSample > 32767)
		{
			newSample = 32767;
		}
		else if (newSample < -32768)
		{
			newSample = -32768;
		}
		index += indexTable[originalSample];
		if (index < 0)
		{
			index = 0;
		}
		if (index > 88)
		{
			index = 88;
		}
		stepsize = stepsizeTable[index];
		return (short)newSample;
	}

	private byte ADPCM_Encode(short originalSample)
	{
		int num = originalSample - predictedSample;
		if (num >= 0)
		{
			newSample = 0;
		}
		else
		{
			newSample = 8;
			num = -num;
		}
		byte b = 4;
		int num2 = stepsize;
		for (int i = 0; i < 3; i++)
		{
			if (num >= num2)
			{
				newSample |= b;
				num -= num2;
			}
			num2 >>= 1;
			b >>= 1;
		}
		num = stepsize >> 3;
		if (((uint)newSample & 4u) != 0)
		{
			num += stepsize;
		}
		if (((uint)newSample & 2u) != 0)
		{
			num += stepsize >> 1;
		}
		if (((uint)newSample & (true ? 1u : 0u)) != 0)
		{
			num += stepsize >> 2;
		}
		if (((uint)newSample & 8u) != 0)
		{
			num = -num;
		}
		predictedSample += num;
		if (predictedSample > 32767)
		{
			predictedSample = 32767;
		}
		if (predictedSample < -32768)
		{
			predictedSample = -32768;
		}
		index += indexTable[newSample];
		if (index < 0)
		{
			index = 0;
		}
		else if (index > 88)
		{
			index = 88;
		}
		stepsize = stepsizeTable[index];
		return (byte)newSample;
	}

	public byte[] Encode(short[] data, BandMode mode)
	{
		Init();
		int num = data.Length / 2;
		if (num % 2 != 0)
		{
			num++;
		}
		byte[] @byte = USpeakPoolUtils.GetByte(num);
		for (int i = 0; i < @byte.Length && i * 2 < data.Length; i++)
		{
			byte b = ADPCM_Encode(data[i * 2]);
			byte b2 = 0;
			if (i * 2 + 1 < data.Length)
			{
				b2 = ADPCM_Encode(data[i * 2 + 1]);
			}
			byte b3 = (byte)((b2 << 4) | b);
			@byte[i] = b3;
		}
		return @byte;
	}

	public short[] Decode(byte[] data, BandMode mode)
	{
		Init();
		short[] @short = USpeakPoolUtils.GetShort(data.Length * 2);
		for (int i = 0; i < data.Length; i++)
		{
			byte b = data[i];
			byte originalSample = (byte)(b & 0xFu);
			byte originalSample2 = (byte)(b >> 4);
			@short[i * 2] = ADPCM_Decode(originalSample);
			@short[i * 2 + 1] = ADPCM_Decode(originalSample2);
		}
		return @short;
	}

	public int GetSampleSize(int recordingFrequency)
	{
		return 0;
	}
}
