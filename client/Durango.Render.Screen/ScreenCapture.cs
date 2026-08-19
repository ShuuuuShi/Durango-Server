using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Screen;

public class ScreenCapture : Singleton<ScreenCapture>
{
	[Flags]
	public enum EffectEnum
	{
		Contrast = 1,
		Tone = 2,
		Tilt = 4
	}

	public enum ToneEnum
	{
		Normal,
		Sepia,
		Grayscale
	}

	[Serializable]
	private struct LocalizeLogo
	{
		public string Locale;

		public Texture Texture;

		public int Width;

		public int Height;
	}

	public struct CaptureOption
	{
		public bool NoUI;

		public EffectEnum Effect;

		public bool Logo;

		public Action<Texture2D> OnResult;

		public bool NeedPostProcess()
		{
			return Logo || Effect != (EffectEnum)0;
		}
	}

	[SerializeField]
	private Material _screenMaterial;

	[SerializeField]
	private Material _tiltEffectMaterial;

	[SerializeField]
	private float _blurAmount = 2f;

	[SerializeField]
	private Material _toneEffectMaterial;

	[SerializeField]
	private Material _contrastAdjustmentMaterial;

	[SerializeField]
	private Material _logoMaterial;

	[SerializeField]
	private LocalizeLogo[] _localizeLogos;

	[SerializeField]
	private ToneEnum _toneEnum;

	private readonly Matrix4x4 _normalMatrix = Matrix4x4.identity;

	private Matrix4x4 _sepiaMatrix = default(Matrix4x4);

	private Matrix4x4 _grayscaleMatrix = default(Matrix4x4);

	private readonly Queue<CaptureOption> _captureQueue = new Queue<CaptureOption>();

	public static bool ApplyFilter { get; set; }

	protected override void OnAwake()
	{
		InitToneMatrices();
	}

	public static void Capture(CaptureOption option)
	{
		Singleton<ScreenCapture>.Instance()._captureQueue.Enqueue(option);
		Singleton<ScreenCapture>.Instance().enabled = true;
	}

	private void OnPostRender()
	{
		if (_captureQueue.Count == 0)
		{
			base.enabled = false;
			return;
		}
		while (_captureQueue.Count > 0)
		{
			CaptureOption op = _captureQueue.Dequeue();
			Texture2D texture2D = CaptureScreenshotToTexture(op.NoUI);
			if (op.NeedPostProcess() && ApplyFilter)
			{
				ApplyPostProcess(texture2D, op);
			}
			if (op.OnResult != null)
			{
				op.OnResult(texture2D);
			}
		}
	}

