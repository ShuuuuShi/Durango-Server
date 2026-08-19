using System.Collections.Generic;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class TankController : MonoBehaviour
{
	public float TrailMaterialOffsetSpeed;

	public float MoveSpeed;

	public float MoveFriction;

	public float MoveAcceleration;

	public float RotateSpeed;

	public float RotateFriction;

	public float RotateAcceleration;

	public Material TrailMaterial;

	public Animator Animator;

	public List<Trail> TankTrackTrails;

	public TankWeaponController WeaponController;

	private float _moveSpeed;

	private float _rotateSpeed;

	public bool InControl;

	private void Update()
	{
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		Animator.SetBool("InControl", InControl);
		if (InControl)
		{
			((Behaviour)WeaponController).enabled = true;
			if (Input.GetKey((KeyCode)119))
			{
				Animator.SetBool("Forward", true);
				Animator.SetBool("Backward", false);
				_moveSpeed += MoveAcceleration * Time.deltaTime * 2f;
				if (_moveSpeed > MoveSpeed)
				{
					_moveSpeed = MoveSpeed;
				}
			}
			else if (Input.GetKey((KeyCode)115))
			{
				Animator.SetBool("Backward", true);
				Animator.SetBool("Forward", false);
				_moveSpeed -= MoveAcceleration * Time.deltaTime * 2f;
				if (_moveSpeed < 0f - MoveSpeed)
				{
					_moveSpeed = 0f - MoveSpeed;
				}
			}
			else
			{
				Animator.SetBool("Backward", false);
				Animator.SetBool("Forward", false);
			}
			if (Input.GetKey((KeyCode)100))
			{
				_rotateSpeed += RotateAcceleration * Time.deltaTime * 2f;
				if (_rotateSpeed > RotateSpeed)
				{
					_rotateSpeed = RotateSpeed;
				}
			}
			else if (Input.GetKey((KeyCode)97))
			{
				_rotateSpeed -= RotateAcceleration * Time.deltaTime * 2f;
				if (_rotateSpeed < 0f - RotateSpeed)
				{
					_rotateSpeed = 0f - RotateSpeed;
				}
			}
		}
		else
		{
			((Behaviour)WeaponController).enabled = false;
		}
		if (Mathf.Abs(_moveSpeed) > 0f)
		{
			TankTrackTrails.ForEach(delegate(Trail trail)
			{
				trail.Emit = true;
			});
		}
		else
		{
			TankTrackTrails.ForEach(delegate(Trail trail)
			{
				trail.Emit = false;
			});
		}
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.forward * _moveSpeed * Time.deltaTime;
		((Component)this).transform.RotateAround(((Component)this).transform.position, ((Component)this).transform.up, _rotateSpeed);
		TrailMaterial.mainTextureOffset = new Vector2(TrailMaterial.mainTextureOffset.x + Mathf.Sign(_moveSpeed) * Mathf.Lerp(0f, TrailMaterialOffsetSpeed, Mathf.Abs(_moveSpeed / MoveSpeed) + Mathf.Abs(_rotateSpeed / RotateSpeed)), TrailMaterial.mainTextureOffset.y);
		_moveSpeed = Mathf.MoveTowards(_moveSpeed, 0f, MoveFriction * Time.deltaTime);
		_rotateSpeed = Mathf.MoveTowards(_rotateSpeed, 0f, RotateFriction * Time.deltaTime);
	}
}
