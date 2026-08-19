using System;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

public class DetectWarpHoleRadar : MonoBehaviour
{
	[Serializable]
	public struct Circles
	{
		public UIWidget _widget;

		public TweenScale _tweenScale;

		public TweenAlpha _tweenAlpha;
	}

	[SerializeField]
	private GameObject _spinner;

	[SerializeField]
	private TweenRotation _tweenRotationForSpinner;

	[SerializeField]
	private Circles[] _circles;

	public int CurrentSpinCount { get; private set; }

	public float CurrentAngle => Maths.PositiveAngDeg(_spinner.transform.localEulerAngles.z);

	public void Init()
	{
		_tweenRotationForSpinner.AddOnFinished(OnFinishedTweenRotationForSpinner);
	}

	public void BeginSpinning()
	{
		FinishSpinning();
		StartTweener(_tweenRotationForSpinner);
		CurrentSpinCount = 0;
		for (int i = 0; i < _circles.Length; i++)
		{
			_circles[i]._widget.alpha = 0f;
			StartTweener(_circles[i]._tweenAlpha);
			StartTweener(_circles[i]._tweenScale);
		}
	}

	public void FinishSpinning()
	{
		for (int i = 0; i < _circles.Length; i++)
		{
			StopTweener(_circles[i]._tweenAlpha);
			StopTweener(_circles[i]._tweenScale);
		}
		StopTweener(_tweenRotationForSpinner);
	}

	private void OnFinishedTweenRotationForSpinner()
	{
		StartTweener(_tweenRotationForSpinner);
		CurrentSpinCount++;
	}

	private static void StartTweener(UITweener tweener)
	{
		tweener.tweenFactor = 0f;
		tweener.PlayForward();
	}

	private static void StopTweener(UITweener tweener)
	{
		tweener.ResetToBeginning();
		tweener.enabled = false;
	}
}
