namespace Durango.MotionInfo;

public class Gathering
{
	public GatheringMotion[] motions;

	public string defaultMotion;

	public GatheringMotion this[int index] => motions[index];

	public int Count
	{
		get
		{
			if (motions == null)
			{
				return 0;
			}
			return motions.Length;
		}
	}
}
