using UnityEngine;

namespace Durango.UI;

public class EmblemTexture : MonoBehaviour
{
	private Point2 _pos = -Point2.one;

	private UITexture _texture;

	private ApngTexture _apngTexture;

	private void Awake()
	{
		EmblemAtlas emblemAtlas = ClanSystem.EmblemAtlas;
		if (emblemAtlas != null)
		{
			emblemAtlas.TextureResized += OnResizeAtlasTexture;
			emblemAtlas.ImageChanged += OnChangeImage;
		}
		Set(_pos);
	}

	private void OnDestroy()
	{
		EmblemAtlas emblemAtlas = ClanSystem.EmblemAtlas;
		if (emblemAtlas != null)
		{
			emblemAtlas.TextureResized -= OnResizeAtlasTexture;
			emblemAtlas.ImageChanged -= OnChangeImage;
		}
	}

	private void OnResizeAtlasTexture()
	{
		Set(_pos);
	}

	private void OnChangeImage(Point2 pos)
	{
		if (_pos == pos)
		{
			Set(_pos);
		}
	}

	private void Set(Point2 pos)
	{
		_pos = pos;
		if (_pos.x < 0 || _pos.y < 0)
		{
			return;
		}
		EmblemAtlas emblemAtlas = ClanSystem.EmblemAtlas;
		if (emblemAtlas != null)
		{
			Texture2D texture = emblemAtlas.Texture;
			Rect uvRect = emblemAtlas.GetUvRect(_pos);
			if (_apngTexture != null)
			{
				_apngTexture.Set(texture, uvRect);
			}
			else if (_texture != null)
			{
				_texture.mainTexture = texture;
				_texture.uvRect = uvRect;
			}
		}
	}

	public static void Set(UITexture comp, Point2 pos)
	{
		if (!(comp == null))
		{
			EmblemTexture emblemTexture = comp.gameObject.AddMissingComponent<EmblemTexture>();
			emblemTexture._texture = comp;
			emblemTexture._apngTexture = null;
			emblemTexture.Set(pos);
		}
	}

	public static void Set(ApngTexture comp, Point2 pos)
	{
		if (!(comp == null))
		{
			EmblemTexture emblemTexture = comp.gameObject.AddMissingComponent<EmblemTexture>();
			emblemTexture._texture = null;
			emblemTexture._apngTexture = comp;
			emblemTexture.Set(pos);
		}
	}
}
