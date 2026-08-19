using UnityEngine;

namespace Durango.Utils;

public static class Collisions
{
	private static RaycastHit[] _raycastHits;

	private static Collider[] _colliders;

	static Collisions()
	{
		_raycastHits = new RaycastHit[256];
		_colliders = new Collider[256];
		GameManager.Reset += delegate
		{
			for (int i = 0; i < _raycastHits.Length; i++)
			{
				_raycastHits[i] = default(RaycastHit);
			}
			for (int j = 0; j < _colliders.Length; j++)
			{
				_colliders[j] = null;
			}
		};
	}

	public static CollisionParam CreateCollisionParam(Vector3 beginPos, Vector3 delta)
	{
		CollisionParam result = default(CollisionParam);
		result.Radius = 20f;
		result.CapBegin = beginPos;
		result.CapEnd = result.CapBegin;
		result.CapEnd.y += 180f;
		float num = ((!(result.CapBegin.y < 0f)) ? 0f : (0f - result.CapBegin.y));
		result.CapBegin.y += num + (result.Radius + 0.001f);
		result.CapEnd.y += num - (result.Radius + 0.001f);
		result.Direction = delta.normalized;
		result.Distance = delta.magnitude;
		result.Mask = LayerHelper.PropMask;
		return result;
	}

	public static Vector3 ProcessSimpleSliding(CollisionParam param)
	{
		for (int i = 0; i < 4; i++)
		{
			if (CheckCollision(param, collideOnOverlapped: false, out var raycastHit))
			{
				Vector3 normal = raycastHit.normal;
				normal.y = 0f;
				normal.Normalize();
				Vector3 vector = param.Direction * param.Distance;
				Vector3 vector2 = Vector3.Reflect(normal, normal);
				Vector3 rhs = Quaternion.Euler(0f, 90f, 0f) * vector2;
				float num = Vector3.Dot(param.Direction, rhs);
				float y = ((!(num > 0f)) ? (-45f) : 45f);
				vector = Quaternion.Euler(0f, y, 0f) * vector;
				vector -= normal * Vector3.Dot(normal, vector);
				param.Direction = Vector3.Normalize(vector);
				param.Distance = vector.magnitude * 0.85f;
				continue;
			}
			return param.Direction * param.Distance;
		}
		return Vector3.zero;
	}

	public static RaycastHit[] RayCast(Ray ray, float dist, int mask, out int count)
	{
		count = Physics.RaycastNonAlloc(ray, _raycastHits, dist, mask);
		if (count >= _colliders.Length)
		{
			_raycastHits = new RaycastHit[count * 2];
			count = Physics.RaycastNonAlloc(ray, _raycastHits, dist, mask);
		}
		return _raycastHits;
	}

	public static Collider[] OverlapSphere(Vector3 pos, float radius, int mask, out int count)
	{
		count = Physics.OverlapSphereNonAlloc(pos, radius, _colliders, mask);
		if (count >= _colliders.Length)
		{
			_colliders = new Collider[count * 2];
			count = Physics.OverlapSphereNonAlloc(pos, radius, _colliders, mask);
		}
		return _colliders;
	}

	public static bool CheckCollision(CollisionParam param, bool collideOnOverlapped, out RaycastHit raycastHit)
	{
		switch (TryCapsuleCast(param, out raycastHit))
		{
		case RayCastResult.Blocked:
			return true;
		case RayCastResult.Overlapped:
			return collideOnOverlapped;
		default:
		{
			Vector3 vector = param.Direction * param.Distance;
			param.CapBegin += vector;
			param.CapEnd += vector;
			param.Distance = 0f;
			return TryCapsuleCast(param, out raycastHit) != RayCastResult.Pass;
		}
		}
	}

	public static RayCastResult TryCapsuleCast(CollisionParam param, out RaycastHit raycastHit)
	{
		if (param.Direction == Vector3.zero)
		{
			param.Direction = Vector3.forward;
		}
		int num;
		while (true)
		{
			num = Physics.CapsuleCastNonAlloc(param.CapBegin, param.CapEnd, param.Radius, param.Direction, _raycastHits, param.Distance, param.Mask, QueryTriggerInteraction.UseGlobal);
			if (num == _raycastHits.Length)
			{
				_raycastHits = new RaycastHit[num * 2];
				continue;
			}
			break;
		}
		int nearestHit = GetNearestHit(_raycastHits, num);
		if (nearestHit == -1)
		{
			raycastHit = default(RaycastHit);
			return RayCastResult.Pass;
		}
		raycastHit = _raycastHits[nearestHit];
		return (!(raycastHit.distance <= 0f)) ? RayCastResult.Blocked : RayCastResult.Overlapped;
	}

	private static int GetNearestHit(RaycastHit[] hits, int count)
	{
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = hits[i];
			if (!raycastHit.collider.isTrigger && (num < 0 || !(hits[num].distance <= raycastHit.distance)))
			{
				num = i;
			}
		}
		return num;
	}

	public static bool RayCastContextAction(Ray ray, int mask, string tagname, out GameObject pickingObject)
	{
		int count;
		RaycastHit[] hits = RayCast(ray, float.PositiveInfinity, mask, out count);
		Transform transformOfNearestHit = GetTransformOfNearestHit(hits, count, tagname);
		if (transformOfNearestHit != null)
		{
			pickingObject = transformOfNearestHit.gameObject;
			return true;
		}
		pickingObject = null;
		return false;
	}

	private static Transform GetTransformOfNearestHit(RaycastHit[] hits, int count, string tagname)
	{
		RaycastHit raycastHit = default(RaycastHit);
		Transform transform = null;
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit2 = hits[i];
			Transform transform2 = ((!(raycastHit2.collider == null)) ? raycastHit2.collider.transform : raycastHit2.transform);
			if (transform != null && raycastHit.distance <= raycastHit2.distance)
			{
				continue;
			}
			while (transform2 != null)
			{
				if (tagname == null || transform2.gameObject.CompareTag(tagname))
				{
					raycastHit = raycastHit2;
					transform = transform2;
					break;
				}
				transform2 = transform2.parent;
			}
		}
		return transform;
	}
}
