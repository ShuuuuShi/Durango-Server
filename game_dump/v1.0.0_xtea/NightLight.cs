using UnityEngine;

public class NightLight : MonoBehaviour
{
	private enum Axis
	{
		None,
		Forward,
		Right,
		Up
	}

	[SerializeField]
	private float _lightScale = 1f;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private Axis _axis;

	private bool _disposed;

	private bool _isLightOn;

	private GameObject _lightMask;

	[ExposedInEditor(null)]
	public bool IsLightOn
	{
		get
		{
			return _isLightOn;
		}
		set
		{
			if (_isLightOn != value)
			{
				_isLightOn = value;
				if (_isLightOn)
				{
					AddLight();
				}
				else
				{
					RemoveLight();
				}
			}
		}
	}

	private void LateUpdate()
	{
		if (_axis != 0 && IsLightOn && IsVisible() && (Object)(object)_lightMask != (Object)null)
		{
			UpdateLightRotation();
		}
	}

	private void UpdateLightRotation()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Vector3)(_axis switch
		{
			Axis.Forward => ((Component)this).transform.forward, 
			Axis.Right => ((Component)this).transform.right, 
			Axis.Up => ((Component)this).transform.up, 
			_ => ((Component)this).transform.right, 
		});
		_lightMask.transform.rotation = Quaternion.LookRotation(Vector3.up, val);
	}

	private void OnDestroy()
	{
		IsLightOn = false;
		_disposed = true;
	}

	private void OnEnable()
	{
		IsLightOn = true;
	}

	private void OnDisable()
	{
		IsLightOn = false;
	}

	private void MakeLight()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		GameObject lightMaskPrefab = KSingleton<NightLightGrid>.Instance().LightMaskPrefab;
		_lightMask = ((Component)this).gameObject.AddChild(lightMaskPrefab);
		_lightMask.layer = lightMaskPrefab.layer;
		_lightMask.transform.localScale = Vector3.one * _lightScale * 200f;
		UpdateLightRotation();
		if ((Object)(object)_material != (Object)null)
		{
			MeshRenderer component = _lightMask.GetComponent<MeshRenderer>();
			((Renderer)component).sharedMaterial = _material;
		}
	}

	private void AddLight()
	{
		if (!_disposed)
		{
			MakeLight();
			KSingleton<NightLightGrid>.Instance().AddNightLight(this);
		}
	}

	private void RemoveLight()
	{
		if (!_disposed)
		{
			if (KSingleton<NightLightGrid>.HasInstance())
			{
				KSingleton<NightLightGrid>.Instance().RemoveNightLight(this);
			}
			Object.Destroy((Object)(object)_lightMask);
			_lightMask = null;
		}
	}

	public bool IsVisible()
	{
		return (Object)(object)_renderer == (Object)null || _renderer.isVisible;
	}
}
