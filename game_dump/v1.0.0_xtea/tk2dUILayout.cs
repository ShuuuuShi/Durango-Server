using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUILayout")]
public class tk2dUILayout : MonoBehaviour
{
	public Vector3 bMin = new Vector3(0f, -1f, 0f);

	public Vector3 bMax = new Vector3(1f, 0f, 0f);

	public List<tk2dUILayoutItem> layoutItems = new List<tk2dUILayoutItem>();

	public bool autoResizeCollider;

	public int ItemCount => layoutItems.Count;

	public event Action<Vector3, Vector3> OnReshape;

	private void Reset()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Component)this).GetComponent<Collider>() != (Object)null))
		{
			return;
		}
		Collider component = ((Component)this).GetComponent<Collider>();
		BoxCollider val = (BoxCollider)(object)((component is BoxCollider) ? component : null);
		if ((Object)(object)val != (Object)null)
		{
			Bounds bounds = ((Collider)val).bounds;
			Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
			Vector3 position = ((Component)this).transform.position;
			Reshape(((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(((Bounds)(ref bounds)).min) - bMin, ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(((Bounds)(ref bounds)).max) - bMax, updateChildren: true);
			Vector3 val2 = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyVector(((Component)this).transform.position - position);
			Transform transform = ((Component)this).transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				Vector3 localPosition = child.localPosition - val2;
				child.localPosition = localPosition;
			}
			val.center -= val2;
			autoResizeCollider = true;
		}
	}

	public virtual void Reshape(Vector3 dMin, Vector3 dMax, bool updateChildren)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		foreach (tk2dUILayoutItem layoutItem in layoutItems)
		{
			layoutItem.oldPos = layoutItem.gameObj.transform.position;
		}
		bMin += dMin;
		bMax += dMax;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(bMin.x, bMax.y);
		Transform transform = ((Component)this).transform;
		Vector3 position = transform.position;
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		transform.position = position + ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(val);
		bMin -= val;
		bMax -= val;
		if (autoResizeCollider)
		{
			BoxCollider component = ((Component)this).GetComponent<BoxCollider>();
			if ((Object)(object)component != (Object)null)
			{
				component.center += (dMin + dMax) / 2f - val;
				component.size += dMax - dMin;
			}
		}
		foreach (tk2dUILayoutItem layoutItem2 in layoutItems)
		{
			Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
			Vector3 val2 = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyVector(layoutItem2.gameObj.transform.position - layoutItem2.oldPos);
			Vector3 val3 = -val2;
			Vector3 val4 = -val2;
			if (updateChildren)
			{
				val3.x += (layoutItem2.snapLeft ? dMin.x : ((!layoutItem2.snapRight) ? 0f : dMax.x));
				val3.y += (layoutItem2.snapBottom ? dMin.y : ((!layoutItem2.snapTop) ? 0f : dMax.y));
				val4.x += (layoutItem2.snapRight ? dMax.x : ((!layoutItem2.snapLeft) ? 0f : dMin.x));
				val4.y += (layoutItem2.snapTop ? dMax.y : ((!layoutItem2.snapBottom) ? 0f : dMin.y));
			}
			if ((Object)(object)layoutItem2.sprite != (Object)null || (Object)(object)layoutItem2.UIMask != (Object)null || (Object)(object)layoutItem2.layout != (Object)null)
			{
				Matrix4x4 val5 = ((Component)this).transform.localToWorldMatrix * layoutItem2.gameObj.transform.worldToLocalMatrix;
				val3 = ((Matrix4x4)(ref val5)).MultiplyVector(val3);
				val4 = ((Matrix4x4)(ref val5)).MultiplyVector(val4);
			}
			if ((Object)(object)layoutItem2.sprite != (Object)null)
			{
				layoutItem2.sprite.ReshapeBounds(val3, val4);
				continue;
			}
			if ((Object)(object)layoutItem2.UIMask != (Object)null)
			{
				layoutItem2.UIMask.ReshapeBounds(val3, val4);
				continue;
			}
			if ((Object)(object)layoutItem2.layout != (Object)null)
			{
				layoutItem2.layout.Reshape(val3, val4, updateChildren: true);
				continue;
			}
			Vector3 val6 = val3;
			if (layoutItem2.snapLeft && layoutItem2.snapRight)
			{
				val6.x = 0.5f * (val3.x + val4.x);
			}
			if (layoutItem2.snapTop && layoutItem2.snapBottom)
			{
				val6.y = 0.5f * (val3.y + val4.y);
			}
			Transform transform2 = layoutItem2.gameObj.transform;
			transform2.position += val6;
		}
		if (this.OnReshape != null)
		{
			this.OnReshape(dMin, dMax);
		}
	}

	public void SetBounds(Vector3 pMin, Vector3 pMax)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
		Reshape(((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(pMin) - bMin, ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(pMax) - bMax, updateChildren: true);
	}

	public Vector3 GetMinBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(bMin);
	}

	public Vector3 GetMaxBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(bMax);
	}

	public void Refresh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Reshape(Vector3.zero, Vector3.zero, updateChildren: true);
	}
}
