using System;
using UnityEngine;

public class AreaOfEffectSprite : MonoBehaviour
{
	public Action<AreaOfEffectSprite> OnFinished;

	public UIPanel panel;

	public UISprite bgSprite;

	public UISprite upperSprite;

	public void Play(Vector3 position, int width, int height, int startAngle, int endAngle, UIWidget.Pivot pivot, float yaw, float duration, bool isRectangle = false)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.position = position;
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		eulerAngles.y = yaw;
		((Component)this).transform.localEulerAngles = eulerAngles;
		UICircleFill component = ((Component)bgSprite).GetComponent<UICircleFill>();
		component.ClearHideRanges();
		component.AddHideRange(endAngle, startAngle);
		component = ((Component)upperSprite).GetComponent<UICircleFill>();
		component.ClearHideRanges();
		component.AddHideRange(endAngle, startAngle);
		UISprite uISprite = upperSprite;
		string spriteName = ((!isRectangle) ? "bg_circle" : "bg_white");
		bgSprite.spriteName = spriteName;
		uISprite.spriteName = spriteName;
		bgSprite.width = width;
		upperSprite.width = width;
		bgSprite.height = height;
		upperSprite.height = height;
		bgSprite.pivot = pivot;
		upperSprite.pivot = pivot;
		((Component)bgSprite).transform.localPosition = Vector3.zero;
		((Component)upperSprite).transform.localPosition = Vector3.zero;
		((Component)upperSprite).transform.localScale = Vector3.zero;
		TweenScale tweenScale = TweenScale.Begin(((Component)upperSprite).gameObject, duration, Vector3.one);
		tweenScale.SetOnFinished(OnFinish);
	}

	public void Interrupt()
	{
		OnFinish();
	}

	private void OnFinish()
	{
		((Component)this).gameObject.SetActive(false);
		if (OnFinished != null)
		{
			OnFinished(this);
		}
	}
}
