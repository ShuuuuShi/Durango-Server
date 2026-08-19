using Durango.Terrain;
using UnityEngine;

namespace Durango.Development;

public class ChunkIndicator : MonoBehaviour
{
	public Vector2 _textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);

	public Vector2 _rectSize = new Vector2(40f, 30f);

	public GUIStyle _fontStyle;

	public bool _showChunkInfoOnGUI = true;

	private Point2 _chunkCoords;

	private GameObject _player;

	private void Start()
	{
		_textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);
	}

	private void Update()
	{
		if (_player == null)
		{
			_player = GameObject.Find("Player");
		}
		if (!(_player == null))
		{
			Vector3 localPosition = _player.transform.localPosition;
			_chunkCoords = Util.ClientPositionToChunkCoords(localPosition);
		}
	}

	private void OnGUI()
	{
		if (_showChunkInfoOnGUI)
		{
			GUI.Label(new Rect(_textPosition.x, _textPosition.y, _rectSize.x, _rectSize.y), "Chunk(X, Y): " + _chunkCoords.x + ", " + _chunkCoords.y, _fontStyle);
		}
	}
}
