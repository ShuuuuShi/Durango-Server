using UnityEngine;

[ExecuteInEditMode]
public class AnimatedWidget : MonoBehaviour
{
	public float width = 1f;

	public float height = 1f;

	private UIWidget mWidget;

	private void OnEnable()
	{
		mWidget = ((Component)this).GetComponent<UIWidget>();
		LateUpdate();
	}

	private void LateUpdate()
	{
		if ((Object)(object)mWidget != (Object)null)
		{
			mWidget.width = Mathf.RoundToInt(width);
			mWidget.height = Mathf.RoundToInt(height);
		}
	}
}
