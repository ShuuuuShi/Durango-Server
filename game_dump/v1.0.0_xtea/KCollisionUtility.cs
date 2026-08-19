using KCollisionData;
using UnityEngine;

public static class KCollisionUtility
{
	private static readonly string[] BlockTagNames = new string[2] { "Blockable", "Artifact" };

	private static RaycastHit[] _raycastHits = (RaycastHit[])(object)new RaycastHit[16];

	private static int _prevHitCount;

	private static Collider[] _colliders = (Collider[])(object)new Collider[64];

	private static int _prevColliderCount;

	public static void Reset()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _raycastHits.Length; i++)
		{
			_raycastHits[i] = default(RaycastHit);
		}
		for (int j = 0; j < _colliders.Length; j++)
		{
			_colliders[j] = null;
		}
	}

	public static CollisionParam CreateCollisionParam(Vector3 beginPos, Vector3 delta)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		CollisionParam result = default(CollisionParam);
		result.Radius = 20f;
		result.CapBegin = beginPos;
		result.CapEnd = result.CapBegin;
		ref Vector3 capEnd = ref result.CapEnd;
		capEnd.y += 180f;
		float num = ((!(result.CapBegin.y < 0f)) ? 0f : (0f - result.CapBegin.y));
		ref Vector3 capBegin = ref result.CapBegin;
		capBegin.y += num + (result.Radius + 0.001f);
		ref Vector3 capEnd2 = ref result.CapEnd;
		capEnd2.y += num - (result.Radius + 0.001f);
		result.Direction = ((Vector3)(ref delta)).normalized;
		result.Distance = ((Vector3)(ref delta)).magnitude;
		result.Mask = LayerMask.op_Implicit(LayerHelper.PropMask);
		return result;
	}

	public static Vector3 ProcessSimpleSliding(CollisionParam param)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			if (CheckCollision(param, collideOnOverlapped: false, out var normal))
			{
				Vector3 val = param.Direction * param.Distance;
				val -= normal * Vector3.Dot(normal, val);
				param.Direction = Vector3.Normalize(val);
				param.Distance = ((Vector3)(ref val)).magnitude * 0.85f;
				continue;
			}
			return param.Direction * param.Distance;
		}
		return Vector3.zero;
	}

	public static RaycastHit[] RayCast(Ray ray, float dist, int mask, out int count)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (_prevHitCount >= _raycastHits.Length)
		{
			_raycastHits = (RaycastHit[])(object)new RaycastHit[_prevHitCount * 2];
		}
		count = Physics.RaycastNonAlloc(ray, _raycastHits, dist, mask);
		_prevHitCount = count;
		return _raycastHits;
	}

	public static Collider[] OverlapSphere(Vector3 pos, float radius, int mask, out int count)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (_prevColliderCount >= _colliders.Length)
		{
			_colliders = (Collider[])(object)new Collider[_prevColliderCount * 2];
		}
		count = Physics.OverlapSphereNonAlloc(pos, radius, _colliders, mask);
		_prevColliderCount = count;
		return _colliders;
	}

	public static bool CheckCollision(CollisionParam param, bool collideOnOverlapped, out Vector3 normal)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		switch (TryCapsuleCast(param, out normal))
		{
		case RayCastResult.Blocked:
			return true;
		case RayCastResult.Overlapped:
			return collideOnOverlapped;
		default:
		{
			Vector3 val = param.Direction * param.Distance;
			param.CapBegin += val;
			param.CapEnd += val;
			param.Distance = 0f;
			return TryCapsuleCast(param, out normal) != RayCastResult.Pass;
		}
		}
	}

	public static RayCastResult TryCapsuleCast(CollisionParam param, out Vector3 normal)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (param.Direction == Vector3.zero)
		{
			param.Direction = Vector3.forward;
		}
		int num;
		while (true)
		{
			num = Physics.CapsuleCastNonAlloc(param.CapBegin, param.CapEnd, param.Radius, param.Direction, _raycastHits, param.Distance, param.Mask);
			if (num == _raycastHits.Length)
			{
				_raycastHits = (RaycastHit[])(object)new RaycastHit[num * 2];
				continue;
			}
			break;
		}
		int nearestHit = GetNearestHit(_raycastHits, num, BlockTagNames);
		if (nearestHit == -1)
		{
			normal = Vector3.zero;
			return RayCastResult.Pass;
		}
		RaycastHit val = _raycastHits[nearestHit];
		normal = ((RaycastHit)(ref val)).normal;
		normal.y = 0f;
		((Vector3)(ref normal)).Normalize();
		return (!(((RaycastHit)(ref val)).distance <= 0f)) ? RayCastResult.Blocked : RayCastResult.Overlapped;
	}

	public static int GetNearestHit(RaycastHit[] hits, int count, string[] tagnames)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			RaycastHit val = hits[i];
			if (((RaycastHit)(ref val)).collider.isTrigger || (num >= 0 && ((RaycastHit)(ref hits[num])).distance <= ((RaycastHit)(ref val)).distance))
			{
				continue;
			}
			for (int j = 0; j < tagnames.Length; j++)
			{
				if (tagnames[j] == null || ((Component)((RaycastHit)(ref val)).collider).gameObject.CompareTag(tagnames[j]))
				{
					num = i;
					break;
				}
			}
		}
		return num;
	}
}
