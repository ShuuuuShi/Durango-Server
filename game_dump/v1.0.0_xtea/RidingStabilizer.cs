using UnityEngine;

public class RidingStabilizer : MonoBehaviour
{
	[SerializeField]
	private float _cullDistance = 1000f;

	[SerializeField]
	private string _spineBoneName = "Bip001_Spine";

	private Transform _spine;

	[SerializeField]
	private float _spinePitchAtRiding = -120f;

	private CharacterBehavior _characterOwner;

	private void Start()
	{
		_characterOwner = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
		Init();
	}

	public void Init()
	{
		if ((Object)null == (Object)(object)_spine)
		{
			_spine = KUtility.FindTransformByName(((Component)_characterOwner).gameObject, _spineBoneName);
		}
	}

	public void SetMountHeadBone(Transform head)
	{
		_spine = head;
	}

	private void LateUpdate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_characterOwner == (Object)null) && !((Object)(object)PlayerBehavior.LocalPlayer == (Object)null) && _characterOwner.IsAlive && _characterOwner.IsVisible)
		{
			Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _characterOwner.CurrentPosition;
			if (!(((Vector3)(ref val)).magnitude > _cullDistance) && Object.op_Implicit((Object)(object)_spine))
			{
				Transform spine = _spine;
				Quaternion rotation = _spine.rotation;
				float x = ((Quaternion)(ref rotation)).eulerAngles.x;
				Quaternion rotation2 = _spine.rotation;
				spine.rotation = Quaternion.Euler(x, ((Quaternion)(ref rotation2)).eulerAngles.y, _spinePitchAtRiding);
			}
		}
	}
}
