using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Envelop Content")]
[RequireComponent(typeof(UIWidget))]
public class EnvelopContent : MonoBehaviour
{
	public Transform targetRoot;

	public int padLeft;

	public int padRight;

	public int padBottom;

	public int padTop;

	private bool mStarted;

	private void Start()
	{
		mStarted = true;
		Execute();
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			Execute();
		}
	}

	[ContextMenu("Execute")]
	public void Execute()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetRoot == (Object)(object)((Component)this).transform)
		{
			Debug.LogError((object)"Target Root object cannot be the same object that has Envelop Content. Make it a sibling instead.", (Object)(object)this);
			return;
		}
		if (NGUITools.IsChild(targetRoot, ((Component)this).transform))
		{
			Debug.LogError((object)"Target Root object should not be a parent of Envelop Content. Make it a sibling instead.", (Object)(object)this);
			return;
		}
		Bounds val = NGUIMath.CalculateRelativeWidgetBounds(((Component)this).transform.parent, targetRoot, considerInactive: false);
		float num = ((Bounds)(ref val)).min.x + (float)padLeft;
		float num2 = ((Bounds)(ref val)).min.y + (float)padBottom;
		float num3 = ((Bounds)(ref val)).max.x + (float)padRight;
		float num4 = ((Bounds)(ref val)).max.y + (float)padTop;
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		component.SetRect(num, num2, num3 - num, num4 - num2);
		((Component)this).BroadcastMessage("UpdateAnchors", (SendMessageOptions)1);
	}
}
