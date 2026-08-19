using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Screen;

public class OutlineScreen : Singleton<OutlineScreen>
{
	[CompilerGenerated]
	private sealed class _003CCoCircleEffect_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OutlineScreen _003C_003E4__this;

		public Vector2 center;

		private float _003Coffset_003E5__2;

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
		public _003CCoCircleEffect_003Ed__18(int _003C_003E1__state)
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
			OutlineScreen outlineScreen = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				if (outlineScreen._fadeOut + _003Coffset_003E5__2 >= outlineScreen._duration)
				{
					outlineScreen._enabled = false;
					outlineScreen._prevCircleCoroutine = null;
					return false;
				}
				_003Coffset_003E5__2 += Time.deltaTime * outlineScreen._speed;
			}
			else
			{
				_003C_003E1__state = -1;
				outlineScreen._enabled = true;
				_003Coffset_003E5__2 = 0f;
				outlineScreen._outlineMaterial.SetVector("_CircleCenter", center);
			}
			outlineScreen._outlineMaterial.SetFloat("_FadeInRadius", outlineScreen._fadeIn + _003Coffset_003E5__2);
			outlineScreen._outlineMaterial.SetFloat("_ColorRadius", outlineScreen._nextColor + _003Coffset_003E5__2);
			outlineScreen._outlineMaterial.SetFloat("_FadeOutRadius", outlineScreen._fadeOut + _003Coffset_003E5__2);
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
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

	[SerializeField]
	private Color _outlineColor1;

	[SerializeField]
	private Color _outlineColor2;

	[SerializeField]
	private float _outlineStrength;

	[SerializeField]
	private float _outlineSize;

	[SerializeField]
	private int _outlineIteration;

	[SerializeField]
	private LayerMask _outlineLayer;

	[SerializeField]
	private float _fadeIn;

	[SerializeField]
	private float _nextColor = -0.5f;

	[SerializeField]
	private float _fadeOut = -1f;

	[SerializeField]
	private float _duration = 2f;

	[SerializeField]
	private float _speed = 2f;

	private UnityEngine.Camera _mainCamera;

	private Material _outlineMaterial;

	private Material _blurMaterial;

	private Shader _colorizeShader;

	private bool _enabled;

	private Coroutine _prevCircleCoroutine;

	public void BeginCircleEffect(Vector2 screen)
	{
		Vector2 center = new Vector2(screen.x / (float)UnityEngine.Screen.width, screen.y / (float)UnityEngine.Screen.height);
		if (_prevCircleCoroutine != null)
		{
			StopCoroutine(_prevCircleCoroutine);
		}
		_prevCircleCoroutine = StartCoroutine(CoCircleEffect(center));
	}

	private IEnumerator CoCircleEffect(Vector2 center)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoCircleEffect_003Ed__18(0)
		{
			_003C_003E4__this = this,
			center = center
		};
	}

	protected override void OnAwake()
	{
		_outlineMaterial = new Material(Shader.Find("Hidden/OutlineScreen"));
		_blurMaterial = new Material(Shader.Find("Hidden/FastBlur"));
		_colorizeShader = Shader.Find("Hidden/OutlineColorize");
		_mainCamera = Singleton<MainCamera>.Instance().GetComponent<UnityEngine.Camera>();
	}

	private void OnPreRender()
	{
		if (_enabled)
		{
			RenderTexture targetTexture = _mainCamera.targetTexture;
			int cullingMask = _mainCamera.cullingMask;
			CameraClearFlags clearFlags = _mainCamera.clearFlags;
			Color backgroundColor = _mainCamera.backgroundColor;
			_mainCamera.cullingMask = _outlineLayer.value;
			_mainCamera.clearFlags = CameraClearFlags.Color;
			_mainCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			_mainCamera.RenderWithShader(_colorizeShader, "RenderType");
			_mainCamera.backgroundColor = backgroundColor;
			_mainCamera.clearFlags = clearFlags;
			_mainCamera.cullingMask = cullingMask;
			int width = targetTexture.width >> 1;
			int height = targetTexture.width >> 1;
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, targetTexture.format);
			Blur(targetTexture, temporary, 1);
			_outlineMaterial.SetColor("_OutlineCol1", _outlineColor1);
			_outlineMaterial.SetColor("_OutlineCol2", _outlineColor2);
			_outlineMaterial.SetFloat("_OutlineStrength", _outlineStrength);
			_outlineMaterial.SetTexture("_BlurTex", temporary);
			Graphics.Blit(targetTexture, null, _outlineMaterial);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private void Blur(RenderTexture input, RenderTexture output, int downSample)
	{
		Graphics.Blit(input, output, _blurMaterial, 0);
		float num = 1f / (1f * (float)(1 << downSample));
		for (int i = 0; i < _outlineIteration; i++)
		{
			float num2 = (float)i * 1f;
			_blurMaterial.SetVector("_Parameter", new Vector4(_outlineSize * num + num2, (0f - _outlineSize) * num - num2, 0f, 0f));
			RenderTexture temporary = RenderTexture.GetTemporary(input.width, input.height, 0, input.format);
			temporary.filterMode = FilterMode.Bilinear;
			Graphics.Blit(output, temporary, _blurMaterial, 1);
			output.DiscardContents();
			Graphics.Blit(temporary, output, _blurMaterial, 2);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
