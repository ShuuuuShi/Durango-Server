namespace MoPhoGames.USpeak.Codec;

public interface ICodec
{
	byte[] Encode(short[] data, BandMode bandMode);

	short[] Decode(byte[] data, BandMode bandMode);

	int GetSampleSize(int recordingFrequency);
}
