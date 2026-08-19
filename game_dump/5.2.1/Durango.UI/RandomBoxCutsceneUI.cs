using System;
using Durango.Cutscene;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RandomBoxCutsceneUI : CutsceneUIBase
{
	[Serializable]
	private struct FadeInfo
	{
		public Color CurtainColor;

		public float Duration;
	}

	[SerializeField]
	private FadeInfo _openingFade;

	[SerializeField]
	private FadeInfo _closingFade;

	[SerializeField]
	private SelectableWidget _skipButton;

	[SerializeField]
	private UISprite _guide;

	[SerializeField]
	private UILabel _skipLabel;

	[SerializeField]
	private int _maxGuideCount;

	public int GuideCount
	{
		get
		{
			return Preferences.GetInt("cutscene_shop_guide_count");
		}
		set
		{
			Preferences.SetInt("cutscene_shop_guide_count", value);
		}
	}

	private void Awake()
	{
		_skipButton.Clicked = ResourceSingleton<Loader>.Instance().UnloadCutscene;
		_skipLabel.text = T._("건너뛰기");
	}

	public override void Open(Action callback)
	{
		base.gameObject.SetActive(value: true);
		UIManager.ShowLoadingCurtain<TransitionCurtain>().PlayColorRoutine(_openingFade.Duration, 0f, _openingFade.CurtainColor, callback);
	}

	public override void Close(Action callback)
	{
		UIManager.ShowLoadingCurtain<TransitionCurtain>().PlayColorRoutine(0f, _closingFade.Duration, _closingFade.CurtainColor, callback);
	}

	[UsedImplicitly]
	private void OnDrag(Vector2 delta)
	{
		RandomBoxScene randomBoxScene = ResourceSingleton<Loader>.Instance().Current as RandomBoxScene;
		if (randomBoxScene != null)
		{
			randomBoxScene.Unbox(delta);
		}
	}

	public void StartGuide()
	{
		if (GuideCount < _maxGuideCount)
		{
			GuideCount++;
			TweenPosition component = _guide.GetComponent<TweenPosition>();
			TweenAlpha component2 = _guide.GetComponent<TweenAlpha>();
			component.PlayForward();
			component2.PlayForward();
		}
	}

	private void OnDisable()
	{
		StopGuide();
	}

	public void StopGuide()
	{
		TweenPosition component = _guide.GetComponent<TweenPosition>();
		TweenAlpha component2 = _guide.GetComponent<TweenAlpha>();
		component.ResetToBeginning();
		component2.ResetToBeginning();
		component.enabled = false;
		component2.enabled = false;
	}
}
