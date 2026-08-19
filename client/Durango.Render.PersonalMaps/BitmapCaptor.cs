using System;
using UnityEngine;

namespace Durango.Render.PersonalMaps;

public class BitmapCaptor : IDisposable
{
	private const int _renderTextureWidth = 960;

	private const int _renderTextureHeight = 540;

	private UnityEngine.Camera _camera;

	private RenderTexture _prevRenderTexture;

	private Texture2D _texture;

	public int Width { get; private set; }

	public int Height { get; private set; }

	public BitmapCaptor(UnityEngine.Camera camera)
	{
		_camera = camera;
		Width = 960;
		Height = 540;
		_prevRenderTexture = _camera.targetTexture;
		_camera.targetTexture = RenderTexture.GetTemporary(Width, Height, _camera.targetTexture.depth, _camera.targetTexture.format);
		_texture = CreateTexture(Width, Height);
	}

	public void Capture(byte[] bitmap, int destX, int destY, int bitmapWidth, int bytesPerPixel, UnityEngine.Camera targetCamera = null)
	{
		if (targetCamera == null)
		{
			targetCamera = _camera;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = targetCamera.targetTexture;
		_texture.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
		RenderTexture.active = active;
		Color[] pixels = _texture.GetPixels();
		int num = 0;
		int num2 = (destY * bitmapWidth + destX) * bytesPerPixel;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				SetBitmapPixel(pixels[num++], bitmap, num2);
				num2 += bytesPerPixel;
			}
			num2 += (bitmapWidth - Width) * bytesPerPixel;
		}
	}

	public void Dispose()
	{
		RenderTexture targetTexture = _camera.targetTexture;
		_camera.targetTexture = _prevRenderTexture;
		UnityEngine.Object.Destroy(_texture);
		RenderTexture.ReleaseTemporary(targetTexture);
	}

	private static Texture2D CreateTexture(int width, int height)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, mipmap: false);
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}

	private static void SetBitmapPixel(Color pixel, byte[] bitmap, int indexBitmap)
	{
		bitmap[indexBitmap] = (byte)(pixel.r * 255f);
		bitmap[indexBitmap + 1] = (byte)(pixel.g * 255f);
		bitmap[indexBitmap + 2] = (byte)(pixel.b * 255f);
	}
}
