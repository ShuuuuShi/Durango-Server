using Ticket;
using UnityEngine;

public class TicketTierHelpWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _tierList;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public void Set(TierMeta[] metas)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		_tierList.Set(KUtility.GetSize(metas));
		int i = 0;
		for (int count = _tierList.Count; i < count; i++)
		{
			TicketTierHelpItem component = _tierList[i].GetComponent<TicketTierHelpItem>();
			component.Set(metas[count - 1 - i], i < _tierList.Count - 1);
		}
		float num = _tierList.Reposition(Vector3.down);
		Widget.height = Mathf.CeilToInt(num);
	}
}
