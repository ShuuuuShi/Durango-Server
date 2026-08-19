using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace tk2dRuntime;

internal static class SpriteCollectionGenerator
{
	public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return CreateFromTexture(texture, size, new string[1] { "Unnamed" }, (Rect[])(object)new Rect[1] { region }, (Vector2[])(object)new Vector2[1] { anchor });
	}

	public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, string[] names, Rect[] regions, Vector2[] anchors)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector2 textureDimensions = default(Vector2);
		((Vector2)(ref textureDimensions))._002Ector((float)texture.width, (float)texture.height);
		return CreateFromTexture(texture, size, textureDimensions, names, regions, null, anchors, null);
	}

	public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Vector2 textureDimensions, string[] names, Rect[] regions, Rect[] trimRects, Vector2[] anchors, bool[] rotated)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return CreateFromTexture(null, texture, size, textureDimensions, names, regions, trimRects, anchors, rotated);
	}

	public static tk2dSpriteCollectionData CreateFromTexture(GameObject parentObject, Texture texture, tk2dSpriteCollectionSize size, Vector2 textureDimensions, string[] names, Rect[] regions, Rect[] trimRects, Vector2[] anchors, bool[] rotated)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (GameObject)(((Object)(object)parentObject != (Object)null) ? ((object)parentObject) : ((object)new GameObject("SpriteCollection")));
		tk2dSpriteCollectionData tk2dSpriteCollectionData = val.AddComponent<tk2dSpriteCollectionData>();
		tk2dSpriteCollectionData.Transient = true;
		tk2dSpriteCollectionData.version = 3;
		tk2dSpriteCollectionData.invOrthoSize = 1f / size.OrthoSize;
		tk2dSpriteCollectionData.halfTargetHeight = size.TargetHeight * 0.5f;
		tk2dSpriteCollectionData.premultipliedAlpha = false;
		string text = "tk2d/BlendVertexColor";
		tk2dSpriteCollectionData.material = new Material(Shader.Find(text));
		tk2dSpriteCollectionData.material.mainTexture = texture;
		tk2dSpriteCollectionData.materials = (Material[])(object)new Material[1] { tk2dSpriteCollectionData.material };
		tk2dSpriteCollectionData.textures = (Texture[])(object)new Texture[1] { texture };
		tk2dSpriteCollectionData.buildKey = Random.Range(0, int.MaxValue);
		float scale = 2f * size.OrthoSize / size.TargetHeight;
		Rect trimRect = default(Rect);
		((Rect)(ref trimRect))._002Ector(0f, 0f, 0f, 0f);
		tk2dSpriteCollectionData.spriteDefinitions = new tk2dSpriteDefinition[regions.Length];
		for (int i = 0; i < regions.Length; i++)
		{
			bool flag = rotated != null && rotated[i];
			if (trimRects != null)
			{
				trimRect = trimRects[i];
			}
			else if (flag)
			{
				((Rect)(ref trimRect)).Set(0f, 0f, ((Rect)(ref regions[i])).height, ((Rect)(ref regions[i])).width);
			}
			else
			{
				((Rect)(ref trimRect)).Set(0f, 0f, ((Rect)(ref regions[i])).width, ((Rect)(ref regions[i])).height);
			}
			tk2dSpriteCollectionData.spriteDefinitions[i] = CreateDefinitionForRegionInTexture(names[i], textureDimensions, scale, regions[i], trimRect, anchors[i], flag);
		}
		tk2dSpriteDefinition[] spriteDefinitions = tk2dSpriteCollectionData.spriteDefinitions;
		foreach (tk2dSpriteDefinition tk2dSpriteDefinition in spriteDefinitions)
		{
			tk2dSpriteDefinition.material = tk2dSpriteCollectionData.material;
		}
		return tk2dSpriteCollectionData;
	}

	private static tk2dSpriteDefinition CreateDefinitionForRegionInTexture(string name, Vector2 textureDimensions, float scale, Rect uvRegion, Rect trimRect, Vector2 anchor, bool rotated)
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		float height = ((Rect)(ref uvRegion)).height;
		float width = ((Rect)(ref uvRegion)).width;
		float x = textureDimensions.x;
		float y = textureDimensions.y;
		tk2dSpriteDefinition tk2dSpriteDefinition = new tk2dSpriteDefinition();
		tk2dSpriteDefinition.flipped = (rotated ? tk2dSpriteDefinition.FlipMode.TPackerCW : tk2dSpriteDefinition.FlipMode.None);
		tk2dSpriteDefinition.extractRegion = false;
		tk2dSpriteDefinition.name = name;
		tk2dSpriteDefinition.colliderType = tk2dSpriteDefinition.ColliderType.Unset;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0.001f, 0.001f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((((Rect)(ref uvRegion)).x + val.x) / x, 1f - (((Rect)(ref uvRegion)).y + ((Rect)(ref uvRegion)).height + val.y) / y);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector((((Rect)(ref uvRegion)).x + ((Rect)(ref uvRegion)).width - val.x) / x, 1f - (((Rect)(ref uvRegion)).y - val.y) / y);
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector(((Rect)(ref trimRect)).x - anchor.x, 0f - ((Rect)(ref trimRect)).y + anchor.y);
		if (rotated)
		{
			val4.y -= width;
		}
		val4 *= scale;
		Vector3 val5 = default(Vector3);
		((Vector3)(ref val5))._002Ector((0f - anchor.x) * scale, anchor.y * scale, 0f);
		Vector3 val6 = val5 + new Vector3(((Rect)(ref trimRect)).width * scale, (0f - ((Rect)(ref trimRect)).height) * scale, 0f);
		Vector3 val7 = default(Vector3);
		((Vector3)(ref val7))._002Ector(0f, (0f - height) * scale, 0f);
		Vector3 val8 = val7 + new Vector3(width * scale, height * scale, 0f);
		if (rotated)
		{
			tk2dSpriteDefinition.positions = (Vector3[])(object)new Vector3[4]
			{
				new Vector3(0f - val8.y + val4.x, val7.x + val4.y, 0f),
				new Vector3(0f - val7.y + val4.x, val7.x + val4.y, 0f),
				new Vector3(0f - val8.y + val4.x, val8.x + val4.y, 0f),
				new Vector3(0f - val7.y + val4.x, val8.x + val4.y, 0f)
			};
			tk2dSpriteDefinition.uvs = (Vector2[])(object)new Vector2[4]
			{
				new Vector2(val2.x, val3.y),
				new Vector2(val2.x, val2.y),
				new Vector2(val3.x, val3.y),
				new Vector2(val3.x, val2.y)
			};
		}
		else
		{
			tk2dSpriteDefinition.positions = (Vector3[])(object)new Vector3[4]
			{
				new Vector3(val7.x + val4.x, val7.y + val4.y, 0f),
				new Vector3(val8.x + val4.x, val7.y + val4.y, 0f),
				new Vector3(val7.x + val4.x, val8.y + val4.y, 0f),
				new Vector3(val8.x + val4.x, val8.y + val4.y, 0f)
			};
			tk2dSpriteDefinition.uvs = (Vector2[])(object)new Vector2[4]
			{
				new Vector2(val2.x, val2.y),
				new Vector2(val3.x, val2.y),
				new Vector2(val2.x, val3.y),
				new Vector2(val3.x, val3.y)
			};
		}
		tk2dSpriteDefinition.normals = (Vector3[])(object)new Vector3[0];
		tk2dSpriteDefinition.tangents = (Vector4[])(object)new Vector4[0];
		tk2dSpriteDefinition.indices = new int[6] { 0, 3, 1, 2, 3, 0 };
		Vector3 val9 = default(Vector3);
		((Vector3)(ref val9))._002Ector(val5.x, val6.y, 0f);
		Vector3 val10 = default(Vector3);
		((Vector3)(ref val10))._002Ector(val6.x, val5.y, 0f);
		tk2dSpriteDefinition.boundsData = (Vector3[])(object)new Vector3[2]
		{
			(val10 + val9) / 2f,
			val10 - val9
		};
		tk2dSpriteDefinition.untrimmedBoundsData = (Vector3[])(object)new Vector3[2]
		{
			(val10 + val9) / 2f,
			val10 - val9
		};
		tk2dSpriteDefinition.texelSize = new Vector2(scale, scale);
		return tk2dSpriteDefinition;
	}

	public static tk2dSpriteCollectionData CreateFromTexturePacker(tk2dSpriteCollectionSize spriteCollectionSize, string texturePackerFileContents, Texture texture)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		List<Rect> list2 = new List<Rect>();
		List<Rect> list3 = new List<Rect>();
		List<Vector2> list4 = new List<Vector2>();
		List<bool> list5 = new List<bool>();
		int num = 0;
		TextReader textReader = new StringReader(texturePackerFileContents);
		bool flag = false;
		bool flag2 = false;
		string item = string.Empty;
		Rect item2 = default(Rect);
		Rect item3 = default(Rect);
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		for (string text = textReader.ReadLine(); text != null; text = textReader.ReadLine())
		{
			if (text.Length > 0)
			{
				char c = text[0];
				switch (num)
				{
				case 0:
					switch (c)
					{
					case 'w':
						zero.x = int.Parse(text.Substring(2));
						break;
					case 'h':
						zero.y = int.Parse(text.Substring(2));
						break;
					case '~':
						num++;
						break;
					}
					break;
				case 1:
					switch (c)
					{
					case 'n':
						item = text.Substring(2);
						break;
					case 'r':
						flag = int.Parse(text.Substring(2)) == 1;
						break;
					case 's':
					{
						string[] array = text.Split();
						((Rect)(ref item2)).Set((float)int.Parse(array[1]), (float)int.Parse(array[2]), (float)int.Parse(array[3]), (float)int.Parse(array[4]));
						break;
					}
					case 'o':
					{
						string[] array2 = text.Split();
						((Rect)(ref item3)).Set((float)int.Parse(array2[1]), (float)int.Parse(array2[2]), (float)int.Parse(array2[3]), (float)int.Parse(array2[4]));
						flag2 = true;
						break;
					}
					case '~':
						list.Add(item);
						list5.Add(flag);
						list2.Add(item2);
						if (!flag2)
						{
							if (flag)
							{
								((Rect)(ref item3)).Set(0f, 0f, ((Rect)(ref item2)).height, ((Rect)(ref item2)).width);
							}
							else
							{
								((Rect)(ref item3)).Set(0f, 0f, ((Rect)(ref item2)).width, ((Rect)(ref item2)).height);
							}
						}
						list3.Add(item3);
						((Vector2)(ref zero2)).Set((float)(int)(((Rect)(ref item3)).width / 2f), (float)(int)(((Rect)(ref item3)).height / 2f));
						list4.Add(zero2);
						item = string.Empty;
						flag2 = false;
						flag = false;
						break;
					}
					break;
				}
			}
		}
		return CreateFromTexture(texture, spriteCollectionSize, zero, list.ToArray(), list2.ToArray(), list3.ToArray(), list4.ToArray(), list5.ToArray());
	}
}
