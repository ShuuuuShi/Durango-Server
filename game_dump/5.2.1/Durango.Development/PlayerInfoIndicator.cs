using Durango.Terrain;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Development;

public class PlayerInfoIndicator : MonoBehaviour
{
	[SerializeField]
	private Vector2 _textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);

	[SerializeField]
	private Vector2 _rectSize = new Vector2(40f, 30f);

	[SerializeField]
	private GUIStyle _fontStyle;

	[SerializeField]
	private bool _showPlayerInfoOnGUI = true;

	private void Start()
	{
		_textPosition = new Vector2((float)Screen.width / 2f - 50f, 50f);
	}

	private void OnGUI()
	{
		if (_showPlayerInfoOnGUI)
		{
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if (!(localPlayer == null))
			{
				Vector3 currentPosition = localPlayer.CurrentPosition;
				Vector3 worldPosition = Util.ClientPositionToWorldPosition(currentPosition);
				TerrainChunkBase chunkFromWorldPosition = Singleton<TerrainBase>.Instance().GetChunkFromWorldPosition(worldPosition);
				int num = Mathf.FloorToInt((worldPosition.x - (float)(chunkFromWorldPosition.Coord.x * 3200)) / 200f);
				int num2 = Mathf.FloorToInt((worldPosition.z - (float)(chunkFromWorldPosition.Coord.y * 3200)) / 200f);
				Biome tileBiome = chunkFromWorldPosition.GetTileBiome(worldPosition);
				string text = "Player ID: " + localPlayer.EntityId + "\n";
				string text2 = text;
				text = text2 + "Player Height: " + currentPosition.y + "\n";
				text2 = text;
				text = string.Concat(text2, "Tile Biome: ", tileBiome, " (", num, ", ", num2, ")\n");
				GUI.Label(new Rect(_textPosition.x, _textPosition.y, _rectSize.x, _rectSize.y), text, _fontStyle);
			}
		}
	}
}
