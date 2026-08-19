using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Click")]
public class UICenterOnClick : MonoBehaviour
{
	private void OnClick()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		UICenterOnChild uICenterOnChild = NGUITools.FindInParents<UICenterOnChild>(((Component)this).gameObject);
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(((Component)this).gameObject);
		if ((Object)(object)uICenterOnChild != (Object)null)
		{
			if (((Behaviour)uICenterOnChild).enabled)
			{
				uICenterOnChild.CenterOn(((Component)this).transform);
			}
		}
		else if ((Object)(object)uIPanel != (Object)null && uIPanel.clipping != 0)
		{
			UIScrollView component = ((Component)uIPanel).GetComponent<UIScrollView>();
			Vector3 pos = -uIPanel.cachedTransform.InverseTransformPoint(((Component)this).transform.position);
			if (!component.canMoveHorizontally)
			{
				pos.x = uIPanel.cachedTransform.localPosition.x;
			}
			if (!component.canMoveVertically)
			{
				pos.y = uIPanel.cachedTransform.localPosition.y;
			}
			SpringPanel.Begin(uIPanel.cachedGameObject, pos, 6f);
		}
	}
}
