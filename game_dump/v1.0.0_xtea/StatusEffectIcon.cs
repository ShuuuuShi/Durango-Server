using System.Collections;
using StatusEffectData;
using UnityEngine;

public class StatusEffectIcon : MonoBehaviour
{
	public delegate void StatusEffectIconDelegate(StatusEffectIcon se);

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _arrow;

	private Transform _trans;

	private int _index;

	private UIWidget _widget;

	private TweenPosition _tweener;

	private Vector3 _fadeInTargetPos;

	private static readonly float[] FadeInStatesTime = new float[3] { 0.5f, 0.4f, 1f };

	private Transform Trans
	{
		get
		{
			if ((Object)(object)_trans == (Object)null)
			{
				_trans = ((Component)this).transform;
			}
			return _trans;
		}
	}

	public StatusEffect Data { get; private set; }

	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			if (_index != value)
			{
				if (_index >= 0)
				{
					IsRequireReposition = true;
				}
				_index = value;
				if (_index < 0)
				{
					IsRequireReposition = false;
				}
			}
		}
	}

	public bool IsPlayingEffect { get; private set; }

	public bool IsRequireReposition { get; set; }

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Trans.localPosition;
		}
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (IsPlayingEffect)
			{
				_fadeInTargetPos = value;
			}
			else
			{
				Trans.localPosition = value;
			}
		}
	}

	private string Icon
	{
		set
		{
			if ((Object)(object)_icon != (Object)null)
			{
				if (string.IsNullOrEmpty(value))
				{
					_icon.spriteName = "icon_question";
				}
				else
				{
					_icon.spriteName = value;
				}
			}
		}
	}

	public int Width => Widget.width;

	public int Height => Widget.height;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public TweenPosition Tweener
	{
		get
		{
			if ((Object)(object)_tweener == (Object)null)
			{
				_tweener = ((Component)this).GetComponent<TweenPosition>();
				if ((Object)(object)_tweener == (Object)null)
				{
					_tweener = ((Component)this).gameObject.AddComponent<TweenPosition>();
				}
			}
			return _tweener;
		}
	}

	public event StatusEffectIconDelegate OnFinishedFadeEffect;

	public void Set(StatusEffect data)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		Data = data;
		Icon = data.Template.icon;
		Color color;
		string spriteName;
		switch (data.Template.icon_color)
		{
		default:
		{
			int num;
			if (num == 1)
			{
				color = PresetColor.UIBuff;
				spriteName = "icon_se_incr";
			}
			else
			{
				color = Color.clear;
				spriteName = string.Empty;
			}
			break;
		}
		case "negative":
			color = PresetColor.UIDebuff;
			spriteName = "icon_se_decr";
			break;
		}
		if ((Object)(object)_arrow != (Object)null)
		{
			if (color.a > 0f)
			{
				_arrow.color = color;
				_arrow.spriteName = spriteName;
			}
			else
			{
				_arrow.alpha = 0f;
			}
		}
	}

	public void PlayFadeOut()
	{
		Index = -1;
		if (!IsPlayingEffect)
		{
			((MonoBehaviour)this).StartCoroutine(coFadeOut());
		}
	}

	private IEnumerator coFadeOut()
	{
		IsPlayingEffect = true;
		Vector3 defaultPos = Trans.localPosition;
		float startTime = Time.time;
		float effectTime = 3f;
		float timer = 0f;
		while (timer < effectTime)
		{
			timer = Time.time - startTime;
			float ratio = Mathf.Sqrt(timer / effectTime);
			Trans.localPosition = defaultPos + Vector3.down * 100f * ratio;
			Widget.alpha = 1f - ratio;
			yield return null;
		}
		IsPlayingEffect = false;
		if (this.OnFinishedFadeEffect != null)
		{
			this.OnFinishedFadeEffect(this);
		}
	}

	public void PlayFadeIn(Vector3 targetPos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPlayingEffect)
		{
			_fadeInTargetPos = targetPos;
			((MonoBehaviour)this).StartCoroutine(CoFadeIn());
		}
	}

	private IEnumerator CoFadeIn()
	{
		IsPlayingEffect = true;
		Vector3 defaultPos = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition + Vector3.up * 100f, Trans.parent);
		int state = 0;
		float startTime = Time.time;
		float effectTime = FadeInStatesTime[state];
		float timer = 0f;
		Vector3 pos = default(Vector3);
		while (timer < effectTime)
		{
			timer = Time.time - startTime;
			float ratio = timer / effectTime;
			switch (state)
			{
			case 0:
				Trans.localPosition = defaultPos + Vector3.up * 60f * Mathf.Pow(ratio, 2f);
				Widget.alpha = Mathf.Clamp01(ratio * 2f);
				break;
			case 1:
				Trans.localPosition = defaultPos + Vector3.up * 10f * ratio;
				break;
			case 2:
				pos.x = Mathf.Lerp(defaultPos.x, _fadeInTargetPos.x, Mathf.Pow(ratio, 2f));
				pos.y = Mathf.Lerp(defaultPos.y, _fadeInTargetPos.y, Mathf.Pow(ratio, 2f));
				pos.z = 0f;
				Trans.localPosition = pos;
				Widget.alpha = Mathf.Pow(Mathf.Abs(ratio * 2f - 1f), 3f);
				break;
			}
			if (timer >= effectTime && state < FadeInStatesTime.Length - 1)
			{
				state++;
				startTime = Time.time;
				effectTime = FadeInStatesTime[state];
				timer = 0f;
				defaultPos = Trans.localPosition;
			}
			yield return null;
		}
		IsPlayingEffect = false;
		if (Index < 0)
		{
			PlayFadeOut();
		}
		else if (this.OnFinishedFadeEffect != null)
		{
			this.OnFinishedFadeEffect(this);
		}
	}
}
