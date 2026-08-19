using System;

namespace MoPhoGames.USpeak.Core;

public struct USpeakFrameContainer
{
	public ushort Samples;

	public byte[] encodedData;

	public void LoadFrom(byte[] source)
	{
		int num = BitConverter.ToInt32(source, 0);
		Samples = BitConverter.ToUInt16(source, 4);
		encodedData = new byte[num];
		Array.Copy(source, 6, encodedData, 0, num);
	}

	public byte[] ToByteArray()
	{
		byte[] array = new byte[6 + encodedData.Length];
		byte[] bytes = BitConverter.GetBytes(encodedData.Length);
		bytes.CopyTo(array, 0);
		byte[] bytes2 = BitConverter.GetBytes(Samples);
		Array.Copy(bytes2, 0, array, 4, 2);
		for (int i = 0; i < encodedData.Length; i++)
		{
			array[i + 6] = encodedData[i];
		}
		return array;
	}
}
