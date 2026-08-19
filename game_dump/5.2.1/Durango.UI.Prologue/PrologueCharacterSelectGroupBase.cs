using System;
using Durango.UI.Control;
using Shared.Player;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Prologue;

public abstract class PrologueCharacterSelectGroupBase : UIBase
{
	public Action OnSubmit;

	public Action OnChangeCostume;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private SelectableButton _selectBtn;

	[SerializeField]
	private SoundEventType _zoomInSound;

	[SerializeField]
	private SoundEventType _zoomOutSound;

	private bool submitted;

	protected virtual void Awake()
	{
		_selectBtn.Clicked = OnSelectCharacter;
		TryClose();
	}

	private void Start()
	{
		base.OnOpenSucceed += OpenSucceed;
		base.OnCloseSucceed += CloseCucceed;
	}

	private void OpenSucceed()
	{
		SoundManager.PlayEvent(_zoomInSound);
	}

	private void CloseCucceed()
	{
		if (!submitted)
		{
			SoundManager.PlayEvent(_zoomOutSound);
		}
	}

	private void OnSelectCharacter()
	{
		submitted = true;
		if (OnSubmit != null)
		{
			OnSubmit();
		}
	}

	protected virtual void OnCancelSelectCharacter(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonMedium);
		Close();
	}

	protected void OnChangeCharacterCostume(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonMedium);
		if (OnChangeCostume != null)
		{
			OnChangeCostume();
		}
	}

	public void SetSelectCharactInfo(Shared.Player.Job job, bool male)
	{
		string key = LocalizeUtil.GetKey(job);
		_nameLabel.text = LocalizeSystem.Get(key);
		Yaml.Job job2 = SingletonDict<Shared.Player.Job, Yaml.Job>.Get(job);
		_descriptionLabel.text = ((job2 != null) ? job2.description.ToString() : string.Empty);
		_commentLabel.text = LocalizeSystem.Get(string.Format("{0}_{1}_description", key, (!male) ? "f" : "m"));
		OnSetSelectCharacterInfo(job2);
		UIUtility.UpdateAnchors(_scrollView.transform);
		_scrollView.ResetPosition();
	}

	protected abstract void OnSetSelectCharacterInfo(Yaml.Job data);
}
