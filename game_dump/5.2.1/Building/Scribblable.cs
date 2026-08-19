namespace Building;

public class Scribblable
{
	public bool Text;

	public Point2 CanvasSize;

	public int LimitFrame;

	public bool Canvas
	{
		get
		{
			if (CanvasSize.x > 0)
			{
				return CanvasSize.y > 0;
			}
			return false;
		}
	}
}
