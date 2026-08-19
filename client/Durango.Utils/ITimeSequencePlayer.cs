namespace Durango.Utils;

public interface ITimeSequencePlayer
{
	float? NextAt();

	void Play();

	void Stop();

	bool IsPlaying();
}
