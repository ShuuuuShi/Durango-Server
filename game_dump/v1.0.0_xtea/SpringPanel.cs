using UnityEngine;

[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Internal/Spring Panel")]
public class SpringPanel : MonoBehaviour
{
	public delegate void OnFinished();

	public static SpringPanel current;

	public Vector3 target = Vector3.zero;

	public float strength = 10f;

	public OnFinished onFinished;

	private UIPanel mPanel;

	private Transform mTrans;

	private UIScrollView mDrag;

	private void Start()
	{
		mPanel = ((Component)this).GetComponent<UIPanel>();
		mDrag = ((Component)this).GetComponent<UIScrollView>();
		mTrans = ((Component)this).transform;
	}

	private void Update()
	{
		AdvanceTowardsPosition();
	}

	protected virtual void AdvanceTowardsPosition()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = RealTime.deltaTime;
		bool flag = false;
		Vector3 localPosition = mTrans.localPosition;
		Vector3 val = NGUIMath.SpringLerp(mTrans.localPosition, target, strength, deltaTime);
		Vector3 val2 = val - target;
		if (((Vector3)(ref val2)).sqrMagnitude < 0.01f)
		{
			val = target;
			((Behaviour)this).enabled = false;
			flag = true;
		}
		mTrans.localPosition = val;
		Vector3 val3 = val - localPosition;
		Vector2 clipOffset = mPanel.clipOffset;
		clipOffset.x -= val3.x;
		clipOffset.y -= val3.y;
		mPanel.clipOffset = clipOffset;
		if ((Object)(object)mDrag != (Object)null)
		{
			mDrag.UpdateScrollbars(recalculateBounds: false);
		}
		if (flag && onFinished != null)
		{
			current = this;
			onFinished();
			current = null;
		}
	}

	public static SpringPanel Begin(GameObject go, Vector3 pos, float strength)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		SpringPanel springPanel = go.GetComponent<SpringPanel>();
		if ((Object)(object)springPanel == (Object)null)
		{
			springPanel = go.AddComponent<SpringPanel>();
		}
		springPanel.target = pos;
		springPanel.strength = strength;
		springPanel.onFinished = null;
		((Behaviour)springPanel).enabled = true;
		return springPanel;
	}
}
