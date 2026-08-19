namespace Building;

public class Scribblable
{
	public bool Text;

	public Point2 CanvasSize;

	public int LimitFrame;

	public bool Canvas => CanvasSize.x > 0 && CanvasSize.y > 0;
}
