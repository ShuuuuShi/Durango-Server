using System;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCapture : KSingleton<ScreenCapture>
{
	public enum ToneEnum
	{
		Normal,
		Sepia,
		Grayscale
	}

	[Flags]
	public enum EffectEnum
	{
		Contrast = 1,
		Tone = 2,
		Tilt = 4
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

	private Matrix4x4 _normalMatrix = Matrix4x4.identity;

	private Matrix4x4 _sepiaMatrix = default(Matrix4x4);

	private Matrix4x4 _grayscaleMatrix = default(Matrix4x4);

	private Queue<CaptureOption> _captureQueue = new Queue<CaptureOption>();

	protected override void OnAwake()
	{
		InitToneMatrices();
	}

	public static void Capture(CaptureOption option)
	{
		KSingleton<ScreenCapture>.Instance()._captureQueue.Enqueue(option);
		((Behaviour)KSingleton<ScreenCapture>.Instance()).enabled = true;
	}

	private void OnPreRender()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (_captureQueue.Count == 0)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		RenderTexture val = new RenderTexture(Screen.width, Screen.height, 0);
		((Texture)val).filterMode = (FilterMode)0;
		while (_captureQueue.Count > 0)
		{
			CaptureOption captureOption = _captureQueue.Dequeue();
			Texture2D val2 = CaptureScreenshotToTexture(captureOption.NoUI);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = val;
			val.MarkRestoreExpected();
			_screenMaterial.mainTexture = (Texture)(object)val2;
			_screenMaterial.SetPass(0);
			DrawScreenQuad();
			if ((captureOption.Effect & EffectEnum.Contrast) != 0)
			{
				ApplyContrast();
			}
			if ((captureOption.Effect & EffectEnum.Tone) != 0)
			{
				ApplyToneEffect();
			}
			if ((captureOption.Effect & EffectEnum.Tilt) != 0)
			{
				ApplyTiltEffect();
			}
			if (captureOption.Logo)
			{
				DrawLogo();
			}
			val2.ReadPixels(new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), 0, 0);
			val2.Apply();
			RenderTexture.active = active;
			if (captureOption.OnResult != null)
			{
				captureOption.OnResult(val2);
			}
		}
		val.Release();
	}

	private void InitToneMatrices()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		((Matrix4x4)(ref _sepiaMatrix)).SetRow(0, new Vector4(0.393f, 0.769f, 0.189f, 0f));
		((Matrix4x4)(ref _sepiaMatrix)).SetRow(1, new Vector4(0.349f, 0.686f, 0.168f, 0f));
		((Matrix4x4)(ref _sepiaMatrix)).SetRow(2, new Vector4(0.272f, 0.534f, 0.131f, 0f));
		((Matrix4x4)(ref _grayscaleMatrix)).SetRow(0, new Vector4(0.21f, 0.72f, 0.07f, 0f));
		((Matrix4x4)(ref _grayscaleMatrix)).SetRow(1, new Vector4(0.21f, 0.72f, 0.07f, 0f));
		((Matrix4x4)(ref _grayscaleMatrix)).SetRow(2, new Vector4(0.21f, 0.72f, 0.07f, 0f));
	}

	private Matrix4x4 GetToneMatrix()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return (Matrix4x4)(_toneEnum switch
		{
			ToneEnum.Sepia => _sepiaMatrix, 
			ToneEnum.Grayscale => _grayscaleMatrix, 
			_ => _normalMatrix, 
		});
	}

	private void ApplyContrast()
	{
		if ((Object)(object)_contrastAdjustmentMaterial != (Object)null)
		{
			_contrastAdjustmentMaterial.SetPass(0);
			DrawScreenQuad();
			_contrastAdjustmentMaterial.SetPass(1);
			DrawScreenQuad();
		}
	}

	private void ApplyToneEffect()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
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
			if (!((Object)(object)texture == (Object)null))
			{
				_logoMaterial.mainTexture = texture;
				_logoMaterial.SetPass(0);
				int num2 = ((_localizeLogos[num].Width <= 0) ? texture.width : _localizeLogos[num].Width);
				int num3 = ((_localizeLogos[num].Height <= 0) ? texture.height : _localizeLogos[num].Height);
				Rect val = default(Rect);
				((Rect)(ref val))._002Ector(20f, 20f, (float)num2, (float)num3);
				val = UIUtility.DivideRect(val, Screen.width, Screen.height);
				DrawScreenQuad(new Rect(0f, 0f, 1f, 1f), val);
			}
		}
	}

	private void DrawScreenQuad()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		DrawScreenQuad(new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 1f, 1f));
	}

	private void DrawScreenQuad(Rect uv, Rect vert)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(((Rect)(ref uv)).x, ((Rect)(ref uv)).y, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).x, ((Rect)(ref vert)).y, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).xMax, ((Rect)(ref uv)).y, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).xMax, ((Rect)(ref vert)).y, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).xMax, ((Rect)(ref uv)).yMax, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).xMax, ((Rect)(ref vert)).yMax, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).x, ((Rect)(ref uv)).yMax, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).x, ((Rect)(ref vert)).yMax, 0f));
		GL.End();
		GL.PopMatrix();
	}

	private Texture2D CaptureScreenshotToTexture(bool noUI)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (noUI)
		{
			Camera component = ((Component)KSingleton<OverlayCamera>.Instance()).GetComponent<Camera>();
			RenderTexture targetTexture = component.targetTexture;
			Texture2D val = new Texture2D(targetTexture.width, targetTexture.height, (TextureFormat)3, false);
			((Texture)val).filterMode = (FilterMode)0;
			((Texture)val).wrapMode = (TextureWrapMode)1;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = targetTexture;
			val.ReadPixels(new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), 0, 0);
			val.Apply();
			RenderTexture.active = active;
			return val;
		}
		Texture2D val2 = new Texture2D(Screen.width, Screen.height, (TextureFormat)3, false);
		((Texture)val2).filterMode = (FilterMode)0;
		((Texture)val2).wrapMode = (TextureWrapMode)1;
		val2.ReadPixels(new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), 0, 0);
		val2.Apply();
		return val2;
	}
}
