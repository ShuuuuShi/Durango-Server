using System.Collections;

namespace Durango.UI;

public class BlankLoadingCurtain : LoadingCurtainBase
{
	private void OnEnable()
	{
		SetState(LoadingState.Open);
		StartCoroutine(CoShowRoutine());
	}

	private IEnumerator CoShowRoutine()
	{
		yield return Fadein();
		while (base.State == LoadingState.Open)
		{
			yield return null;
		}
		yield return Fadeout();
		SetState(LoadingState.Closed);
	}

	public void Close()
	{
		SetState(LoadingState.Closing);
	}
}
