using Durango.Logic.Interactions;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI;

public class TargetFloatingNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UISprite _arrow;

	private UIPanel _panel;

	private readonly TargetPosition _target = new TargetPosition();

	private Vector3 _offset;

	private float _enableAt;

	public string Key { get; private set; }

	public TargetPosition Target => _target;

	private void OnEnable()
	{
		_enableAt = Time.time;
	}

	public void Initialize()
	{
		_panel = GetComponent<UIPanel>();
	}

	public void Make(string key)
	{
		Key = key;
	}

	public void Release()
	{
		_target.Reset();
		_offset = Vector3.zero;
	}

	public void UpdateTick()
	{
		UpdatePosition();
		UpdateAlpha();
	}

	private void UpdatePosition()
	{
		if (_target.TryGet(out var pos))
		{
			pos += _offset;
			Vector3 localPosition = MainCamera.WorldToNGUIPos(pos);
			Vector3 vector = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			if (localPosition.sqrMagnitude < vector.sqrMagnitude)
			{
				base.gameObject.SetActive(value: true);
				base.transform.localPosition = localPosition;
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateAlpha()
	{
		float value = Time.time - _enableAt;
		_panel.alpha = Mathf.Clamp01(value);
	}

	public void SetDepth(int depth)
	{
		_panel.depth = depth;
	}

	public bool IsValid()
	{
		Vector3 pos;
		return _target.TryGet(out pos);
	}

	public TargetFloatingNode SetIcon(string icon)
	{
		_icon.spriteName = icon;
		return this;
	}

	public TargetFloatingNode SetIconColor(Color col)
	{
		_icon.color = col;
		return this;
	}

	public TargetFloatingNode SetBorderColor(Color col)
	{
		_border.color = col;
		_arrow.color = col;
		return this;
	}

	public TargetFloatingNode SetOffset(Vector3 offset)
	{
		_offset = offset;
		return this;
	}
}
