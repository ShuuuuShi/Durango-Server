using UnityEngine;

public static class tk2dSpriteGeomGen
{
	private static readonly int[] boxIndicesBack = new int[36]
	{
		0, 1, 2, 2, 1, 3, 6, 5, 4, 7,
		5, 6, 3, 7, 6, 2, 3, 6, 4, 5,
		1, 4, 1, 0, 6, 4, 0, 6, 0, 2,
		1, 7, 3, 5, 7, 1
	};

	private static readonly int[] boxIndicesFwd = new int[36]
	{
		2, 1, 0, 3, 1, 2, 4, 5, 6, 6,
		5, 7, 6, 7, 3, 6, 3, 2, 1, 5,
		4, 0, 1, 4, 0, 4, 6, 2, 0, 6,
		3, 7, 1, 1, 7, 5
	};

	private static readonly Vector3[] boxUnitVertices = (Vector3[])(object)new Vector3[8]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, 1f, 1f)
	};

	private static Matrix4x4 boxScaleMatrix = Matrix4x4.identity;

	public static void SetSpriteColors(Color32[] dest, int offset, int numVertices, Color c, bool premulAlpha)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (premulAlpha)
		{
			c.r *= c.a;
			c.g *= c.a;
			c.b *= c.a;
		}
		Color32 val = Color32.op_Implicit(c);
		for (int i = 0; i < numVertices; i++)
		{
			dest[offset + i] = val;
		}
	}

	public static Vector2 GetAnchorOffset(tk2dBaseSprite.Anchor anchor, float width, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			zero.x = (int)(width / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.x = (int)width;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			zero.y = (int)(height / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerLeft:
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.LowerRight:
			zero.y = (int)height;
			break;
		}
		return zero;
	}

	public static void GetSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		numVertices = spriteDef.positions.Length;
		numIndices = spriteDef.indices.Length;
	}

	public static void SetSpriteGeom(Vector3[] pos, Vector2[] uv, Vector3[] norm, Vector4[] tang, int offset, tk2dSpriteDefinition spriteDef, Vector3 scale)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < spriteDef.positions.Length; i++)
		{
			ref Vector3 reference = ref pos[offset + i];
			reference = Vector3.Scale(spriteDef.positions[i], scale);
		}
		for (int j = 0; j < spriteDef.uvs.Length; j++)
		{
			ref Vector2 reference2 = ref uv[offset + j];
			reference2 = spriteDef.uvs[j];
		}
		if (norm != null && spriteDef.normals != null)
		{
			for (int k = 0; k < spriteDef.normals.Length; k++)
			{
				ref Vector3 reference3 = ref norm[offset + k];
				reference3 = spriteDef.normals[k];
			}
		}
		if (tang != null && spriteDef.tangents != null)
		{
			for (int l = 0; l < spriteDef.tangents.Length; l++)
			{
				ref Vector4 reference4 = ref tang[offset + l];
				reference4 = spriteDef.tangents[l];
			}
		}
	}

	public static void SetSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		for (int i = 0; i < spriteDef.indices.Length; i++)
		{
			indices[offset + i] = vStart + spriteDef.indices[i];
		}
	}

	public static void GetClippedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		if (spriteDef.positions.Length == 4)
		{
			numVertices = 4;
			numIndices = 6;
		}
		else
		{
			numVertices = 0;
			numIndices = 0;
		}
	}

	public static void SetClippedSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 clipBottomLeft, Vector2 clipTopRight, float colliderOffsetZ, float colliderExtentZ)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		//IL_0785: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		if (spriteDef.positions.Length == 4)
		{
			Vector3 val = spriteDef.untrimmedBoundsData[0] - spriteDef.untrimmedBoundsData[1] * 0.5f;
			Vector3 val2 = spriteDef.untrimmedBoundsData[0] + spriteDef.untrimmedBoundsData[1] * 0.5f;
			float num = Mathf.Lerp(val.x, val2.x, clipBottomLeft.x);
			float num2 = Mathf.Lerp(val.x, val2.x, clipTopRight.x);
			float num3 = Mathf.Lerp(val.y, val2.y, clipBottomLeft.y);
			float num4 = Mathf.Lerp(val.y, val2.y, clipTopRight.y);
			Vector3 val3 = spriteDef.boundsData[1];
			Vector3 val4 = spriteDef.boundsData[0] - val3 * 0.5f;
			float num5 = (num - val4.x) / val3.x;
			float num6 = (num2 - val4.x) / val3.x;
			float num7 = (num3 - val4.y) / val3.y;
			float num8 = (num4 - val4.y) / val3.y;
			Vector2 val5 = default(Vector2);
			((Vector2)(ref val5))._002Ector(Mathf.Clamp01(num5), Mathf.Clamp01(num7));
			Vector2 val6 = default(Vector2);
			((Vector2)(ref val6))._002Ector(Mathf.Clamp01(num6), Mathf.Clamp01(num8));
			Vector3 val7 = spriteDef.positions[0];
			Vector3 val8 = spriteDef.positions[3];
			Vector3 val9 = default(Vector3);
			((Vector3)(ref val9))._002Ector(Mathf.Lerp(val7.x, val8.x, val5.x) * scale.x, Mathf.Lerp(val7.y, val8.y, val5.y) * scale.y, val7.z * scale.z);
			Vector3 val10 = default(Vector3);
			((Vector3)(ref val10))._002Ector(Mathf.Lerp(val7.x, val8.x, val6.x) * scale.x, Mathf.Lerp(val7.y, val8.y, val6.y) * scale.y, val7.z * scale.z);
			((Vector3)(ref boundsCenter)).Set(val9.x + (val10.x - val9.x) * 0.5f, val9.y + (val10.y - val9.y) * 0.5f, colliderOffsetZ);
			((Vector3)(ref boundsExtents)).Set((val10.x - val9.x) * 0.5f, (val10.y - val9.y) * 0.5f, colliderExtentZ);
			ref Vector3 reference = ref pos[offset];
			reference = new Vector3(val9.x, val9.y, val9.z);
			ref Vector3 reference2 = ref pos[offset + 1];
			reference2 = new Vector3(val10.x, val9.y, val9.z);
			ref Vector3 reference3 = ref pos[offset + 2];
			reference3 = new Vector3(val9.x, val10.y, val9.z);
			ref Vector3 reference4 = ref pos[offset + 3];
			reference4 = new Vector3(val10.x, val10.y, val9.z);
			if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
			{
				Vector2 val11 = default(Vector2);
				((Vector2)(ref val11))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val5.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val5.x));
				Vector2 val12 = default(Vector2);
				((Vector2)(ref val12))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val6.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val6.x));
				ref Vector2 reference5 = ref uv[offset];
				reference5 = new Vector2(val11.x, val11.y);
				ref Vector2 reference6 = ref uv[offset + 1];
				reference6 = new Vector2(val11.x, val12.y);
				ref Vector2 reference7 = ref uv[offset + 2];
				reference7 = new Vector2(val12.x, val11.y);
				ref Vector2 reference8 = ref uv[offset + 3];
				reference8 = new Vector2(val12.x, val12.y);
			}
			else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
			{
				Vector2 val13 = default(Vector2);
				((Vector2)(ref val13))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val5.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val5.x));
				Vector2 val14 = default(Vector2);
				((Vector2)(ref val14))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val6.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val6.x));
				ref Vector2 reference9 = ref uv[offset];
				reference9 = new Vector2(val13.x, val13.y);
				ref Vector2 reference10 = ref uv[offset + 2];
				reference10 = new Vector2(val14.x, val13.y);
				ref Vector2 reference11 = ref uv[offset + 1];
				reference11 = new Vector2(val13.x, val14.y);
				ref Vector2 reference12 = ref uv[offset + 3];
				reference12 = new Vector2(val14.x, val14.y);
			}
			else
			{
				Vector2 val15 = default(Vector2);
				((Vector2)(ref val15))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val5.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val5.y));
				Vector2 val16 = default(Vector2);
				((Vector2)(ref val16))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val6.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val6.y));
				ref Vector2 reference13 = ref uv[offset];
				reference13 = new Vector2(val15.x, val15.y);
				ref Vector2 reference14 = ref uv[offset + 1];
				reference14 = new Vector2(val16.x, val15.y);
				ref Vector2 reference15 = ref uv[offset + 2];
				reference15 = new Vector2(val15.x, val16.y);
				ref Vector2 reference16 = ref uv[offset + 3];
				reference16 = new Vector2(val16.x, val16.y);
			}
		}
	}

	public static void SetClippedSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		if (spriteDef.positions.Length == 4)
		{
			indices[offset] = vStart;
			indices[offset + 1] = vStart + 3;
			indices[offset + 2] = vStart + 1;
			indices[offset + 3] = vStart + 2;
			indices[offset + 4] = vStart + 3;
			indices[offset + 5] = vStart;
		}
	}

	public static void GetSlicedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, bool borderOnly)
	{
		if (spriteDef.positions.Length == 4)
		{
			numVertices = 16;
			numIndices = ((!borderOnly) ? 54 : 48);
		}
		else
		{
			numVertices = 0;
			numIndices = 0;
		}
	}

	public static void SetSlicedSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		if (spriteDef.positions.Length != 4)
		{
			return;
		}
		float x = spriteDef.texelSize.x;
		float y = spriteDef.texelSize.y;
		Vector3[] positions = spriteDef.positions;
		float num = positions[1].x - positions[0].x;
		float num2 = positions[2].y - positions[0].y;
		float num3 = borderTopRight.y * num2;
		float num4 = borderBottomLeft.y * num2;
		float num5 = borderTopRight.x * num;
		float num6 = borderBottomLeft.x * num;
		float num7 = dimensions.x * x;
		float num8 = dimensions.y * y;
		float num9 = 0f;
		float num10 = 0f;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			num9 = -(int)(dimensions.x / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			num9 = -(int)dimensions.x;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			num10 = -(int)(dimensions.y / 2f);
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
		case tk2dBaseSprite.Anchor.UpperCenter:
		case tk2dBaseSprite.Anchor.UpperRight:
			num10 = -(int)dimensions.y;
			break;
		}
		num9 *= x;
		num10 *= y;
		((Vector3)(ref boundsCenter)).Set(scale.x * (num7 * 0.5f + num9), scale.y * (num8 * 0.5f + num10), colliderOffsetZ);
		((Vector3)(ref boundsExtents)).Set(scale.x * (num7 * 0.5f), scale.y * (num8 * 0.5f), colliderExtentZ);
		Vector2[] uvs = spriteDef.uvs;
		Vector2 val = uvs[1] - uvs[0];
		Vector2 val2 = uvs[2] - uvs[0];
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(num9, num10, 0f);
		Vector3[] array = (Vector3[])(object)new Vector3[4]
		{
			val3,
			val3 + new Vector3(0f, num4, 0f),
			val3 + new Vector3(0f, num8 - num3, 0f),
			val3 + new Vector3(0f, num8, 0f)
		};
		Vector2[] array2 = (Vector2[])(object)new Vector2[4]
		{
			uvs[0],
			uvs[0] + val2 * borderBottomLeft.y,
			uvs[0] + val2 * (1f - borderTopRight.y),
			uvs[0] + val2
		};
		for (int i = 0; i < 4; i++)
		{
			ref Vector3 reference = ref pos[offset + i * 4];
			reference = array[i];
			ref Vector3 reference2 = ref pos[offset + i * 4 + 1];
			reference2 = array[i] + new Vector3(num6, 0f, 0f);
			ref Vector3 reference3 = ref pos[offset + i * 4 + 2];
			reference3 = array[i] + new Vector3(num7 - num5, 0f, 0f);
			ref Vector3 reference4 = ref pos[offset + i * 4 + 3];
			reference4 = array[i] + new Vector3(num7, 0f, 0f);
			for (int j = 0; j < 4; j++)
			{
				ref Vector3 reference5 = ref pos[offset + i * 4 + j];
				reference5 = Vector3.Scale(pos[offset + i * 4 + j], scale);
			}
			ref Vector2 reference6 = ref uv[offset + i * 4];
			reference6 = array2[i];
			ref Vector2 reference7 = ref uv[offset + i * 4 + 1];
			reference7 = array2[i] + val * borderBottomLeft.x;
			ref Vector2 reference8 = ref uv[offset + i * 4 + 2];
			reference8 = array2[i] + val * (1f - borderTopRight.x);
			ref Vector2 reference9 = ref uv[offset + i * 4 + 3];
			reference9 = array2[i] + val;
		}
	}

	public static void SetSlicedSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, bool borderOnly)
	{
		if (spriteDef.positions.Length == 4)
		{
			int[] array = new int[54]
			{
				0, 4, 1, 1, 4, 5, 1, 5, 2, 2,
				5, 6, 2, 6, 3, 3, 6, 7, 4, 8,
				5, 5, 8, 9, 6, 10, 7, 7, 10, 11,
				8, 12, 9, 9, 12, 13, 9, 13, 10, 10,
				13, 14, 10, 14, 11, 11, 14, 15, 5, 9,
				6, 6, 9, 10
			};
			int num = array.Length;
			if (borderOnly)
			{
				num -= 6;
			}
			for (int i = 0; i < num; i++)
			{
				indices[offset + i] = vStart + array[i];
			}
		}
	}

	public static void GetTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int num = (int)Mathf.Ceil(dimensions.x * spriteDef.texelSize.x / spriteDef.untrimmedBoundsData[1].x);
		int num2 = (int)Mathf.Ceil(dimensions.y * spriteDef.texelSize.y / spriteDef.untrimmedBoundsData[1].y);
		numVertices = num * num2 * 4;
		numIndices = num * num2 * 6;
	}

	public static void SetTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_067d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_0965: Unknown result type (might be due to invalid IL or missing references)
		//IL_0974: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0980: Unknown result type (might be due to invalid IL or missing references)
		//IL_0985: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09db: Unknown result type (might be due to invalid IL or missing references)
		//IL_09dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_0909: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Unknown result type (might be due to invalid IL or missing references)
		//IL_092d: Unknown result type (might be due to invalid IL or missing references)
		//IL_094c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0951: Unknown result type (might be due to invalid IL or missing references)
		//IL_077f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0784: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		int num = (int)Mathf.Ceil(dimensions.x * spriteDef.texelSize.x / spriteDef.untrimmedBoundsData[1].x);
		int num2 = (int)Mathf.Ceil(dimensions.y * spriteDef.texelSize.y / spriteDef.untrimmedBoundsData[1].y);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(dimensions.x * spriteDef.texelSize.x * scale.x, dimensions.y * spriteDef.texelSize.y * scale.y);
		Vector2 val2 = Vector2.Scale(spriteDef.texelSize, Vector2.op_Implicit(scale)) * 0.1f;
		Vector3 zero = Vector3.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			zero.x = 0f - val.x / 2f;
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.x = 0f - val.x;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			zero.y = 0f - val.y / 2f;
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
		case tk2dBaseSprite.Anchor.UpperCenter:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.y = 0f - val.y;
			break;
		}
		Vector3 val3 = zero;
		zero -= Vector3.Scale(spriteDef.positions[0], scale);
		((Vector3)(ref boundsCenter)).Set(val.x * 0.5f + val3.x, val.y * 0.5f + val3.y, colliderOffsetZ);
		((Vector3)(ref boundsExtents)).Set(val.x * 0.5f, val.y * 0.5f, colliderExtentZ);
		int num3 = 0;
		Vector3 val4 = Vector3.Scale(spriteDef.untrimmedBoundsData[1], scale);
		Vector3 zero2 = Vector3.zero;
		Vector3 val5 = zero2;
		Vector2 val7 = default(Vector2);
		Vector3 val8 = default(Vector3);
		Vector3 val9 = default(Vector3);
		Vector2 val10 = default(Vector2);
		Vector2 val11 = default(Vector2);
		Vector2 val12 = default(Vector2);
		Vector2 val13 = default(Vector2);
		Vector2 val14 = default(Vector2);
		Vector2 val15 = default(Vector2);
		for (int i = 0; i < num2; i++)
		{
			val5.x = zero2.x;
			for (int j = 0; j < num; j++)
			{
				float num4 = 1f;
				float num5 = 1f;
				if (Mathf.Abs(val5.x + val4.x) > Mathf.Abs(val.x) + val2.x)
				{
					num4 = val.x % val4.x / val4.x;
				}
				if (Mathf.Abs(val5.y + val4.y) > Mathf.Abs(val.y) + val2.y)
				{
					num5 = val.y % val4.y / val4.y;
				}
				Vector3 val6 = val5 + zero;
				if (num4 != 1f || num5 != 1f)
				{
					Vector2 zero3 = Vector2.zero;
					((Vector2)(ref val7))._002Ector(num4, num5);
					((Vector3)(ref val8))._002Ector(Mathf.Lerp(spriteDef.positions[0].x, spriteDef.positions[3].x, zero3.x) * scale.x, Mathf.Lerp(spriteDef.positions[0].y, spriteDef.positions[3].y, zero3.y) * scale.y, spriteDef.positions[0].z * scale.z);
					((Vector3)(ref val9))._002Ector(Mathf.Lerp(spriteDef.positions[0].x, spriteDef.positions[3].x, val7.x) * scale.x, Mathf.Lerp(spriteDef.positions[0].y, spriteDef.positions[3].y, val7.y) * scale.y, spriteDef.positions[0].z * scale.z);
					ref Vector3 reference = ref pos[offset + num3];
					reference = val6 + new Vector3(val8.x, val8.y, val8.z);
					ref Vector3 reference2 = ref pos[offset + num3 + 1];
					reference2 = val6 + new Vector3(val9.x, val8.y, val8.z);
					ref Vector3 reference3 = ref pos[offset + num3 + 2];
					reference3 = val6 + new Vector3(val8.x, val9.y, val8.z);
					ref Vector3 reference4 = ref pos[offset + num3 + 3];
					reference4 = val6 + new Vector3(val9.x, val9.y, val8.z);
					if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
					{
						((Vector2)(ref val10))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.x));
						((Vector2)(ref val11))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val7.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val7.x));
						ref Vector2 reference5 = ref uv[offset + num3];
						reference5 = new Vector2(val10.x, val10.y);
						ref Vector2 reference6 = ref uv[offset + num3 + 1];
						reference6 = new Vector2(val10.x, val11.y);
						ref Vector2 reference7 = ref uv[offset + num3 + 2];
						reference7 = new Vector2(val11.x, val10.y);
						ref Vector2 reference8 = ref uv[offset + num3 + 3];
						reference8 = new Vector2(val11.x, val11.y);
					}
					else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
					{
						((Vector2)(ref val12))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.x));
						((Vector2)(ref val13))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val7.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val7.x));
						ref Vector2 reference9 = ref uv[offset + num3];
						reference9 = new Vector2(val12.x, val12.y);
						ref Vector2 reference10 = ref uv[offset + num3 + 2];
						reference10 = new Vector2(val13.x, val12.y);
						ref Vector2 reference11 = ref uv[offset + num3 + 1];
						reference11 = new Vector2(val12.x, val13.y);
						ref Vector2 reference12 = ref uv[offset + num3 + 3];
						reference12 = new Vector2(val13.x, val13.y);
					}
					else
					{
						((Vector2)(ref val14))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.y));
						((Vector2)(ref val15))._002Ector(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, val7.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, val7.y));
						ref Vector2 reference13 = ref uv[offset + num3];
						reference13 = new Vector2(val14.x, val14.y);
						ref Vector2 reference14 = ref uv[offset + num3 + 1];
						reference14 = new Vector2(val15.x, val14.y);
						ref Vector2 reference15 = ref uv[offset + num3 + 2];
						reference15 = new Vector2(val14.x, val15.y);
						ref Vector2 reference16 = ref uv[offset + num3 + 3];
						reference16 = new Vector2(val15.x, val15.y);
					}
				}
				else
				{
					ref Vector3 reference17 = ref pos[offset + num3];
					reference17 = val6 + Vector3.Scale(spriteDef.positions[0], scale);
					ref Vector3 reference18 = ref pos[offset + num3 + 1];
					reference18 = val6 + Vector3.Scale(spriteDef.positions[1], scale);
					ref Vector3 reference19 = ref pos[offset + num3 + 2];
					reference19 = val6 + Vector3.Scale(spriteDef.positions[2], scale);
					ref Vector3 reference20 = ref pos[offset + num3 + 3];
					reference20 = val6 + Vector3.Scale(spriteDef.positions[3], scale);
					ref Vector2 reference21 = ref uv[offset + num3];
					reference21 = spriteDef.uvs[0];
					ref Vector2 reference22 = ref uv[offset + num3 + 1];
					reference22 = spriteDef.uvs[1];
					ref Vector2 reference23 = ref uv[offset + num3 + 2];
					reference23 = spriteDef.uvs[2];
					ref Vector2 reference24 = ref uv[offset + num3 + 3];
					reference24 = spriteDef.uvs[3];
				}
				num3 += 4;
				val5.x += val4.x;
			}
			val5.y += val4.y;
		}
	}

	public static void SetTiledSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GetTiledSpriteGeomDesc(out var _, out var numIndices, spriteDef, dimensions);
		int num = 0;
		for (int i = 0; i < numIndices; i += 6)
		{
			indices[offset + i] = vStart + spriteDef.indices[0] + num;
			indices[offset + i + 1] = vStart + spriteDef.indices[1] + num;
			indices[offset + i + 2] = vStart + spriteDef.indices[2] + num;
			indices[offset + i + 3] = vStart + spriteDef.indices[3] + num;
			indices[offset + i + 4] = vStart + spriteDef.indices[4] + num;
			indices[offset + i + 5] = vStart + spriteDef.indices[5] + num;
			num += 4;
		}
	}

	public static void SetBoxMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, Vector3 origin, Vector3 extents, Matrix4x4 mat, Vector3 baseScale)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		boxScaleMatrix.m03 = origin.x * baseScale.x;
		boxScaleMatrix.m13 = origin.y * baseScale.y;
		boxScaleMatrix.m23 = origin.z * baseScale.z;
		boxScaleMatrix.m00 = extents.x * baseScale.x;
		boxScaleMatrix.m11 = extents.y * baseScale.y;
		boxScaleMatrix.m22 = extents.z * baseScale.z;
		Matrix4x4 val = mat * boxScaleMatrix;
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref pos[posOffset + i];
			reference = ((Matrix4x4)(ref val)).MultiplyPoint(boxUnitVertices[i]);
		}
		float num = mat.m00 * mat.m11 * mat.m22 * baseScale.x * baseScale.y * baseScale.z;
		int[] array = ((!(num >= 0f)) ? boxIndicesBack : boxIndicesFwd);
		for (int j = 0; j < array.Length; j++)
		{
			indices[indicesOffset + j] = vStart + array[j];
		}
	}

	public static void SetSpriteDefinitionMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, tk2dSpriteDefinition spriteDef, Matrix4x4 mat, Vector3 baseScale)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < spriteDef.colliderVertices.Length; i++)
		{
			Vector3 val = Vector3.Scale(spriteDef.colliderVertices[i], baseScale);
			val = ((Matrix4x4)(ref mat)).MultiplyPoint(val);
			pos[posOffset + i] = val;
		}
		float num = mat.m00 * mat.m11 * mat.m22;
		int[] array = ((!(num >= 0f)) ? spriteDef.colliderIndicesBack : spriteDef.colliderIndicesFwd);
		for (int j = 0; j < array.Length; j++)
		{
			indices[indicesOffset + j] = vStart + array[j];
		}
	}

	public static void SetSpriteVertexNormals(Vector3[] pos, Vector3 pMin, Vector3 pMax, Vector3[] spriteDefNormals, Vector4[] spriteDefTangents, Vector3[] normals, Vector4[] tangents)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pMax - pMin;
		int num = pos.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 val2 = pos[i];
			float num2 = (val2.x - pMin.x) / val.x;
			float num3 = (val2.y - pMin.y) / val.y;
			float num4 = (1f - num2) * (1f - num3);
			float num5 = num2 * (1f - num3);
			float num6 = (1f - num2) * num3;
			float num7 = num2 * num3;
			if (spriteDefNormals != null && spriteDefNormals.Length == 4 && i < normals.Length)
			{
				ref Vector3 reference = ref normals[i];
				reference = spriteDefNormals[0] * num4 + spriteDefNormals[1] * num5 + spriteDefNormals[2] * num6 + spriteDefNormals[3] * num7;
			}
			if (spriteDefTangents != null && spriteDefTangents.Length == 4 && i < tangents.Length)
			{
				ref Vector4 reference2 = ref tangents[i];
				reference2 = spriteDefTangents[0] * num4 + spriteDefTangents[1] * num5 + spriteDefTangents[2] * num6 + spriteDefTangents[3] * num7;
			}
		}
	}
}
