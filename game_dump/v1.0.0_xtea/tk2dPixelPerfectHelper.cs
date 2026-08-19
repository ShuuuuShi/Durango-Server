using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Deprecated/Extra/tk2dPixelPerfectHelper")]
public class tk2dPixelPerfectHelper : MonoBehaviour
{
	private static tk2dPixelPerfectHelper _inst;

	[NonSerialized]
	public Camera cam;

	public int collectionTargetHeight = 640;

	public float collectionOrthoSize = 1f;

	public float targetResolutionHeight = 640f;

	[NonSerialized]
	public float scaleD;

	[NonSerialized]
	public float scaleK;

	public static tk2dPixelPerfectHelper inst
	{
		get
		{
			if ((Object)(object)_inst == (Object)null)
			{
				_inst = Object.FindObjectOfType(typeof(tk2dPixelPerfectHelper)) as tk2dPixelPerfectHelper;
				if ((Object)(object)_inst == (Object)null)
				{
					return null;
				}
				inst.Setup();
			}
			return _inst;
		}
	}

	public bool CameraIsOrtho => cam.orthographic;

	private void Awake()
	{
		Setup();
		_inst = this;
	}

	public virtual void Setup()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)collectionTargetHeight / targetResolutionHeight;
		if ((Object)(object)((Component)this).GetComponent<Camera>() != (Object)null)
		{
			cam = ((Component)this).GetComponent<Camera>();
		}
		if ((Object)(object)cam == (Object)null)
		{
			cam = Camera.main;
		}
		if (cam.orthographic)
		{
			scaleK = num * cam.orthographicSize / collectionOrthoSize;
			scaleD = 0f;
		}
		else
		{
			float num2 = num * Mathf.Tan((float)Math.PI / 180f * cam.fieldOfView * 0.5f) / collectionOrthoSize;
			scaleK = num2 * (0f - ((Component)cam).transform.position.z);
			scaleD = num2;
		}
	}

	public static float CalculateScaleForPerspectiveCamera(float fov, float zdist)
	{
		return Mathf.Abs(Mathf.Tan((float)Math.PI / 180f * fov * 0.5f) * zdist);
	}
}
