using UnityEngine;

[ExecuteInEditMode]
public class AnimatedAlpha : MonoBehaviour
{
	[Range(0f, 1f)]
	public float alpha = 1f;

	private UIWidget mWidget;

	private UIPanel mPanel;

	private void OnEnable()
	{
		mWidget = ((Component)this).GetComponent<UIWidget>();
		mPanel = ((Component)this).GetComponent<UIPanel>();
		LateUpdate();
	}

	private void LateUpdate()
	{
		if ((Object)(object)mWidget != (Object)null)
		{
			mWidget.alpha = alpha;
		}
		if ((Object)(object)mPanel != (Object)null)
		{
			mPanel.alpha = alpha;
		}
	}
}
