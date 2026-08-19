using UnityEngine;

public class LoadingIndicatorWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _loading;

	[SerializeField]
	private UILabel _label;

	private void Update()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time % 2f / 2f;
		_loading.transform.localRotation = Quaternion.Euler(0f, 0f, (0f - num) * 360f);
	}

	public void SetExplainLabel(string text)
	{
		_label.text = text;
	}
}
