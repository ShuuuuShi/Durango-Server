using System.Collections;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUITweenItem")]
public class tk2dUITweenItem : tk2dUIBaseItemControl
{
	private Vector3 onUpScale;

	public Vector3 onDownScale = new Vector3(0.9f, 0.9f, 0.9f);

	public float tweenDuration = 0.1f;

	public bool canButtonBeHeldDown = true;

	[SerializeField]
	private bool useOnReleaseInsteadOfOnUp;

	private bool internalTweenInProgress;

	private Vector3 tweenTargetScale = Vector3.one;

	private Vector3 tweenStartingScale = Vector3.one;

	private float tweenTimeElapsed;

	public bool UseOnReleaseInsteadOfOnUp => useOnReleaseInsteadOfOnUp;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		onUpScale = ((Component)this).transform.localScale;
	}

	private void OnEnable()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnDown += ButtonDown;
			if (canButtonBeHeldDown)
			{
				if (useOnReleaseInsteadOfOnUp)
				{
					uiItem.OnRelease += ButtonUp;
				}
				else
				{
					uiItem.OnUp += ButtonUp;
				}
			}
		}
		internalTweenInProgress = false;
		tweenTimeElapsed = 0f;
		((Component)this).transform.localScale = onUpScale;
	}

	private void OnDisable()
	{
		if (!Object.op_Implicit((Object)(object)uiItem))
		{
			return;
		}
		uiItem.OnDown -= ButtonDown;
		if (canButtonBeHeldDown)
		{
			if (useOnReleaseInsteadOfOnUp)
			{
				uiItem.OnRelease -= ButtonUp;
			}
			else
			{
				uiItem.OnUp -= ButtonUp;
			}
		}
	}

	private void ButtonDown()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (tweenDuration <= 0f)
		{
			((Component)this).transform.localScale = onDownScale;
			return;
		}
		((Component)this).transform.localScale = onUpScale;
		tweenTargetScale = onDownScale;
		tweenStartingScale = ((Component)this).transform.localScale;
		if (!internalTweenInProgress)
		{
			((MonoBehaviour)this).StartCoroutine(ScaleTween());
			internalTweenInProgress = true;
		}
	}

	private void ButtonUp()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (tweenDuration <= 0f)
		{
			((Component)this).transform.localScale = onUpScale;
			return;
		}
		tweenTargetScale = onUpScale;
		tweenStartingScale = ((Component)this).transform.localScale;
		if (!internalTweenInProgress)
		{
			((MonoBehaviour)this).StartCoroutine(ScaleTween());
			internalTweenInProgress = true;
		}
	}

	private IEnumerator ScaleTween()
	{
		for (tweenTimeElapsed = 0f; tweenTimeElapsed < tweenDuration; tweenTimeElapsed += tk2dUITime.deltaTime)
		{
			((Component)this).transform.localScale = Vector3.Lerp(tweenStartingScale, tweenTargetScale, tweenTimeElapsed / tweenDuration);
			yield return null;
		}
		((Component)this).transform.localScale = tweenTargetScale;
		internalTweenInProgress = false;
		if (!canButtonBeHeldDown)
		{
			if (tweenDuration <= 0f)
			{
				((Component)this).transform.localScale = onUpScale;
				yield break;
			}
			tweenTargetScale = onUpScale;
			tweenStartingScale = ((Component)this).transform.localScale;
			((MonoBehaviour)this).StartCoroutine(ScaleTween());
			internalTweenInProgress = true;
		}
	}

	public void InternalSetUseOnReleaseInsteadOfOnUp(bool state)
	{
		useOnReleaseInsteadOfOnUp = state;
	}
}
