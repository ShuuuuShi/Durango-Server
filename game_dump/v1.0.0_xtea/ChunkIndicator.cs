using UnityEngine;

public class ChunkIndicator : MonoBehaviour
{
	public Vector2 _textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);

	public Vector2 _rectSize = new Vector2(40f, 30f);

	public GUIStyle _fontStyle;

	public bool _showChunkInfoOnGUI = true;

	private Vector2 _chunkCoords;

	private GameObject _player;

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);
	}

	private void Update()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_player == (Object)null)
		{
			_player = GameObject.Find("Player");
		}
		if (!((Object)(object)_player == (Object)null))
		{
			Vector3 localPosition = _player.transform.localPosition;
			_chunkCoords = TerrainA6.ClientPositionToChunkCoords(localPosition);
		}
	}

	private void OnGUI()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (_showChunkInfoOnGUI)
		{
			GUI.Label(new Rect(_textPosition.x, _textPosition.y, _rectSize.x, _rectSize.y), "Chunk(X, Y): " + (int)_chunkCoords.x + ", " + (int)_chunkCoords.y, _fontStyle);
		}
	}
}
