public class FellingTreeController : KSingleton<FellingTreeController>
{
	public float fallenAngle = 45f;

	public float bouncing1Angle = 5f;

	public float bouncing2Angle = -5f;

	public float curveFactor = 3f;

	public float bouncing1Time = 2f;

	public float bouncing2Time = 2.1f;

	public float bouncing3Time = 2.2f;

	public float fadingOutTime = 4f;

	public string treeFellingSound = "Sound/Effect/tree_felling.wav";

	public string treeBouncingSound = "Sound/Effect/tree_bouncing.wav";
}
