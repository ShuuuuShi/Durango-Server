using System;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class AlarmRewardWidget : MonoBehaviour
{
	[SerializeField]
	protected int _group;

	[SerializeField]
	[CanBeNull]
	protected UILabel _mainLabel;

	[SerializeField]
	[CanBeNull]
	protected UILabel _subLabel;

	[SerializeField]
	protected UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _rgbIconTex;

	private bool _isInit;

	private bool _isPause;

	public string Key { get; private set; }

	public int Group => _group;

	public event Action<AlarmRewardWidget> Disabled;

	protected void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			OnInit();
		}
	}

	private void OnDisable()
	{
		if (!_isPause && this.Disabled != null)
		{
			this.Disabled(this);
		}
	}

	protected virtual void OnInit()
	{
	}

	public virtual void Set(string key, AlarmRewardQueue.Args args)
	{
		Init();
		Key = key;
		if (_mainLabel != null)
		{
			_mainLabel.text = args.Main;
		}
		if (_subLabel != null)
		{
			_subLabel.text = args.Sub;
		}
		RewardIconWidget.SetItemIcon(_iconSprite, _rgbIconTex, args.Icon, args.IconScale);
		UpdateLayout();
		Play();
	}

	protected virtual void Play()
	{
		base.gameObject.SetActive(value: true);
	}

	protected virtual void UpdateLayout()
	{
	}

	protected void TimeOut()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Pause(bool pause)
	{
		if (_isPause != pause)
		{
			_isPause = pause;
			base.gameObject.SetActive(!pause);
		}
	}
}
