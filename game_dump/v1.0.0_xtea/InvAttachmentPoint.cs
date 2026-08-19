using UnityEngine;

[AddComponentMenu("NGUI/Examples/Item Attachment Point")]
public class InvAttachmentPoint : MonoBehaviour
{
	public InvBaseItem.Slot slot;

	private GameObject mPrefab;

	private GameObject mChild;

	public GameObject Attach(GameObject prefab)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mPrefab != (Object)(object)prefab)
		{
			mPrefab = prefab;
			if ((Object)(object)mChild != (Object)null)
			{
				Object.Destroy((Object)(object)mChild);
			}
			if ((Object)(object)mPrefab != (Object)null)
			{
				Transform transform = ((Component)this).transform;
				ref GameObject reference = ref mChild;
				Object obj = Object.Instantiate((Object)(object)mPrefab, transform.position, transform.rotation);
				reference = (GameObject)(object)((obj is GameObject) ? obj : null);
				Transform transform2 = mChild.transform;
				transform2.parent = transform;
				transform2.localPosition = Vector3.zero;
				transform2.localRotation = Quaternion.identity;
				transform2.localScale = Vector3.one;
			}
		}
		return mChild;
	}
}
