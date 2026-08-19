public interface IClipEnumerator
{
	bool TryMoveNext(int index, out AnimationSequenceClip clip);
}
