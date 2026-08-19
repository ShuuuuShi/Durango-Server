using UnityEngine;

[AddComponentMenu("NGUI/Tween/Spring Position")]
public class SpringPosition : MonoBehaviour
{
	public delegate void OnFinished();

	public static SpringPosition current;

	public Vector3 target = Vector3.zero;

	public float strength = 10f;

	public bool worldSpace;

	public bool ignoreTimeScale;

	public bool updateScrollView;

	public OnFinished onFinished;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	public string callWhenFinished;

	private Transform mTrans;

	private float mThreshold;

	private UIScrollView mSv;

	private void Start()
	{
		mTrans = ((Component)this).transform;
		if (updateScrollView)
		{
			mSv = NGUITools.FindInParents<UIScrollView>(((Component)this).gameObject);
		}
	}

	private void Update()
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = ((!ignoreTimeScale) ? Time.deltaTime : RealTime.deltaTime);
		if (worldSpace)
		{
			if (mThreshold == 0f)
			{
				Vector3 val = target - mTrans.position;
				mThreshold = ((Vector3)(ref val)).sqrMagnitude * 0.001f;
			}
			mTrans.position = NGUIMath.SpringLerp(mTrans.position, target, strength, deltaTime);
			float num = mThreshold;
			Vector3 val2 = target - mTrans.position;
			if (num >= ((Vector3)(ref val2)).sqrMagnitude)
			{
				mTrans.position = target;
				NotifyListeners();
				((Behaviour)this).enabled = false;
			}
		}
		else
		{
			if (mThreshold == 0f)
			{
				Vector3 val3 = target - mTrans.localPosition;
				mThreshold = ((Vector3)(ref val3)).sqrMagnitude * 1E-05f;
			}
			mTrans.localPosition = NGUIMath.SpringLerp(mTrans.localPosition, target, strength, deltaTime);
			float num2 = mThreshold;
			Vector3 val4 = target - mTrans.localPosition;
			if (num2 >= ((Vector3)(ref val4)).sqrMagnitude)
			{
				mTrans.localPosition = target;
				NotifyListeners();
				((Behaviour)this).enabled = false;
			}
		}
		if ((Object)(object)mSv != (Object)null)
		{
			mSv.UpdateScrollbars(recalculateBounds: true);
		}
	}

	private void NotifyListeners()
	{
		current = this;
		if (onFinished != null)
		{
			onFinished();
		}
		if ((Object)(object)eventReceiver != (Object)null && !string.IsNullOrEmpty(callWhenFinished))
		{
			eventReceiver.SendMessage(callWhenFinished, (object)this, (SendMessageOptions)1);
		}
		current = null;
	}

	public static SpringPosition Begin(GameObject go, Vector3 pos, float strength)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		SpringPosition springPosition = go.GetComponent<SpringPosition>();
		if ((Object)(object)springPosition == (Object)null)
		{
			springPosition = go.AddComponent<SpringPosition>();
		}
		springPosition.target = pos;
		springPosition.strength = strength;
		springPosition.onFinished = null;
		if (!((Behaviour)springPosition).enabled)
		{
			springPosition.mThreshold = 0f;
			((Behaviour)springPosition).enabled = true;
		}
		return springPosition;
	}
}
