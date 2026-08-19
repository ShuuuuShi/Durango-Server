using System;
using System.Collections.Generic;
using InteractionData;
using L10N;
using TimerData;
using UnityEngine;

public class InteractionMenu : MonoBehaviour
{
	private class GatheringQueueIcon
	{
		public int Id;

		public int Index;

		public int PrevIndex;

		public GameObject Container;

		public UISprite Icon;

		public UISprite Border;

		public UISprite Shadow;
	}

	public delegate void InteractionMenuDelegate(InteractionMenu menu);

	public const int IconSize = 64;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _shadow;

	public InteractionMenuDelegate OnClickMenu;

	public InteractionMenuDelegate OnLongClickMenu;

	public InteractionMenuDelegate OnGatheringQueueClick;

	private UIWidget _widget;

	private Collider _touchCollider;

	private ProgressGauge _progressGauge;

	private float _alpha;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _nameBG;

	private Vector3 _nameLabelPos;

	private int _nameFontSize;

	[SerializeField]
	private UILabel _timeLabel;

	private float _duration;

	private float _remainTimer;

	private bool _isTimerTextMode;

	[SerializeField]
	private UISpriteLabel _requireLabel;

	[SerializeField]
	private UISprite _requireBg;

	private Vector3 _requireLabelPos;

	private TweenPosition _positionTweener;

	private TweenAlpha _alphaTweener;

	[SerializeField]
	private UIWidget _progressGaugeWidget;

	[SerializeField]
	private GameObject _gatheringQueueIcon;

	private Queue<GatheringQueueIcon> _gatheringQueueIconPool = new Queue<GatheringQueueIcon>();

	private List<GatheringQueueIcon> _gatheringQueueIcons = new List<GatheringQueueIcon>();

	private bool _isInit;

	private bool _isPress;

	public float MenuRadian { get; set; }

	public bool Valid { get; set; }

	public InteractionMenuControl Parent { get; set; }

	public InteractionMenuData Data { get; private set; }

	public bool IsMajor { get; private set; }

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

	public Collider TouchCollider
	{
		get
		{
			if ((Object)(object)_touchCollider == (Object)null)
			{
				_touchCollider = ((Component)this).GetComponent<Collider>();
			}
			return _touchCollider;
		}
	}

