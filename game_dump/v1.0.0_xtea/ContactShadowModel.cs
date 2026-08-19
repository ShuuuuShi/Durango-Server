using System;
using System.Collections;
using UnityEngine;

public class ContactShadowModel : MonoBehaviour
{
	public Action<ContactShadowModel> OnRemove;

	[SerializeField]
	public Vector3 FootShadowOffset;

	[SerializeField]
	public float FootShadowRotBiasLeft;

	[SerializeField]
	public float FootShadowRotRatioLeft;

	[SerializeField]
	public float FootShadowRotBiasRight;

	[SerializeField]
	public float FootShadowRotRatioRight;

	[SerializeField]
	public Vector3 CenterShadowOffset;

	[SerializeField]
	public Vector3 CenterShadowRot;

	[SerializeField]
	public float ShadowRemoveHeight;

	[SerializeField]
	private GameObject _leftFootShadow;

	[SerializeField]
	private GameObject _rightFootShadow;

	[SerializeField]
	private GameObject _centerShadow;

	public GameObject Target { get; set; }

	public bool IsRapidUpdateMode { get; set; }

	public bool DestroyIfInvisible { get; set; }

	private IEnumerator Start()
	{
		if ((Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject == (Object)(object)((Component)this).gameObject)
		{
			while ((Object)(object)PlayerBehavior.LocalPlayer == (Object)null || !((Component)PlayerBehavior.LocalPlayer).gameObject.activeSelf)
			{
				yield return (object)new WaitForSeconds(0.1f);
			}
			Target = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		}
		if ((Object)(object)Target == (Object)null)
		{
			yield break;
		}
		Transform leftFoot = KUtility.FindTransformByName(Target, "Bip001_L_Foot");
		Transform rightFoot = KUtility.FindTransformByName(Target, "Bip001_R_Foot");
		CharacterBehavior chracter = Target.GetComponent<CharacterBehavior>();
		while (!IsDestroyed<GameObject>(Target) && Target.activeInHierarchy)
		{
			if (!chracter.IsVisible)
			{
				yield return (object)new WaitForSeconds(0.5f);
			}
			if (DestroyIfInvisible && Target.transform.position.y > ShadowRemoveHeight)
			{
				if (OnRemove != null)
				{
					OnRemove(this);
				}
				yield break;
			}
			if (IsDestroyed<Transform>(leftFoot) || IsDestroyed<Transform>(rightFoot))
			{
				if (OnRemove != null)
				{
					OnRemove(this);
				}
				yield break;
			}
			Vector3 leftFootPos = CalcContactPosition(leftFoot.position);
			Vector3 leftFootGroundPos = KMathUtil.Make2D(leftFoot.position);
			Vector3 rightFootPos = CalcContactPosition(rightFoot.position);
			Vector3 rightFootGroundPos = KMathUtil.Make2D(rightFoot.position);
			Vector3 origin = (leftFootGroundPos + rightFootGroundPos) * 0.5f;
			((Component)this).gameObject.transform.position = origin;
			_leftFootShadow.transform.position = leftFootPos + FootShadowOffset;
			float lFootYaw = _leftFootShadow.transform.localPosition.x * FootShadowRotRatioLeft + FootShadowRotBiasLeft;
			_leftFootShadow.transform.localRotation = Quaternion.Euler(0f, lFootYaw, 0f);
			_rightFootShadow.transform.position = rightFootPos + FootShadowOffset;
			float rFootYaw = _rightFootShadow.transform.localPosition.x * FootShadowRotRatioRight + FootShadowRotBiasRight;
			_rightFootShadow.transform.localRotation = Quaternion.Euler(0f, rFootYaw, 0f);
			_centerShadow.transform.position = (leftFootGroundPos + rightFootGroundPos) * 0.5f + CenterShadowOffset;
			_centerShadow.transform.localRotation = Quaternion.Euler(CenterShadowRot);
			float yieldTime = 0.3f;
			if (!IsRapidUpdateMode)
			{
				Vector3 val = leftFootPos - leftFoot.position;
				if (!(((Vector3)(ref val)).sqrMagnitude > 5f))
				{
					goto IL_046f;
				}
			}
			yieldTime = 0.016f;
			goto IL_046f;
			IL_046f:
			yield return (object)new WaitForSeconds(yieldTime);
		}
		if (OnRemove != null)
		{
			OnRemove(this);
		}
	}

	private bool IsDestroyed<T>(T obj) where T : class
	{
		return obj?.Equals(null) ?? true;
	}

	private Vector3 CalcContactPosition(Vector3 pos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		pos.y = 0f;
		return pos;
	}
}
