using UnityEngine;

namespace Durango.UI.InGame;

public abstract class AreaOfEffectAlert : MonoBehaviour
{
	public abstract int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt);

	public abstract int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt);

	public abstract int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt);

	public abstract void Stop(int id, float delay);

	public abstract void Move(int id, Vector3 position);
}
