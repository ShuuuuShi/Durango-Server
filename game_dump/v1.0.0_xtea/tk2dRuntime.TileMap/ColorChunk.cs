using System;
using UnityEngine;

namespace tk2dRuntime.TileMap;

[Serializable]
public class ColorChunk
{
	public Color32[] colors;

	public bool Dirty { get; set; }

	public bool Empty => colors.Length == 0;

	public ColorChunk()
	{
		colors = (Color32[])(object)new Color32[0];
	}
}
