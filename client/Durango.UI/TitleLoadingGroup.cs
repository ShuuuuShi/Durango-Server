using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TitleLoadingGroup : MonoBehaviour
{
	[SerializeField]
	private PrerequisiteLoader _prerequisiteLoader;

	[SerializeField]
	private UIWidget _loadingCuratin;

	[SerializeField]
	private TweenAlpha _tweener;

	public float Duration => _tweener.duration;

	public GameObject LoadingCurtain => _loadingCuratin.gameObject;

	public PrerequisiteLoader PrerequisiteLoader => _prerequisiteLoader;

	public void Play(EventDelegate.Callback fadeOutFinished)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
		_loadingCuratin.gameObject.SetActive(value: true);
		_tweener.SetOnFinished(fadeOutFinished);
		_tweener.Sample(0f, isFinished: false);
		_tweener.PlayForward();
	}

	public void HideTitleSceneWithCurtain()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
		_loadingCuratin.gameObject.SetActive(value: true);
		_tweener.Sample(1f, isFinished: false);
	}
}
