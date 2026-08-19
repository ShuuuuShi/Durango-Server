using UnityEngine;

public class CheatUtility : MonoBehaviour
{
	private PlayerBehavior _player;

	public PlayerBehavior GetLocalPlayer()
	{
		if ((Object)(object)_player != (Object)null)
		{
			return _player;
		}
		_player = PlayerBehavior.LocalPlayer;
		return _player;
	}
}
