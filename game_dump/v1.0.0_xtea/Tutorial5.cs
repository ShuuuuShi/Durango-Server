using UnityEngine;

public class Tutorial5 : MonoBehaviour
{
	public void SetDurationToCurrentProgress()
	{
		UITweener[] componentsInChildren = ((Component)this).GetComponentsInChildren<UITweener>();
		UITweener[] array = componentsInChildren;
		foreach (UITweener uITweener in array)
		{
			uITweener.duration = Mathf.Lerp(2f, 0.5f, UIProgressBar.current.value);
		}
	}
}
