using System.Collections;
using UnityEngine;

public class OutlineScreen : KSingleton<OutlineScreen>
{
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

	private Camera _mainCamera;

	private Material _outlineMaterial;

	private Material _blurMaterial;

	private Shader _colorizeShader;

	private bool _enabled;

	private Coroutine _prevCircleCoroutine;

	public void BeginCircleEffect(Vector2 screen)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 center = default(Vector2);
		((Vector2)(ref center))._002Ector(screen.x / (float)Screen.width, screen.y / (float)Screen.height);
		if (_prevCircleCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_prevCircleCoroutine);
		}
		_prevCircleCoroutine = ((MonoBehaviour)this).StartCoroutine(CoCircleEffect(center));
	}

	private IEnumerator CoCircleEffect(Vector2 center)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_enabled = true;
		float offset = 0f;
		_outlineMaterial.SetVector("_CircleCenter", Vector4.op_Implicit(center));
		while (true)
		{
			_outlineMaterial.SetFloat("_FadeInRadius", _fadeIn + offset);
			_outlineMaterial.SetFloat("_ColorRadius", _nextColor + offset);
			_outlineMaterial.SetFloat("_FadeOutRadius", _fadeOut + offset);
			yield return null;
			if (_fadeOut + offset >= _duration)
			{
				break;
			}
			offset += Time.deltaTime * _speed;
		}
		_enabled = false;
		_prevCircleCoroutine = null;
	}

	protected override void OnAwake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		_outlineMaterial = new Material(Shader.Find("Hidden/OutlineScreen"));
		_blurMaterial = new Material(Shader.Find("Hidden/FastBlur"));
		_colorizeShader = Shader.Find("Hidden/OutlineColorize");
		_mainCamera = ((Component)KSingleton<MainCamera>.Instance()).GetComponent<Camera>();
	}

	private void OnPreRender()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (_enabled)
		{
			RenderTexture targetTexture = _mainCamera.targetTexture;
			int cullingMask = _mainCamera.cullingMask;
			CameraClearFlags clearFlags = _mainCamera.clearFlags;
			Color backgroundColor = _mainCamera.backgroundColor;
			_mainCamera.cullingMask = ((LayerMask)(ref _outlineLayer)).value;
			_mainCamera.clearFlags = (CameraClearFlags)2;
			_mainCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			_mainCamera.RenderWithShader(_colorizeShader, "RenderType");
			_mainCamera.backgroundColor = backgroundColor;
			_mainCamera.clearFlags = clearFlags;
			_mainCamera.cullingMask = cullingMask;
			int num = targetTexture.width >> 1;
			int num2 = targetTexture.width >> 1;
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, targetTexture.format);
			Blur(targetTexture, temporary, 1);
			_outlineMaterial.SetColor("_OutlineCol1", _outlineColor1);
			_outlineMaterial.SetColor("_OutlineCol2", _outlineColor2);
			_outlineMaterial.SetFloat("_OutlineStrength", _outlineStrength);
			_outlineMaterial.SetTexture("_BlurTex", (Texture)(object)temporary);
			Graphics.Blit((Texture)(object)targetTexture, (RenderTexture)null, _outlineMaterial);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private void Blur(RenderTexture input, RenderTexture output, int downSample)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Graphics.Blit((Texture)(object)input, output, _blurMaterial, 0);
		float num = 1f / (1f * (float)(1 << downSample));
		for (int i = 0; i < _outlineIteration; i++)
		{
			float num2 = (float)i * 1f;
			_blurMaterial.SetVector("_Parameter", new Vector4(_outlineSize * num + num2, (0f - _outlineSize) * num - num2, 0f, 0f));
			RenderTexture temporary = RenderTexture.GetTemporary(input.width, input.height, 0, input.format);
			((Texture)temporary).filterMode = (FilterMode)1;
			Graphics.Blit((Texture)(object)output, temporary, _blurMaterial, 1);
			output.DiscardContents();
			Graphics.Blit((Texture)(object)temporary, output, _blurMaterial, 2);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
