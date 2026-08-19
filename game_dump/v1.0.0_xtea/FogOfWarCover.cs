using System.Collections.Generic;
using UnityEngine;

public class FogOfWarCover : MonoBehaviour
{
	[SerializeField]
	private Material _revealMaterial;

	[SerializeField]
	private RenderTexture _renderTexture;

	private bool _setDefoggingChunk;

	private bool _isFirstRender = true;

	private Vector3 _lastPlayerPos = Vector3.zero;

	private Vector2 _lastFogRenderPos = Vector2.op_Implicit(Vector3.zero);

	private int _lastRevealRenderFrame = -1;

	private readonly Queue<Vector2>[] _defoggingChunks = new Queue<Vector2>[4];

	private void Awake()
	{
		KSingleton<GameManager>.Instance().PreReconnect += delegate
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = _renderTexture;
			_renderTexture.MarkRestoreExpected();
			GL.Clear(false, true, Color.white);
			RenderTexture.active = active;
		};
	}

	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (!TerrainA6.IsPlayerInitialized || !_setDefoggingChunk)
		{
			return;
		}
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		if (_lastRevealRenderFrame != -1 && _lastPlayerPos == currentPosition && _lastPlayerPos != Vector3.zero && !HasDefoggingChunks())
		{
			return;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _renderTexture;
		_renderTexture.MarkRestoreExpected();
		_revealMaterial.SetPass(0);
		GL.PushMatrix();
		GL.LoadOrtho();
		if (_isFirstRender)
		{
			_isFirstRender = false;
			GL.Clear(false, true, Color.white);
			GL.PopMatrix();
			RenderTexture.active = active;
			return;
		}
		int mapSize = UIManager.MapContext.MapSize;
		float size = 20f / (float)mapSize;
		bool flag = false;
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
					DrawQuad(pos, mapSize, size);
					flag = true;
					continue;
				}
				_defoggingChunks[i] = null;
				break;
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			_lastRevealRenderFrame = -1;
		}
		else
		{
			Vector2 val = TerrainA6.ClientPositionToTilePosition(currentPosition);
			if (_lastRevealRenderFrame == -1 || val != _lastFogRenderPos)
			{
				DrawQuad(val, mapSize, size);
				_lastFogRenderPos = val;
				_lastRevealRenderFrame = Time.frameCount;
			}
		}
		GL.PopMatrix();
		RenderTexture.active = active;
		_lastPlayerPos = currentPosition;
	}

	private static void DrawQuad(Vector2 pos, int revealWidth, float size)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		GL.Begin(7);
		float num = pos.x / (float)revealWidth - size * 0.5f;
		float num2 = pos.y / (float)revealWidth - size * 0.5f;
		GL.MultiTexCoord(0, new Vector3(0f, 0f));
		GL.MultiTexCoord(1, new Vector3(num, num2));
		GL.Vertex(new Vector3(num, num2));
		GL.MultiTexCoord(0, new Vector3(1f, 0f));
		GL.MultiTexCoord(1, new Vector3(num + size, num2));
		GL.Vertex(new Vector3(num + size, num2));
		GL.MultiTexCoord(0, new Vector3(1f, 1f));
		GL.MultiTexCoord(1, new Vector3(num + size, num2 + size));
		GL.Vertex(new Vector3(num + size, num2 + size));
		GL.MultiTexCoord(0, new Vector3(0f, 1f));
		GL.MultiTexCoord(1, new Vector3(num, num2 + size));
		GL.Vertex(new Vector3(num, num2 + size));
		GL.End();
	}

	public void SetDefoggedChunks(IBitArray2d visibleGrid)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		int width = visibleGrid.Width;
		int height = visibleGrid.Height;
		for (int i = 0; i < _defoggingChunks.Length; i++)
		{
			_defoggingChunks[i] = new Queue<Vector2>();
		}
		Vector2 item = default(Vector2);
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				if (!visibleGrid.Get(j, k))
				{
					continue;
				}
				_defoggingChunks[0].Enqueue(new Vector2((float)j, (float)k));
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
						((Vector2)(ref item))._002Ector((float)j + (float)(num - j) * 0.5f, (float)k + (float)(num2 - k) * 0.5f);
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

	public bool IsDefoggingCompleted()
	{
		return _setDefoggingChunk && !HasDefoggingChunks();
	}
}
