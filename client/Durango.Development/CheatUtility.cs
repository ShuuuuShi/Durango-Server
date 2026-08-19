using UnityEngine;

namespace Durango.Development;

public class CheatUtility : MonoBehaviour
{
	private PlayerBehavior _player;

	public PlayerBehavior GetLocalPlayer()
	{
		if (_player != null)
		{
			return _player;
		}
		_player = PlayerBehavior.LocalPlayer;
		return _player;
	}
}
