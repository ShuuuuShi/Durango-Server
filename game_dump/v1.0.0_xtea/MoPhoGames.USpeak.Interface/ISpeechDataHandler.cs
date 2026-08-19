namespace MoPhoGames.USpeak.Interface;

public interface ISpeechDataHandler
{
	void USpeakOnSerializeAudio(byte[] data);

	void USpeakInitializeSettings(int data);
}
