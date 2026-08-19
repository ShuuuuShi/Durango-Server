using Durango.Utils;

namespace Durango.Logic.PlayGuide;

public class AirBalloonLandingTodo : ToDoBase
{
	public override void OnAddItem()
	{
		if ((bool)PlayerBehavior.LocalPlayer && PlayerBehavior.LocalPlayer.Driver.IsHovering)
		{
			Singleton<PetManager>.Instance().AirBalloonUnmounted += AirBalloonUnmounted;
		}
		else
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		Singleton<PetManager>.Instance().AirBalloonUnmounted -= AirBalloonUnmounted;
	}

	private void AirBalloonUnmounted()
	{
		CallComplete();
	}
}