	private void ApplyPostProcess(Texture2D tex, CaptureOption op)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(UnityEngine.Screen.width, UnityEngine.Screen.height, 0);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		temporary.MarkRestoreExpected();
		_screenMaterial.mainTexture = tex;
		_screenMaterial.SetPass(0);
		DrawScreenQuad();
		if ((op.Effect & EffectEnum.Contrast) != 0)
		{
			ApplyContrast();
		}
		if ((op.Effect & EffectEnum.Tone) != 0)
		{
			ApplyToneEffect();
		}
		if ((op.Effect & EffectEnum.Tilt) != 0)
		{
			ApplyTiltEffect();
		}
		if (op.Logo)
		{
			DrawLogo();
		}
		tex.ReadPixels(new Rect(0f, 0f, tex.width, tex.height), 0, 0);
		tex.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		_screenMaterial.mainTexture = null;
	}

	private void InitToneMatrices()
	{
		_sepiaMatrix.SetRow(0, new Vector4(0.393f, 0.769f, 0.189f, 0f));
		_sepiaMatrix.SetRow(1, new Vector4(0.349f, 0.686f, 0.168f, 0f));
		_sepiaMatrix.SetRow(2, new Vector4(0.272f, 0.534f, 0.131f, 0f));
		_grayscaleMatrix.SetRow(0, new Vector4(0.21f, 0.72f, 0.07f, 0f));
		_grayscaleMatrix.SetRow(1, new Vector4(0.21f, 0.72f, 0.07f, 0f));
		_grayscaleMatrix.SetRow(2, new Vector4(0.21f, 0.72f, 0.07f, 0f));
	}

	private Matrix4x4 GetToneMatrix()
	{
		return _toneEnum switch
		{
			ToneEnum.Sepia => _sepiaMatrix, 
			ToneEnum.Grayscale => _grayscaleMatrix, 
			_ => _normalMatrix, 
		};
	}

	private void ApplyContrast()
	{
		if (_contrastAdjustmentMaterial != null)
		{
			_contrastAdjustmentMaterial.SetPass(0);
			DrawScreenQuad();
			_contrastAdjustmentMaterial.SetPass(1);
			DrawScreenQuad();
		}
	}

	private void ApplyToneEffect()
	{
		_toneEffectMaterial.SetPass(0);
		DrawScreenQuad();
		_toneEffectMaterial.SetMatrix("_ToneMatrix", GetToneMatrix());
		_toneEffectMaterial.SetPass(1);
		DrawScreenQuad();
	}

	private void ApplyTiltEffect()
	{
		_tiltEffectMaterial.SetPass(0);
		DrawScreenQuad();
		_tiltEffectMaterial.SetFloat("_BlurAmount", _blurAmount);
		_tiltEffectMaterial.SetPass(1);
		DrawScreenQuad();
		_tiltEffectMaterial.SetPass(2);
		DrawScreenQuad();
		_tiltEffectMaterial.SetFloat("_BlurAmount", _blurAmount);
		_tiltEffectMaterial.SetPass(3);
		DrawScreenQuad();
	}

	private void DrawLogo()
	{
		string locale = LocalizeSystem.Locale;
		int num = -1;
		for (int i = 0; i < _localizeLogos.Length; i++)
		{
			if (_localizeLogos[i].Locale == locale)
			{
				num = i;
				break;
			}
			if (string.IsNullOrEmpty(_localizeLogos[i].Locale))
			{
				num = i;
			}
		}
		if (num != -1)
		{
			Texture texture = _localizeLogos[num].Texture;
			if (!(texture == null))
			{
				_logoMaterial.mainTexture = texture;
				_logoMaterial.SetPass(0);
				int num2 = ((_localizeLogos[num].Width <= 0) ? texture.width : _localizeLogos[num].Width);
				int num3 = ((_localizeLogos[num].Height <= 0) ? texture.height : _localizeLogos[num].Height);
				Rect rect = new Rect(20f, 20f, num2, num3);
				rect = UIUtility.DivideRect(rect, UnityEngine.Screen.width, UnityEngine.Screen.height);
				DrawScreenQuad(new Rect(0f, 0f, 1f, 1f), rect);
			}
		}
	}

	private static void DrawScreenQuad()
	{
		DrawScreenQuad(new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 1f, 1f));
	}

	private static void DrawScreenQuad(Rect uv, Rect vert)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(uv.x, uv.y, 0f));
		GL.Vertex(new Vector3(vert.x, vert.y, 0f));
		GL.TexCoord(new Vector3(uv.xMax, uv.y, 0f));
		GL.Vertex(new Vector3(vert.xMax, vert.y, 0f));
		GL.TexCoord(new Vector3(uv.xMax, uv.yMax, 0f));
		GL.Vertex(new Vector3(vert.xMax, vert.yMax, 0f));
		GL.TexCoord(new Vector3(uv.x, uv.yMax, 0f));
		GL.Vertex(new Vector3(vert.x, vert.yMax, 0f));
		GL.End();
		GL.PopMatrix();
	}

	private static Texture2D CaptureScreenshotToTexture(bool noUI)
	{
		if (noUI)
		{
			UnityEngine.Camera component = Singleton<OverlayCamera>.Instance().GetComponent<UnityEngine.Camera>();
			RenderTexture targetTexture = component.targetTexture;
			Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, mipmap: false);
			texture2D.filterMode = FilterMode.Point;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = targetTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			return texture2D;
		}
		Texture2D texture2D2 = new Texture2D(UnityEngine.Screen.width, UnityEngine.Screen.height, TextureFormat.RGB24, mipmap: false);
		texture2D2.filterMode = FilterMode.Point;
		texture2D2.wrapMode = TextureWrapMode.Clamp;
		texture2D2.ReadPixels(new Rect(0f, 0f, texture2D2.width, texture2D2.height), 0, 0);
		texture2D2.Apply();
		return texture2D2;
	}
}
