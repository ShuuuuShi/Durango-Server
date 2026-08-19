using UnityEngine;

public class TileGrid : MonoBehaviour
{
	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private SpriteData _tileSprite;

	[SerializeField]
	private SpriteData _chunkSprite;

	private bool _isInit;

	private void OnEnable()
	{
		Init();
		((Component)_panel).gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		((Component)_panel).gameObject.SetActive(false);
	}

	private void Init()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		UISpriteData sprite = _tileSprite.atlas.GetSprite(_tileSprite.sprite);
		int num = sprite.width * 16;
		int num2 = sprite.height * 16;
		Vector3 one = Vector3.one;
		one.x = 200f / (float)sprite.width;
		one.y = 200f / (float)sprite.height;
		Vector3 localPosition = default(Vector3);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				UISprite uISprite = ((Component)_panel).gameObject.AddChild<UISprite>();
				UISprite uISprite2 = ((Component)_panel).gameObject.AddChild<UISprite>();
				_tileSprite.Set(uISprite);
				_chunkSprite.Set(uISprite2);
				uISprite.type = UIBasicSprite.Type.Tiled;
				uISprite.width = num;
				uISprite.height = num2;
				((Component)uISprite).transform.localScale = one;
				uISprite2.type = UIBasicSprite.Type.Sliced;
				uISprite2.width = 3200;
				uISprite2.height = 3200;
				uISprite2.depth = 1;
				((Object)uISprite).name = $"{i}_{j}";
				((Vector3)(ref localPosition))._002Ector((float)(i * num) * one.x, (float)(j * num2) * one.y);
				((Component)uISprite).transform.localPosition = localPosition;
				((Component)uISprite2).transform.localPosition = localPosition;
			}
		}
	}
}
