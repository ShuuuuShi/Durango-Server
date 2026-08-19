using System;
using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

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

	private static WaitForSeconds _wairForSeconds = new WaitForSeconds(0.3f);

	private static WaitForSeconds _wairForSecondsRapid = new WaitForSeconds(0.016f);

	public GameObject Target { get; set; }

	public bool IsRapidUpdateMode { get; set; }

	public bool DestroyIfInvisible { get; set; }

	private IEnumerator Start()
	{
		if (PlayerBehavior.LocalPlayer.gameObject == base.gameObject)
		{
			while (PlayerBehavior.LocalPlayer == null || !PlayerBehavior.LocalPlayer.gameObject.activeSelf)
			{
				yield return new WaitForSeconds(0.1f);
			}
			Target = PlayerBehavior.LocalPlayer.gameObject;
		}
		if (Target == null)
		{
			yield break;
		}
		Transform leftFoot = KUtility.FindTransformByName(Target, "Bip001_L_Foot");
		Transform rightFoot = KUtility.FindTransformByName(Target, "Bip001_R_Foot");
		CharacterBehavior character = Target.GetComponent<CharacterBehavior>();
		while (Target != null && Target.activeInHierarchy && character != null)
		{
			if (!character.WillBeRendered)
			{
				yield return null;
			}
			if (leftFoot == null || rightFoot == null || Target == null || (DestroyIfInvisible && Target.transform.position.y > ShadowRemoveHeight))
			{
				if (OnRemove != null)
				{
					OnRemove(this);
				}
				yield break;
			}
			float y = Mathf.Max(0f, Target.transform.position.y);
			Vector3 leftFootPos = CalcContactPosition(leftFoot.position);
			leftFootPos.y = y;
			Vector3 leftFootGroundPos = Maths.Make2D(leftFoot.position);
			Vector3 rightFootPos = CalcContactPosition(rightFoot.position);
			rightFootPos.y = y;
			Vector3 rightFootGroundPos = Maths.Make2D(rightFoot.position);
			Vector3 origin = (leftFootGroundPos + rightFootGroundPos) * 0.5f;
			origin.y = y;
			base.gameObject.transform.position = origin;
			_leftFootShadow.transform.position = leftFootPos + FootShadowOffset;
			float lFootYaw = _leftFootShadow.transform.localPosition.x * FootShadowRotRatioLeft + FootShadowRotBiasLeft;
			_leftFootShadow.transform.localRotation = Quaternion.Euler(0f, lFootYaw, 0f);
			_rightFootShadow.transform.position = rightFootPos + FootShadowOffset;
			float rFootYaw = _rightFootShadow.transform.localPosition.x * FootShadowRotRatioRight + FootShadowRotBiasRight;
			_rightFootShadow.transform.localRotation = Quaternion.Euler(0f, rFootYaw, 0f);
			_centerShadow.transform.position = (leftFootGroundPos + rightFootGroundPos) * 0.5f + CenterShadowOffset;
			_centerShadow.transform.localRotation = Quaternion.Euler(CenterShadowRot);
			if (IsRapidUpdateMode || (leftFootPos - leftFoot.position).sqrMagnitude > 5f)
			{
				yield return _wairForSecondsRapid;
			}
			else
			{
				yield return _wairForSeconds;
			}
		}
		if (OnRemove != null)
		{
			OnRemove(this);
		}
	}

	private static Vector3 CalcContactPosition(Vector3 pos)
	{
		return pos;
	}
}
