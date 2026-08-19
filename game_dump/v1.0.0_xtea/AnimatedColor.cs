using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(UIWidget))]
public class AnimatedColor : MonoBehaviour
{
	public Color color = Color.white;

	private UIWidget mWidget;

	private void OnEnable()
	{
		mWidget = ((Component)this).GetComponent<UIWidget>();
		LateUpdate();
	}

	private void LateUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		mWidget.color = color;
	}
}
