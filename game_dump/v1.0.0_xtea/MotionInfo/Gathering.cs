namespace MotionInfo;

public class Gathering
{
	public GatheringMotion[] motions;

	public string defaultMotion;

	public GatheringMotion this[int index] => motions[index];

	public int Count => (motions != null) ? motions.Length : 0;
}
