namespace Durango.Render.Camera;

public struct Item<T>
{
	public T Value;

	public float Duration;

	public NgInterpolate.Function Ease;
}
