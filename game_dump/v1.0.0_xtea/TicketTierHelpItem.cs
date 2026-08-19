using L10N;
using Ticket;
using UnityEngine;

public class TicketTierHelpItem : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ListObjectPool _tierNames;

	[SerializeField]
	private GameObject _lineSprite;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public void Set(TierMeta tier, bool activeSeparator)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		string text = T._(tier.Name);
		tier.Icon.Set(_iconSprite);
		int num = tier.MaxRank - tier.MinRank + 1;
		_tierNames.Set(num);
		for (int i = 0; i < num; i++)
		{
			UILabel component = _tierNames[i].GetComponent<UILabel>();
			component.text = ((num <= 1) ? text : $"{text} {num - i}");
			Transform val = ((Component)component).transform.FindChild("separator");
			((Component)val).gameObject.SetActive(i < num - 1);
		}
		_lineSprite.gameObject.SetActive(activeSeparator);
		float num2 = _tierNames.Reposition(Vector3.down);
		Widget.height = Mathf.CeilToInt(num2);
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
