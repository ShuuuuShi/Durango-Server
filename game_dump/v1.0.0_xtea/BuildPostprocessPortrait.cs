using Player;
using UnityEngine;

public class BuildPostprocessPortrait : MonoBehaviour
{
	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private Texture _textureMask;

	public string PlayerName { get; private set; }

	public void SetPlayerInfo(PlayerInfo info)
	{
		PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
		portraitArgument.Mask = _textureMask;
		PortraitBuilder.Set(portraitArgument, _texture);
		PlayerName = info.Name;
	}
}
