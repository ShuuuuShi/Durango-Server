using System;
using System.Collections;
using Durango.Prologue;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PrologueLoadingCurtain : LoadingCurtainBase
{
	[Serializable]
	private struct YearInfo
	{
		public UIWidget Parent;

		public UILabel Title;

		public UILabel Year;
	}

	[SerializeField]
	private YearInfo _yearInfo;

	[SerializeField]
	private UIWidget _downloadWarning;

	private bool _isTap;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		_isTap = false;
		StartCoroutine(CoShowRoutine());
		SetState(LoadingState.Open);
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
	}

	private void OnTouchScreen(GameObject obj, bool press)
	{
		_isTap = true;
	}

	private IEnumerator CoShowRoutine()
	{
		base.Widget.alpha = 1f;
		yield return ShowYearInfo();
		if (!GameManager.IsPrologueMode)
		{
			yield return WaitForChunkLoading();
			if (LoadingCurtainBase.IsChunkLoadFailed)
			{
				yield break;
			}
		}
		yield return WaitForTap();
		SetState(LoadingState.Closing);
		yield return Fadeout();
		SetState(LoadingState.Closed);
	}

	private IEnumerator WarnAboutDataNetwork()
	{
		_downloadWarning.gameObject.SetActive(value: true);
		_yearInfo.Parent.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(2f);
		yield return WaitForTap();
		while (_downloadWarning.alpha > 0f)
		{
			_downloadWarning.alpha -= Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator ShowYearInfo()
	{
		_downloadWarning.gameObject.SetActive(value: false);
		_yearInfo.Parent.gameObject.SetActive(value: true);
		bool isPrologue = GameManager.IsPrologueMode;
		_yearInfo.Title.text = ((!isPrologue) ? T._("미지의 땅") : T._("지구"));
		_yearInfo.Year.text = ((!isPrologue) ? T._("연도 불명") : ConditionalText.Format(T._("서기 {year}년")));
		yield return new WaitForSeconds(2f);
	}

	private IEnumerator WaitForTap()
	{
		_isTap = false;
		float remainTime = 3f;
		while (remainTime > 0f && !_isTap)
		{
			remainTime -= Time.deltaTime;
			yield return null;
		}
	}
}