	private ProgressGauge ProgressGauge
	{
		get
		{
			if ((Object)(object)_progressGauge == (Object)null)
			{
				_progressGauge = ((Component)this).GetComponent<ProgressGauge>();
			}
			return _progressGauge;
		}
	}

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
			if (((Behaviour)AlphaTweener).enabled)
			{
				AlphaTweener.to = value;
			}
			else
			{
				Widget.alpha = value;
			}
		}
	}

	private string Icon
	{
		set
		{
			_iconSprite.spriteName = value;
			UIUtility.ResizeToSquare(_iconSprite, 64);
		}
	}

	private string Description
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				((Component)_description).gameObject.SetActive(true);
				_description.text = value;
			}
			else
			{
				((Component)_description).gameObject.SetActive(false);
			}
		}
	}

	private string Name
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_nameLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_nameLabel).gameObject.SetActive(true);
			_nameLabel.text = value;
			_nameBG.UpdateAnchors();
		}
	}

	public string RequireText
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_requireLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_requireLabel).gameObject.SetActive(true);
			_requireLabel.text = value;
		}
	}

	public bool NeedInitAnimation { get; set; }

	public TweenPosition PositionTweener
	{
		get
		{
			if ((Object)(object)_positionTweener == (Object)null)
			{
				_positionTweener = ((Component)this).GetComponent<TweenPosition>();
				_positionTweener.AddOnFinished(delegate
				{
					NeedInitAnimation = false;
				});
			}
			return _positionTweener;
		}
	}

	public TweenAlpha AlphaTweener
	{
		get
		{
			if ((Object)(object)_alphaTweener == (Object)null)
			{
				_alphaTweener = ((Component)this).GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public int Index { get; set; }

	public UIWidget ProgressGaugeWidget => _progressGaugeWidget;

	public GameObject GatheringQueueWidget => _gatheringQueueIcon;

	private bool Select
	{
		set
		{
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			if (value)
			{
				_shadow.alpha = 1f;
				_background.color = UIManager.UIYellow;
				_iconSprite.color = UIManager.UIYellow;
				_description.color = UIManager.UIYellow;
				_timeLabel.color = UIManager.UIYellow;
				_nameLabel.color = UIManager.UIBlack;
				_nameBG.color = UIManager.UIYellow;
			}
			else
			{
				_shadow.alpha = 0.6f;
				_background.color = UIManager.UIBlack;
				_iconSprite.color = ((!(Data.Color == Color.clear)) ? Data.Color : Color.white);
				_description.color = UIManager.UIWhite;
				_timeLabel.color = UIManager.UIWhite;
				_nameLabel.color = UIManager.UIWhite;
				_nameBG.color = UIManager.UIBlack;
			}
		}
	}

	private bool IsPress
	{
		get
		{
			return _isPress;
		}
		set
		{
			_isPress = value;
		}
	}

	public static string GetTimeString(float time)
	{
		if (time < 0f)
		{
			return string.Empty;
		}
		if (time == 0f)
		{
			return T._("-초");
		}
		if (time < 10f)
		{
			return T._("{0:n1}초", time);
		}
		return TimerSystem.TimeToString(time, TimePeriod.Sec, 2, 1f);
	}

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		Select = false;
		ProgressGaugeWidget.alpha = 0f;
		GatheringQueueWidget.SetActive(false);
	}

	private void OnDisable()
	{
		ClearGatheringQueueItem();
	}

	private void Update()
	{
		UpdateTimeText();
	}

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_nameLabelPos = ((Component)_nameLabel).transform.localPosition;
			_nameFontSize = _nameLabel.fontSize;
			_requireLabelPos = ((Component)_requireLabel).transform.localPosition;
		}
	}

	public void Set(InteractionMenuData data)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Data = data;
		Valid = true;
		RequireText = null;
		IsMajor = Data.Priority >= InteractionMenuPriority.Major;
		((Component)this).transform.localScale = Vector3.one * ((!IsMajor) ? InteractionMenuControl.MinorScale : InteractionMenuControl.MajorScale);
		Refresh();
	}

	public void Refresh()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Init();
		Name = Data.Name;
		Icon = Data.Icon;
		Description = Data.Description;
		RequireText = null;
		_iconSprite.color = Data.Color;
		_background.color = UIManager.UIBlack;
		if (Data.GatheringData != null)
		{
			SetDurationText(Data.GatheringData.Duration);
			if (Data.GatheringData.IsAvailableForGathering())
			{
				Alpha = 1f;
			}
			else
			{
				bool flag = Data.GatheringData.RequiredTools != null && Data.GatheringData.RequiredTools.Count > 0;
				Alpha = 0.75f;
				RequireText = ((!flag) ? null : "[img_notool]");
			}
		}
		else
		{
			SetDurationText(Data.Duration);
			Alpha = 1f;
		}
		if (Data.Timer != null)
		{
			ProgressGauge.Play(Data.Timer);
		}
		else if (ProgressGauge.Timer != null)
		{
			ProgressGauge.Timer.Stop();
		}
	}

	private void OnPress(bool press)
	{
		Select = press;
		if (press)
		{
			((MonoBehaviour)this).CancelInvoke("OnLongClick");
			((MonoBehaviour)this).Invoke("OnLongClick", 0.5f);
		}
		else if (IsPress && OnClickMenu != null)
		{
			OnClickMenu(this);
		}
		IsPress = press;
	}

	private void OnDrag(Vector2 delta)
	{
		IsPress = false;
	}

	private void OnLongClick()
	{
		if (IsPress)
		{
			IsPress = false;
			if (OnLongClickMenu != null)
			{
				OnLongClickMenu(this);
			}
		}
	}

	public void UpdateNameLabelPosition()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		int num = ((MenuRadian > 0f && MenuRadian < (float)Math.PI) ? 1 : (-1));
		if (num > 0)
		{
			_nameLabel.pivot = UIWidget.Pivot.BottomLeft;
			_requireLabel.Label.pivot = UIWidget.Pivot.Left;
			_requireBg.flip = UIBasicSprite.Flip.Nothing;
		}
		else
		{
			_nameLabel.pivot = UIWidget.Pivot.BottomRight;
			_requireLabel.Label.pivot = UIWidget.Pivot.Right;
			_requireBg.flip = UIBasicSprite.Flip.Horizontally;
		}
		Vector3 nameLabelPos = _nameLabelPos;
		nameLabelPos.x *= (float)num;
		((Component)_nameLabel).transform.localPosition = nameLabelPos;
		if (IsMajor)
		{
			_nameLabel.fontSize = _nameFontSize;
		}
		else
		{
			float minorScale = InteractionMenuControl.MinorScale;
			_nameLabel.fontSize = (int)((float)_nameFontSize / minorScale);
		}
		_nameBG.UpdateAnchors();
		Vector3 requireLabelPos = _requireLabelPos;
		requireLabelPos.x *= (float)num;
		((Component)_requireLabel).transform.localPosition = requireLabelPos;
	}

	private void UpdateTimeText()
	{
		if (Data.Timer != null)
		{
			float remain = Data.Timer.Remain;
			if (remain > 0f)
			{
				SetTimerText(remain);
				return;
			}
		}
		SetDurationText(_duration);
	}

	private void SetDurationText(float duration)
	{
		if (_isTimerTextMode || _duration != duration)
		{
			_duration = duration;
			_isTimerTextMode = false;
			if (_duration < 0f)
			{
				((Component)_timeLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_timeLabel).gameObject.SetActive(true);
			_timeLabel.text = GetTimeString(_duration);
		}
	}

	private void SetTimerText(float timer)
	{
		bool flag = timer < 10f;
		if (flag)
		{
			timer *= 10f;
		}
		timer = Mathf.CeilToInt(timer);
		if (flag)
		{
			timer *= 0.1f;
		}
		if (!_isTimerTextMode || _remainTimer != timer)
		{
			_remainTimer = timer;
			_isTimerTextMode = true;
			if (_remainTimer < 0f)
			{
				((Component)_timeLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_timeLabel).gameObject.SetActive(true);
			_timeLabel.text = GetTimeString(_remainTimer);
		}
	}

	private GatheringQueueIcon GatheringQueueIconPop()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		GatheringQueueIcon gatheringQueueIcon = null;
		if (_gatheringQueueIconPool.Count > 0)
		{
			gatheringQueueIcon = _gatheringQueueIconPool.Dequeue();
		}
		else
		{
			GameObject val = ((Component)GatheringQueueWidget.transform.parent).gameObject.AddChild(GatheringQueueWidget);
			val.SetActive(true);
			gatheringQueueIcon = new GatheringQueueIcon();
			gatheringQueueIcon.Container = val;
			gatheringQueueIcon.Icon = ((Component)val.transform.FindChild("Icon")).GetComponent<UISprite>();
			gatheringQueueIcon.Border = ((Component)val.transform.FindChild("Border")).GetComponent<UISprite>();
			gatheringQueueIcon.Shadow = ((Component)val.transform.FindChild("Shadow")).GetComponent<UISprite>();
			gatheringQueueIcon.Icon.color = UIManager.UIWhite;
			gatheringQueueIcon.Border.color = UIManager.UIBlack;
			gatheringQueueIcon.Shadow.color = UIManager.UIBlack;
			gatheringQueueIcon.Shadow.alpha = 0.6f;
			UIEventListener.Get(gatheringQueueIcon.Container).onClick = OnGatheringQueueIconClick;
		}
		gatheringQueueIcon.Id = -1;
		gatheringQueueIcon.Index = -1;
		gatheringQueueIcon.PrevIndex = -1;
		_gatheringQueueIcons.Add(gatheringQueueIcon);
		gatheringQueueIcon.Container.SetActive(true);
		return gatheringQueueIcon;
	}

	private void GatheringQueueIconPush(GatheringQueueIcon icon)
	{
		_gatheringQueueIcons.Remove(icon);
		_gatheringQueueIconPool.Enqueue(icon);
		icon.Container.SetActive(false);
	}

	public void SetGatheringQueueItems(List<int> ids, List<string> icons)
	{
		if (ids == null || ids.Count == 0)
		{
			ClearGatheringQueueItem();
			return;
		}
		for (int i = 0; i < _gatheringQueueIcons.Count; i++)
		{
			_gatheringQueueIcons[i].Index = -1;
		}
		int count = ids.Count;
		for (int j = 0; j < count; j++)
		{
			if (!FindGatheringQueueIcon(ids[j], out var icon))
			{
				icon = GatheringQueueIconPop();
			}
			icon.Id = ids[j];
			icon.Index = j;
			icon.Icon.spriteName = icons[j];
		}
		RepositionGatheringQueueIcon();
	}

	public void ClearGatheringQueueItem()
	{
		for (int num = _gatheringQueueIcons.Count - 1; num >= 0; num--)
		{
			GatheringQueueIconPush(_gatheringQueueIcons[num]);
		}
		RepositionGatheringQueueIcon();
	}

	private bool FindGatheringQueueIcon(int id, out GatheringQueueIcon icon)
	{
		int count = _gatheringQueueIcons.Count;
		for (int i = 0; i < count; i++)
		{
			if (_gatheringQueueIcons[i].Id == id)
			{
				icon = _gatheringQueueIcons[i];
				return true;
			}
		}
		icon = null;
		return false;
	}

	private void RepositionGatheringQueueIcon()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GatheringQueueWidget == (Object)null)
		{
			return;
		}
		Vector3 localPosition = GatheringQueueWidget.transform.localPosition;
		int num = ((MenuRadian > 0f && MenuRadian < (float)Math.PI) ? 1 : (-1));
		localPosition.x = Mathf.Abs(localPosition.x) * (float)num;
		int width = GatheringQueueWidget.GetComponent<UIWidget>().width;
		for (int num2 = _gatheringQueueIcons.Count - 1; num2 >= 0; num2--)
		{
			if (_gatheringQueueIcons[num2].Index == -1)
			{
				GatheringQueueIconPush(_gatheringQueueIcons[num2]);
			}
		}
		int depth = GatheringQueueWidget.GetComponent<UIWidget>().depth;
		int count = _gatheringQueueIcons.Count;
		for (int i = 0; i < count; i++)
		{
			GatheringQueueIcon gatheringQueueIcon = _gatheringQueueIcons[i];
			Vector3 val = localPosition + Vector3.right * (float)num * (float)gatheringQueueIcon.Index * (float)width;
			if (gatheringQueueIcon.Index == gatheringQueueIcon.PrevIndex)
			{
				gatheringQueueIcon.Container.transform.localPosition = val;
			}
			else
			{
				if (gatheringQueueIcon.PrevIndex != -1)
				{
					TweenPosition component = gatheringQueueIcon.Container.GetComponent<TweenPosition>();
					if ((Object)(object)component != (Object)null)
					{
						component.from = gatheringQueueIcon.Container.transform.localPosition;
						component.to = val;
						component.tweenFactor = 0f;
						component.PlayForward();
					}
				}
				else
				{
					TweenAlpha component2 = gatheringQueueIcon.Container.GetComponent<TweenAlpha>();
					if ((Object)(object)component2 != (Object)null)
					{
						component2.tweenFactor = 0f;
						component2.PlayForward();
					}
					gatheringQueueIcon.Container.transform.localPosition = val;
				}
				gatheringQueueIcon.PrevIndex = gatheringQueueIcon.Index;
			}
			int num3 = depth - 10 * i;
			gatheringQueueIcon.Container.GetComponent<UIWidget>().depth = num3;
			gatheringQueueIcon.Shadow.depth = num3 + 1;
			gatheringQueueIcon.Border.depth = num3 + 2;
			gatheringQueueIcon.Icon.depth = num3 + 3;
		}
	}

	private void OnGatheringQueueIconClick(GameObject go)
	{
		if (OnGatheringQueueClick != null)
		{
			OnGatheringQueueClick(this);
		}
	}
}
