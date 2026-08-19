using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class UIModelRender : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoFillStaticTexture_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public UIModelRender _003C_003E4__this;

		public UITexture texture;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoFillStaticTexture_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			UIModelRender uIModelRender = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForEndOfFrame();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (uIModelRender._staticTexture == null)
				{
					int defaultRenderTargetSize = Platform.Instance.DefaultRenderTargetSize;
					uIModelRender._staticTexture = RenderTexture.GetTemporary(defaultRenderTargetSize, defaultRenderTargetSize, 0);
				}
				Graphics.Blit(uIModelRender._texture, uIModelRender._staticTexture);
				texture.mainTexture = uIModelRender._staticTexture;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const float DefaultZoom = 200f;

	private const float MaxZoomSize = 400f;

	private const float MinZoomSize = 80f;

	public const float CameraRotation = 50f;

	private GameObject _modelObject;

	private RenderTexture _texture;

	private RenderTexture _staticTexture;

	private Camera _camera;

	public Transform ModelTransform => _modelObject.transform;

	protected void Awake()
	{
		_camera = GetComponentInChildren<Camera>();
	}

	private void OnEnable()
	{
		int defaultRenderTargetSize = Platform.Instance.DefaultRenderTargetSize;
		_texture = RenderTexture.GetTemporary(defaultRenderTargetSize, defaultRenderTargetSize, 24);
		_camera.targetTexture = _texture;
		_camera.orthographicSize = 200f;
	}

	private void OnDisable()
	{
		RenderTexture.ReleaseTemporary(_texture);
		_camera.targetTexture = null;
		_camera.orthographicSize = 200f;
		SetModelObject(null);
		if (_staticTexture != null)
		{
			RenderTexture.ReleaseTemporary(_staticTexture);
			_staticTexture = null;
		}
	}

	public void SetModel(GameObject obj, float cameraAngle, float modelScale = 1f, Bounds? bounds = null, float yPivot = 0f)
	{
		SetModelObject(obj);
		if (obj == null)
		{
			return;
		}
		if (!bounds.HasValue)
		{
			Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
			float num = 0f;
			int i = 0;
			for (int size = KUtility.GetSize(componentsInChildren); i < size; i++)
			{
				if (!componentsInChildren[i].enabled)
				{
					continue;
				}
				if (componentsInChildren[i] is ParticleSystemRenderer || componentsInChildren[i] is TrailRenderer)
				{
					componentsInChildren[i].enabled = false;
					continue;
				}
				Bounds bounds2 = componentsInChildren[i].bounds;
				float sqrMagnitude = bounds2.extents.sqrMagnitude;
				if (sqrMagnitude > num)
				{
					num = sqrMagnitude;
					bounds = bounds2;
				}
			}
		}
		if (!bounds.HasValue)
		{
			bounds = new Bounds(Vector3.zero, Vector3.one * _camera.orthographicSize);
		}
		float num2 = _camera.orthographicSize * 2f * 0.8f;
		Vector3 size2 = bounds.Value.size;
		float num3 = Mathf.Max(size2.x, Mathf.Max(size2.y, size2.z));
		float num4 = num2 / num3 * modelScale;
		_modelObject.transform.localScale *= num4;
		Transform transform = _camera.transform;
		transform.localEulerAngles = new Vector3(cameraAngle, 50f, 0f);
		Vector3 vector = new Vector3(0f, Mathf.Lerp(num2 * 0.5f, (0f - num2) * 0.5f + size2.y * num4, yPivot), 0f);
		transform.localPosition = vector - transform.forward * 400f;
	}

	private void SetModelObject(GameObject obj)
	{
		if (_modelObject != null)
		{
			UnityEngine.Object.Destroy(_modelObject);
		}
		if (!(obj == null))
		{
			_modelObject = new GameObject("TargetModel");
			_modelObject.transform.parent = base.transform;
			obj.transform.parent = _modelObject.transform;
			obj.transform.localPosition = Vector3.zero;
			_modelObject.transform.localPosition = Vector3.zero;
			NGUITools.SetLayer(_modelObject, base.gameObject.layer);
		}
	}

	public void FillTexture([NotNull] UITexture texture)
	{
		texture.mainTexture = _texture;
	}

	public void FillStaticTexture([NotNull] UITexture texture)
	{
		texture.mainTexture = null;
		StartCoroutine(CoFillStaticTexture(texture));
	}

	private IEnumerator CoFillStaticTexture([NotNull] UITexture texture)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFillStaticTexture_003Ed__17(0)
		{
			_003C_003E4__this = this,
			texture = texture
		};
	}

	public void Zoom(float zoomDelta, Vector2 center)
	{
		if (!(_modelObject == null))
		{
			_camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - zoomDelta * 5f, 80f, 400f);
			Vector3 localPosition = _modelObject.transform.localPosition;
			SetModelPosition(localPosition.x, localPosition.z);
		}
	}

	public void Panning(Vector3 gesturePosition)
	{
		if (!(_modelObject == null))
		{
			Vector3 localPosition = _modelObject.transform.localPosition;
			float orthographicSize = _camera.orthographicSize;
			float t = 1f - (400f - orthographicSize) / 320f;
			float num = Mathf.Lerp(0.007f, 0.035f, t);
			Vector3 vector = new Vector3(Mathf.Cos(-0.87266463f), 0f, Mathf.Sin(-0.87266463f));
			Vector3 vector2 = gesturePosition.x * num * vector;
			Vector3 vector3 = gesturePosition.y * num * new Vector3(0f - vector.z, 0f, vector.x);
			localPosition += vector2 + vector3;
			SetModelPosition(localPosition.x, localPosition.z);
		}
	}

	private void SetModelPosition(float x, float z)
	{
		x = Mathf.Clamp(x, -5f, 5f);
		z = Mathf.Clamp(z, -5f, 5f);
		_modelObject.transform.localPosition = new Vector3(x, 0f, z);
	}
}
