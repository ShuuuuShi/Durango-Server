using TerrainData;
using UnityEngine;

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
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_textPosition = new Vector2((float)Screen.width / 2f - 50f, 50f);
	}

	private void OnGUI()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		if (_showPlayerInfoOnGUI)
		{
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if (!((Object)(object)localPlayer == (Object)null))
			{
				Vector3 currentPosition = localPlayer.CurrentPosition;
				Vector3 worldPosition = TerrainA6.ClientPositionToWorldPosition(currentPosition);
				TerrainChunkA6 chunkFromWorldPosition = TerrainA6.GetChunkFromWorldPosition(worldPosition);
				int num = Mathf.FloorToInt((worldPosition.x - chunkFromWorldPosition.Coords.x * 3200f) / 200f);
				int num2 = Mathf.FloorToInt((worldPosition.z - chunkFromWorldPosition.Coords.y * 3200f) / 200f);
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
