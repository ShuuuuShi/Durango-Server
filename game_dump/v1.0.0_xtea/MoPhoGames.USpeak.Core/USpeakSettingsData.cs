namespace MoPhoGames.USpeak.Core;

public class USpeakSettingsData
{
	public BandMode bandMode;

	public int Codec;

	public USpeakSettingsData()
	{
		bandMode = BandMode.Narrow;
		Codec = 0;
	}

	public USpeakSettingsData(byte src)
	{
		if ((src & 1) == 1)
		{
			bandMode = BandMode.Narrow;
		}
		else if ((src & 2) == 2)
		{
			bandMode = BandMode.Wide;
		}
		else
		{
			bandMode = BandMode.UltraWide;
		}
		Codec = src >> 2;
	}

	public byte ToByte()
	{
		byte b = 0;
		if (bandMode == BandMode.Narrow)
		{
			b = (byte)(b | 1u);
		}
		else if (bandMode == BandMode.Wide)
		{
			b = (byte)(b | 2u);
		}
		return (byte)(b | (byte)(Codec << 2));
	}
}
