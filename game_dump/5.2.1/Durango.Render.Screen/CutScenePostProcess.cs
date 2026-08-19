using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Screen;

public class CutScenePostProcess : MonoBehaviour
{
	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private GameObject _centerObject;

	[SerializeField]
	private Texture2D _rampTex;

	[SerializeField]
	private Vector3 _hilight;

	[SerializeField]
	private Vector3 _midtone;

	[SerializeField]
	private Vector3 _shadow;

	[SerializeField]
	private Vector2 _blurCenter = new Vector2(0.5f, 0.5f);

	[ExposedInEditor(null)]
	private float _blurSize;

	private Material _material;

	public float BlurSize
	{
		get
		{
			return _blurSize;
		}
		set
		{
			if (_blurSize <= 0f && value > 0f)
			{
				Material.EnableKeyword("RADIAL_BLUR_ON");
			}
			else if (_blurSize > 0f && value <= 0f)
			{
				Material.DisableKeyword("RADIAL_BLUR_ON");
			}
			_blurSize = value;
		}
	}

	private Material Material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(_shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
				_material.SetTexture("_RampTex", _rampTex);
				_material.SetTexture("_LookupTex", GenerateLookupTex());
			}
			return _material;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_centerObject != null)
		{
			Vector3 vector = Singleton<MainCamera>.Instance().Camera.WorldToViewportPoint(_centerObject.transform.position);
			_blurCenter.x = vector.x;
			_blurCenter.y = vector.y;
		}
		if (BlurSize > 0f)
		{
			Material.SetFloat("_BlurSize", BlurSize);
			Material.SetVector("_BlurCenter", _blurCenter);
		}
		Graphics.Blit(source, destination, Material);
	}

	private Texture2D GenerateLookupTex()
	{
		Texture2D texture2D = new Texture2D(256, 1, TextureFormat.RGB24, mipmap: false);
		Color32[] pixels = texture2D.GetPixels32();
		for (int i = 0; i < 256; i++)
		{
			float original = (float)i / 255f;
			for (int j = 0; j < 3; j++)
			{
				byte b = (byte)Mathf.Clamp(CustomColorCorrectionEffect.GetColorCorretionResult(original, _hilight[j], _midtone[j], _shadow[j]) * 255f, 0f, 255f);
				switch (j)
				{
				case 0:
					pixels[i].r = b;
					break;
				case 1:
					pixels[i].g = b;
					break;
				case 2:
					pixels[i].b = b;
					break;
				}
			}
		}
		texture2D.SetPixels32(pixels);
		texture2D.Apply(updateMipmaps: false);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Point;
		return texture2D;
	}
}
