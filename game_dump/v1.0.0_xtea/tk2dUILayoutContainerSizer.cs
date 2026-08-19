using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUILayoutContainerSizer")]
public class tk2dUILayoutContainerSizer : tk2dUILayoutContainer
{
	public bool horizontal;

	public bool expand = true;

	public Vector2 margin = Vector2.zero;

	public float spacing;

	protected override void DoChildLayout()
	{
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		int count = layoutItems.Count;
		if (count == 0)
		{
			return;
		}
		float num = bMax.x - bMin.x - 2f * margin.x;
		float num2 = bMax.y - bMin.y - 2f * margin.y;
		float num3 = ((!horizontal) ? num2 : num) - spacing * (float)(count - 1);
		float num4 = 1f;
		float num5 = num3;
		float num6 = 0f;
		float[] array = new float[count];
		for (int i = 0; i < count; i++)
		{
			tk2dUILayoutItem tk2dUILayoutItem2 = layoutItems[i];
			if (tk2dUILayoutItem2.fixedSize)
			{
				array[i] = ((!horizontal) ? (tk2dUILayoutItem2.layout.bMax.y - tk2dUILayoutItem2.layout.bMin.y) : (tk2dUILayoutItem2.layout.bMax.x - tk2dUILayoutItem2.layout.bMin.x));
				num5 -= array[i];
			}
			else if (tk2dUILayoutItem2.fillPercentage > 0f)
			{
				float num7 = num4 * tk2dUILayoutItem2.fillPercentage / 100f;
				array[i] = num3 * num7;
				num5 -= array[i];
				num4 -= num7;
			}
			else
			{
				num6 += tk2dUILayoutItem2.sizeProportion;
			}
		}
		for (int j = 0; j < count; j++)
		{
			tk2dUILayoutItem tk2dUILayoutItem3 = layoutItems[j];
			if (!tk2dUILayoutItem3.fixedSize && tk2dUILayoutItem3.fillPercentage <= 0f)
			{
				array[j] = num5 * tk2dUILayoutItem3.sizeProportion / num6;
			}
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float num8 = 0f;
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		if (horizontal)
		{
			innerSize = new Vector2(2f * margin.x + spacing * (float)(count - 1), bMax.y - bMin.y);
		}
		else
		{
			innerSize = new Vector2(bMax.x - bMin.x, 2f * margin.y + spacing * (float)(count - 1));
		}
		for (int k = 0; k < count; k++)
		{
			tk2dUILayoutItem tk2dUILayoutItem4 = layoutItems[k];
			Matrix4x4 val = tk2dUILayoutItem4.gameObj.transform.localToWorldMatrix * ((Component)this).transform.worldToLocalMatrix;
			if (horizontal)
			{
				if (expand)
				{
					zero.y = bMin.y + margin.y;
					zero2.y = bMax.y - margin.y;
				}
				else
				{
					zero.y = ((Matrix4x4)(ref val)).MultiplyPoint(tk2dUILayoutItem4.layout.bMin).y;
					zero2.y = ((Matrix4x4)(ref val)).MultiplyPoint(tk2dUILayoutItem4.layout.bMax).y;
				}
				zero.x = bMin.x + margin.x + num8;
				zero2.x = zero.x + array[k];
			}
			else
			{
				if (expand)
				{
					zero.x = bMin.x + margin.x;
					zero2.x = bMax.x - margin.x;
				}
				else
				{
					zero.x = ((Matrix4x4)(ref val)).MultiplyPoint(tk2dUILayoutItem4.layout.bMin).x;
					zero2.x = ((Matrix4x4)(ref val)).MultiplyPoint(tk2dUILayoutItem4.layout.bMax).x;
				}
				zero2.y = bMax.y - margin.y - num8;
				zero.y = zero2.y - array[k];
			}
			tk2dUILayoutItem4.layout.SetBounds(((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(zero), ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(zero2));
			num8 += array[k] + spacing;
			if (horizontal)
			{
				ref Vector2 reference = ref innerSize;
				reference.x += array[k];
			}
			else
			{
				ref Vector2 reference2 = ref innerSize;
				reference2.y += array[k];
			}
		}
	}
}
