using Durango.Utils;
using UnityEngine;

namespace Durango.Environment;

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
	public float Radius = 600f;

	[SerializeField]
	private float _lightScale = 1f;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private Axis _axis;

	[SerializeField]
	private bool _skipLightMask;

	[SerializeField]
	private Vector3 _direction;

	[SerializeField]
	private bool _onlyForDay;

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

	public Vector3 Direction => _direction;

	private void Start()
	{
		if (_onlyForDay)
		{
			TimeGauge.IsSunUpChanged += TimeGuage_IsSunUpChanged;
		}
	}

	private void OnDestroy()
	{
		IsLightOn = false;
		if (_onlyForDay)
		{
			TimeGauge.IsSunUpChanged -= TimeGuage_IsSunUpChanged;
		}
	}

	private void OnEnable()
	{
		IsLightOn = !_onlyForDay || TimeGauge.IsSunUp;
	}

	private void OnDisable()
	{
		IsLightOn = false;
	}

	private void TimeGuage_IsSunUpChanged()
	{
		IsLightOn = TimeGauge.IsSunUp;
	}

	public void Process()
	{
		if (_axis != 0 && IsLightOn && IsVisible() && _lightMask != null)
		{
			UpdateLightRotation();
		}
	}

	private void UpdateLightRotation()
	{
		Vector3 upwards = _axis switch
		{
			Axis.Forward => base.transform.forward, 
			Axis.Right => base.transform.right, 
			Axis.Up => base.transform.up, 
			_ => base.transform.right, 
		};
		_lightMask.transform.rotation = Quaternion.LookRotation(Vector3.up, upwards);
	}

	private void MakeLightMask()
	{
		if (_lightMask == null)
		{
			GameObject lightMaskPrefab = Singleton<NightLightGrid>.Instance().LightMaskPrefab;
			_lightMask = base.gameObject.AddChild(lightMaskPrefab);
			_lightMask.layer = lightMaskPrefab.layer;
			_lightMask.transform.localScale = Vector3.one * _lightScale * 200f;
		}
		UpdateLightRotation();
		if (_material != null)
		{
			MeshRenderer component = _lightMask.GetComponent<MeshRenderer>();
			component.sharedMaterial = _material;
		}
	}

	private void AddLight()
	{
		if (!_skipLightMask)
		{
			MakeLightMask();
		}
		Singleton<NightLightGrid>.Instance().AddNightLight(this);
	}

	private void RemoveLight()
	{
		if (_lightMask != null)
		{
			Object.Destroy(_lightMask);
			_lightMask = null;
		}
		if (Singleton<NightLightGrid>.HasInstance())
		{
			Singleton<NightLightGrid>.Instance().RemoveNightLight(this);
		}
	}

	public bool IsVisible()
	{
		return _renderer == null || _renderer.isVisible;
	}
}
