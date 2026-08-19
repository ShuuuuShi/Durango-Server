using System;
using System.Collections.Generic;
using BestHTTP;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class EmblemAtlas
{
	public interface IEmblemMaker
	{
		void Make(string id, [NotNull] Action<Color[]> onResult);
	}

	private const int Unit = 256;

	private const int EmblemSize = 32;

	private readonly AsyncCachedDictionary<string, Point2> _emblems;

	private readonly List<int> _posList = new List<int>();

	private readonly Texture2D _readTexture = new Texture2D(0, 0);

	private IEmblemMaker _defaultEmblemMaker;

	public Texture2D Texture { get; private set; }

	public event Action TextureResized;

	public event Action<Point2> ImageChanged;

	public EmblemAtlas(IEmblemMaker defaultEmblemMaker = null)
	{
		_emblems = new AsyncCachedDictionary<string, Point2>(Request);
		_emblems.EmptyValue = -Point2.one;
		_defaultEmblemMaker = defaultEmblemMaker;
		Texture = new Texture2D(256, 256, TextureFormat.RGBA32, mipmap: false);
		Texture.wrapMode = TextureWrapMode.Clamp;
		Texture.filterMode = FilterMode.Point;
	}

	public void Get(string key, Action<Point2> onResult, bool refresh)
	{
		_emblems.Request(key, (onResult != null) ? onResult : new Action<Point2>(DefaultOnResult), refresh);
	}

	private void DefaultOnResult(Point2 pos)
	{
	}

	private void Request(string key, Point2 cachedValue, Action<string, Point2> onResult)
	{
		Http.Request(GameManager.GatewayUrl + "/clans/" + key + "/emblem.png", delegate(byte[] bytes, HTTPResponse response)
		{
			bool flag = false;
			Point2 value;
			if (bytes == null || bytes.Length == 0)
			{
				if (_defaultEmblemMaker != null)
				{
					_defaultEmblemMaker.Make(key, delegate(Color[] cols)
					{
						bool flag2 = false;
						if (_emblems.TryGetCachedValue(key, out var value2))
						{
							flag2 = true;
						}
						else
						{
							value2 = Add();
						}
						SetImage(value2, cols);
						if (onResult != null)
						{
							onResult(key, value2);
						}
						if (flag2 && this.ImageChanged != null)
						{
							this.ImageChanged(value2);
						}
					});
					return;
				}
				value = -Point2.one;
			}
			else
			{
				if (_emblems.TryGetCachedValue(key, out value))
				{
					if (value == -Point2.one)
					{
						value = Add();
					}
					flag = true;
				}
				else
				{
					value = Add();
				}
				SetImage(value, bytes);
			}
			if (onResult != null)
			{
				onResult(key, value);
			}
			if (flag && this.ImageChanged != null)
			{
				this.ImageChanged(value);
			}
		});
	}

	private void SetImage(Point2 pos, byte[] bytes)
	{
		Color32[] array;
		if (bytes == null || bytes.Length == 0)
		{
			array = new Color32[1024];
			for (int i = 0; i < array.Length; i++)
			{
				ref Color32 reference = ref array[i];
				reference = Color.clear;
			}
		}
		else
		{
			_readTexture.LoadImage(bytes);
			array = ((_readTexture.width == 32 && _readTexture.height == 32) ? _readTexture.GetPixels32() : UIUtility.ResizeTexturePixels(_readTexture, 32, 32));
		}
		SetImage(pos, array);
	}

	private void SetImage(Point2 pos, Color32[] cols)
	{
		Texture.SetPixels32(pos.x * 32, pos.y * 32, 32, 32, cols);
		Texture.Apply();
	}

	private void SetImage(Point2 pos, Color[] cols)
	{
		Texture.SetPixels(pos.x * 32, pos.y * 32, 32, 32, cols);
		Texture.Apply();
	}

	private Point2 Add()
	{
		int width = Texture.width;
		int height = Texture.height;
		int num = width / 32;
		int num2 = height / 32;
		for (int i = 0; i < num; i++)
		{
			int num3 = ((i < _posList.Count) ? _posList[i] : 0);
			if (num3 < num2)
			{
				Point2 result = new Point2(i, num3);
				if (i < _posList.Count)
				{
					_posList[i]++;
					return result;
				}
				_posList.Add(1);
				return result;
			}
		}
		Color32[] pixels = Texture.GetPixels32();
		Point2 result2;
		if (width < height)
		{
			Texture.Resize(width + 256, height);
			result2 = new Point2(_posList.Count, 0);
			_posList.Add(1);
		}
		else
		{
			Texture.Resize(width, height + 256);
			result2 = new Point2(0, _posList[0]);
			_posList[0]++;
		}
		Texture.SetPixels32(0, 0, width, height, pixels);
		Texture.Apply();
		if (this.TextureResized != null)
		{
			this.TextureResized();
		}
		return result2;
	}

	public Rect GetUvRect(Point2 pos)
	{
		Texture2D texture = Texture;
		int width = texture.width;
		int height = texture.height;
		return UIUtility.DivideRect(new Rect(pos.x * 32, pos.y * 32, 32f, 32f), width, height);
	}
}
