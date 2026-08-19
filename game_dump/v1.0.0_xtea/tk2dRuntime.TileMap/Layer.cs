using System;
using UnityEngine;

namespace tk2dRuntime.TileMap;

[Serializable]
public class Layer
{
	private const int tileMask = 16777215;

	private const int flagMask = -16777216;

	public int hash;

	public SpriteChannel spriteChannel;

	public int width;

	public int height;

	public int numColumns;

	public int numRows;

	public int divX;

	public int divY;

	public GameObject gameObject;

	public bool IsEmpty => spriteChannel.chunks.Length == 0;

	public int NumActiveChunks
	{
		get
		{
			int num = 0;
			SpriteChunk[] chunks = spriteChannel.chunks;
			foreach (SpriteChunk spriteChunk in chunks)
			{
				if (!spriteChunk.IsEmpty)
				{
					num++;
				}
			}
			return num;
		}
	}

	public Layer(int hash, int width, int height, int divX, int divY)
	{
		spriteChannel = new SpriteChannel();
		Init(hash, width, height, divX, divY);
	}

	public void Init(int hash, int width, int height, int divX, int divY)
	{
		this.divX = divX;
		this.divY = divY;
		this.hash = hash;
		numColumns = (width + divX - 1) / divX;
		numRows = (height + divY - 1) / divY;
		this.width = width;
		this.height = height;
		spriteChannel.chunks = new SpriteChunk[numColumns * numRows];
		for (int i = 0; i < numColumns * numRows; i++)
		{
			spriteChannel.chunks[i] = new SpriteChunk();
		}
	}

	public void Create()
	{
		spriteChannel.chunks = new SpriteChunk[numColumns * numRows];
	}

	public int[] GetChunkData(int x, int y)
	{
		return GetChunk(x, y).spriteIds;
	}

	public SpriteChunk GetChunk(int x, int y)
	{
		return spriteChannel.chunks[y * numColumns + x];
	}

	private SpriteChunk FindChunkAndCoordinate(int x, int y, out int offset)
	{
		int num = x / divX;
		int num2 = y / divY;
		SpriteChunk result = spriteChannel.chunks[num2 * numColumns + num];
		int num3 = x - num * divX;
		int num4 = y - num2 * divY;
		offset = num4 * divX + num3;
		return result;
	}

	private bool GetRawTileValue(int x, int y, ref int value)
	{
		int offset;
		SpriteChunk spriteChunk = FindChunkAndCoordinate(x, y, out offset);
		if (spriteChunk.spriteIds == null || spriteChunk.spriteIds.Length == 0)
		{
			return false;
		}
		value = spriteChunk.spriteIds[offset];
		return true;
	}

	private void SetRawTileValue(int x, int y, int value)
	{
		int offset;
		SpriteChunk spriteChunk = FindChunkAndCoordinate(x, y, out offset);
		if (spriteChunk != null)
		{
			CreateChunk(spriteChunk);
			spriteChunk.spriteIds[offset] = value;
			spriteChunk.Dirty = true;
		}
	}

	public void DestroyGameData(tk2dTileMap tilemap)
	{
		SpriteChunk[] chunks = spriteChannel.chunks;
		foreach (SpriteChunk spriteChunk in chunks)
		{
			if (spriteChunk.HasGameData)
			{
				spriteChunk.DestroyColliderData(tilemap);
				spriteChunk.DestroyGameData(tilemap);
			}
		}
	}

	public int GetTile(int x, int y)
	{
		int value = 0;
		if (GetRawTileValue(x, y, ref value) && value != -1)
		{
			return value & 0xFFFFFF;
		}
		return -1;
	}

	public tk2dTileFlags GetTileFlags(int x, int y)
	{
		int value = 0;
		if (GetRawTileValue(x, y, ref value) && value != -1)
		{
			return (tk2dTileFlags)(value & -16777216);
		}
		return tk2dTileFlags.None;
	}

	public int GetRawTile(int x, int y)
	{
		int value = 0;
		if (GetRawTileValue(x, y, ref value))
		{
			return value;
		}
		return -1;
	}

	public void SetTile(int x, int y, int tile)
	{
		tk2dTileFlags tileFlags = GetTileFlags(x, y);
		int value = ((tile != -1) ? (tile | (int)tileFlags) : (-1));
		SetRawTileValue(x, y, value);
	}

	public void SetTileFlags(int x, int y, tk2dTileFlags flags)
	{
		int tile = GetTile(x, y);
		if (tile != -1)
		{
			int value = tile | (int)flags;
			SetRawTileValue(x, y, value);
		}
	}

	public void ClearTile(int x, int y)
	{
		SetTile(x, y, -1);
	}

	public void SetRawTile(int x, int y, int rawTile)
	{
		SetRawTileValue(x, y, rawTile);
	}

	private void CreateChunk(SpriteChunk chunk)
	{
		if (chunk.spriteIds == null || chunk.spriteIds.Length == 0)
		{
			chunk.spriteIds = new int[divX * divY];
			for (int i = 0; i < divX * divY; i++)
			{
				chunk.spriteIds[i] = -1;
			}
		}
	}

	private void Optimize(SpriteChunk chunk)
	{
		bool flag = true;
		int[] spriteIds = chunk.spriteIds;
		foreach (int num in spriteIds)
		{
			if (num != -1)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			chunk.spriteIds = new int[0];
		}
	}

	public void Optimize()
	{
		SpriteChunk[] chunks = spriteChannel.chunks;
		foreach (SpriteChunk chunk in chunks)
		{
			Optimize(chunk);
		}
	}

	public void OptimizeIncremental()
	{
		SpriteChunk[] chunks = spriteChannel.chunks;
		foreach (SpriteChunk spriteChunk in chunks)
		{
			if (spriteChunk.Dirty)
			{
				Optimize(spriteChunk);
			}
		}
	}

	public void ClearDirtyFlag()
	{
		SpriteChunk[] chunks = spriteChannel.chunks;
		foreach (SpriteChunk spriteChunk in chunks)
		{
			spriteChunk.Dirty = false;
		}
	}
}
