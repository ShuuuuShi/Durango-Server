using UnityEngine;

public class OpenURLOnClick : MonoBehaviour
{
	private void OnClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		UILabel component = ((Component)this).GetComponent<UILabel>();
		if ((Object)(object)component != (Object)null)
		{
			string urlAtPosition = component.GetUrlAtPosition(UICamera.lastWorldPosition);
			if (!string.IsNullOrEmpty(urlAtPosition))
			{
				Application.OpenURL(urlAtPosition);
			}
		}
	}
}
