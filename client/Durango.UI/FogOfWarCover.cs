using System;
using System.Collections.Generic;
using Durango.Terrain;
using UnityEngine;

namespace Durango.UI;

public class FogOfWarCover : MonoBehaviour
{
	[SerializeField]
	private Material _revealMaterial;

	[SerializeField]
	private bool _updateLocalPlayer;

	private RenderTexture _renderTexture;

	private bool _setDefoggingChunk;

	private bool _isFirstRender = true;

	private Vector2? _lastFogRenderPos;

	private int _mapSize;

	private Action<RenderTexture> _onCompleted;

	private readonly Queue<Vector2>[] _defoggingChunks = new Queue<Vector2>[4];

	public RenderTexture Initialize(int size, Action<RenderTexture> completed)
	{
		RenderTexture.ReleaseTemporary(_renderTexture);
		RenderTextureFormat format = RenderTextureFormat.ARGB32;
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
		{
			format = RenderTextureFormat.R8;
		}
		_mapSize = size;
		_renderTexture = RenderTexture.GetTemporary(size, size, 0, format);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _renderTexture;
		_renderTexture.MarkRestoreExpected();
		GL.Clear(clearDepth: false, clearColor: true, Color.white);
		RenderTexture.active = active;
		_isFirstRender = true;
		_setDefoggingChunk = false;
		_onCompleted = completed;
		return _renderTexture;
	}

	private void OnDestroy()
	{
		if (_renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(_renderTexture);
			_renderTexture = null;
		}
	}

	private void Update()
	{
		if (!TerrainBase.IsPlayerInitialized || !_setDefoggingChunk || _renderTexture == null)
		{
			return;
		}
		bool flag = !HasDefoggingChunks();
		if (_onCompleted != null && flag)
		{
			_onCompleted(_renderTexture);
			_onCompleted = null;
		}
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		if (_updateLocalPlayer)
		{
			Vector2? lastFogRenderPos = _lastFogRenderPos;
			if (!lastFogRenderPos.HasValue || !(lastFogRenderPos.GetValueOrDefault() == (Vector2)currentPosition))
			{
				goto IL_00a4;
			}
		}
		if (flag)
		{
			return;
		}
		goto IL_00a4;
		IL_00a4:
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _renderTexture;
		_renderTexture.MarkRestoreExpected();
		_revealMaterial.SetPass(0);
		GL.PushMatrix();
		GL.LoadOrtho();
		if (_isFirstRender)
		{
			_isFirstRender = false;
			GL.Clear(clearDepth: false, clearColor: true, Color.white);
			GL.PopMatrix();
			RenderTexture.active = active;
			return;
		}
		float size = 20f / (float)_mapSize;
		bool flag2 = false;
		for (int i = 0; i < _defoggingChunks.Length; i++)
		{
			Queue<Vector2> queue = _defoggingChunks[i];
			if (queue == null)
			{
				continue;
			}
			for (int j = 0; j < 100; j++)
			{
				if (queue.Count > 0)
				{
					Vector2 pos = queue.Dequeue();
					pos.x += 0.5f;
					pos.y += 0.5f;
					pos.x *= 16f;
					pos.y *= 16f;
					DrawQuad(pos, _mapSize, size);
					flag2 = true;
					continue;
				}
				_defoggingChunks[i] = null;
				break;
			}
			if (flag2)
			{
				break;
			}
		}
		if (!flag2 && _updateLocalPlayer)
		{
			Vector2 vector = Util.ClientPositionToTilePosition(currentPosition);
			Vector2? lastFogRenderPos2 = _lastFogRenderPos;
			if (vector != lastFogRenderPos2)
			{
				DrawQuad(vector, _mapSize, size);
				_lastFogRenderPos = vector;
			}
		}
		GL.PopMatrix();
		RenderTexture.active = active;
	}

	private static void DrawQuad(Vector2 pos, int revealWidth, float size)
	{
		GL.Begin(7);
		float num = pos.x / (float)revealWidth - size * 0.5f;
		float num2 = pos.y / (float)revealWidth - size * 0.5f;
		GL.TexCoord(new Vector3(0f, 0f));
		GL.Vertex(new Vector3(num, num2));
		GL.TexCoord(new Vector3(1f, 0f));
		GL.Vertex(new Vector3(num + size, num2));
		GL.TexCoord(new Vector3(1f, 1f));
		GL.Vertex(new Vector3(num + size, num2 + size));
		GL.TexCoord(new Vector3(0f, 1f));
		GL.Vertex(new Vector3(num, num2 + size));
		GL.End();
	}

	public void SetDefoggedChunks(BitArray2D visibleGrid)
	{
		int width = visibleGrid.Width;
		int height = visibleGrid.Height;
		for (int i = 0; i < _defoggingChunks.Length; i++)
		{
			_defoggingChunks[i] = new Queue<Vector2>();
		}
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				if (!visibleGrid.Get(j, k))
				{
					continue;
				}
				_defoggingChunks[0].Enqueue(new Vector2(j, k));
				bool flag = true;
				for (int l = 0; l < 3; l++)
				{
					int num = j + ((l != 1) ? 1 : 0);
					int num2 = k + ((l != 0) ? 1 : 0);
					if (num >= width || num2 >= height || !visibleGrid.Get(num, num2))
					{
						flag = false;
					}
					else if (flag || l != 2)
					{
						Vector2 item = new Vector2((float)j + (float)(num - j) * 0.5f, (float)k + (float)(num2 - k) * 0.5f);
						_defoggingChunks[1 + l].Enqueue(item);
					}
				}
			}
		}
		_setDefoggingChunk = true;
	}

	private bool HasDefoggingChunks()
	{
		for (int i = 0; i < _defoggingChunks.Length; i++)
		{
			Queue<Vector2> queue = _defoggingChunks[i];
			if (queue != null && queue.Count > 0)
			{
				return true;
			}
		}
		return false;
	}
}
