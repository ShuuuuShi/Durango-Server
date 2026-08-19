using Durango.Utils.Extensions;

namespace Durango.Player.Animation;

public class PlayerAnimationClipInfo : PlayerAnimationClipInfoBase
{
	public bool IsLoop { get; set; }

	public float Length { get; set; }

	public PlayerAnimationClipTag Tag { get; set; }

	public float FadeOutTime { get; set; }

	public float FadeInTime { get; set; }

	public string EquipAnimation { get; set; }

	public PlayerRootMotionPath[] Path { get; set; }

	public PlayerAnimationClipInfo()
	{
		FadeInTime = -1f;
		FadeOutTime = -1f;
	}

	public bool HasAnimTag(PlayerAnimationClipTag tag)
	{
		return (Tag & tag) != 0;
	}

	public PlayerRootMotionPath GetPath(bool isMale)
	{
		if (Path == null)
		{
			return null;
		}
		return Path.Get((!isMale) ? 1 : 0);
	}
}
