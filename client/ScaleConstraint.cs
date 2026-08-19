using UnityEngine;

public class ScaleConstraint : MonoBehaviour
{
	[SerializeField]
	private float _ratio = 0.5625f;

	public void OnEnable()
	{
		if (!(_ratio >= 1f))
		{
			float num = (float)Screen.width / (float)Screen.height;
			Vector3 localScale = base.transform.localScale;
			localScale.x = (localScale.y = Mathf.Min(num / _ratio, 1f));
			base.transform.localScale = localScale;
		}
	}
}
