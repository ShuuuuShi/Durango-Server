using UnityEngine;

namespace Durango.UI;

public class CombatStateEffect : UIWidget
{
	private struct SpriteSet
	{
		public Transform Parent;

		public Vector3 BasePosition;

		public UISprite Base;

		public UISprite Background;
	}

	[SerializeField]
	private UISprite _baseSprite;

	[SerializeField]
	private float _speed;

	private SpriteSet _top;

	private SpriteSet _bottom;

	private Vector2 _tileSize;

	private float _ratio;

	private float _timerBegin;

	private float _timerDuration;

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			_top = new SpriteSet
			{
				Parent = base.gameObject.AddChild().transform
			};
			_top.Base = _top.Parent.gameObject.AddChild(_baseSprite.gameObject).GetComponent<UISprite>();
			_top.Base.flip = UIBasicSprite.Flip.Vertically;
			_top.Background = _top.Parent.gameObject.AddChild(_baseSprite.gameObject).GetComponent<UISprite>();
			_top.Background.flip = UIBasicSprite.Flip.Vertically;
			_bottom = new SpriteSet
			{
				Parent = base.gameObject.AddChild().transform
			};
			_bottom.Base = _bottom.Parent.gameObject.AddChild(_baseSprite.gameObject).GetComponent<UISprite>();
			_bottom.Background = _bottom.Parent.gameObject.AddChild(_baseSprite.gameObject).GetComponent<UISprite>();
			UISprite @base = _top.Base;
			int num = _baseSprite.depth + 1;
			_bottom.Base.depth = num;
			@base.depth = num;
			UISprite background = _top.Background;
			Color color = new Color32(0, 0, 0, 100);
			_bottom.Background.color = color;
			background.color = color;
			_baseSprite.gameObject.SetActive(value: false);
			UIManager.AddOnScreenResized(OnScreenResize);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			UpdateSize();
			ResetTimer();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			UpdateTimer();
			UpdatePosition();
		}
	}

	private void OnScreenResize()
	{
		UpdateSize();
	}

	private void UpdatePosition()
	{
		_top.Parent.localPosition = _top.BasePosition + Vector3.right * _tileSize.x * _ratio;
		_bottom.Parent.localPosition = _bottom.BasePosition + Vector3.right * _tileSize.x * _ratio;
		_ratio = (_ratio + Time.deltaTime * _speed) % 1f;
	}

	private void UpdateTimer()
	{
		if (!(_timerDuration <= 0f))
		{
			float num = (Time.time - _timerBegin) / _timerDuration;
			if (num > 1f)
			{
				UISprite @base = _top.Base;
				bool flag = false;
				_bottom.Base.enabled = flag;
				@base.enabled = flag;
				_timerBegin = 0f;
				_timerDuration = 0f;
			}
			else
			{
				int num2 = (int)(((float)_top.Background.width - _tileSize.x) * (1f - num) + _tileSize.x * (1f - _ratio));
				UISprite base2 = _top.Base;
				int num3 = num2;
				_bottom.Base.width = num3;
				base2.width = num3;
			}
		}
	}

	private void UpdateSize()
	{
		Point2 point = new Point2(UIManager.ScreenWidth, UIManager.ScreenHeight);
		_tileSize = _baseSprite.GetTileSize();
		int num = point.x + (int)_tileSize.x;
		UISprite background = _top.Background;
		int num2 = num;
		_bottom.Background.width = num2;
		background.width = num2;
		UISprite @base = _top.Base;
		num2 = num;
		_bottom.Base.width = num2;
		@base.width = num2;
		Vector3 basePosition = new Vector3(0f - _tileSize.x - (float)point.x * 0.5f, (float)point.y * 0.5f - (float)_baseSprite.height * 0.5f);
		_top.BasePosition = basePosition;
		basePosition.y = 0f - basePosition.y;
		_bottom.BasePosition = basePosition;
		UpdateTimer();
		UpdatePosition();
	}

	public void SetColor(Color col)
	{
		UISprite @base = _top.Base;
		_bottom.Base.color = col;
		@base.color = col;
	}

	[ExposedInEditor(null)]
	public void StartTimer(float duration)
	{
		_timerBegin = Time.time;
		_timerDuration = duration;
	}

	public void ResetTimer()
	{
		_timerBegin = 0f;
		_timerDuration = 0f;
		Transform obj = _top.Base.transform;
		Vector3 zero = Vector3.zero;
		_bottom.Base.transform.localPosition = zero;
		obj.localPosition = zero;
		UISprite @base = _top.Base;
		int num = _top.Background.width;
		_bottom.Base.width = num;
		@base.width = num;
		UISprite base2 = _top.Base;
		bool flag = true;
		_bottom.Base.enabled = flag;
		base2.enabled = flag;
	}
}
