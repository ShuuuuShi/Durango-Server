namespace Durango.UI;

public class BrushDrawer : DrawerBase
{
	protected override Point2 ChangePos(int x, int y, int kernel)
	{
		kernel = ((kernel >= 2) ? kernel : 2);
		return new Point2(x / kernel * kernel + 1, y / kernel * kernel + 1);
	}
}
