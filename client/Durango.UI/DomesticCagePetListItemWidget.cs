using System;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class DomesticCagePetListItemWidget : UIWidget
{
	[SerializeField]
	private UISprite _portrait;

	[SerializeField]
	private UIWidget _dinoStatus;

	[SerializeField]
	private UILabel _dinoName;

	[SerializeField]
	private UILabel _denoDetail;

	[SerializeField]
	private UISprite _statusSprite;

	[SerializeField]
	private UISprite _glowSprite;

	[SerializeField]
	private UISprite _addableSprite;

	[SerializeField]
	private TweenerPlayer _yammyAnimation;

	[SerializeField]
	private GameObject _cheatButton;

	public DomesticationInfo? Rein { get; private set; }

	public event Action<DomesticationInfo> SkipProgressCheat;

	protected override void OnStart()
	{
		base.OnStart();
		if (!Application.isPlaying || !Debug.isDebugBuild)
		{
			return;
		}
		UIEventListener.Get(_cheatButton).onClick = delegate
		{
			if (Rein.HasValue && this.SkipProgressCheat != null)
			{
				this.SkipProgressCheat(Rein.Value);
			}
		};
	}

	public void SetSelect(bool select)
	{
		GetComponent<Selectable>().Selected = select;
	}

	public void Set(DomesticationInfo rein)
	{
		Rein = rein;
		_dinoStatus.gameObject.SetActive(value: true);
		_addableSprite.gameObject.SetActive(value: false);
		Animal animal = SingletonDict<int, Animal>.Get(rein.EntityType);
		if (animal == null)
		{
			UISprite portrait = _portrait;
			string empty = string.Empty;
			_denoDetail.text = empty;
			empty = empty;
			_dinoName.text = empty;
			portrait.spriteName = empty;
		}
		else
		{
			_dinoName.text = animal.Name;
			_portrait.spriteName = animal.Portrait;
			SetStatus(rein);
			_cheatButton.gameObject.SetActive(Debug.isDebugBuild);
		}
	}

	private void SetStatus(DomesticationInfo rein)
	{
		CageStatus cageStatus = PetUtil.ConverInfoToStatus(rein);
		_statusSprite.spriteName = PetUtil.ConverStatusToSrpite(cageStatus);
		Pair<Color, Color> pair = PetUtil.ConverStatusToGradient(cageStatus);
		_statusSprite.gradientTop = pair.Item1;
		_statusSprite.gradientBottom = pair.Item2;
		_glowSprite.gameObject.SetActive(cageStatus == CageStatus.InProgress);
		_denoDetail.text = T._("{0:lv:}  <bar/>  {1}", rein.Level, (cageStatus != CageStatus.InProgress) ? PetUtil.GetDomesticPetStatusText(cageStatus) : PetUtil.ConvertInfoToRemainingTime(rein, 2, "min"));
	}

	public void SetAsAddable()
	{
		Rein = null;
		_dinoStatus.gameObject.SetActive(value: false);
		_addableSprite.gameObject.SetActive(value: true);
		_cheatButton.gameObject.SetActive(value: false);
	}

	public void PlayYammyAnimation()
	{
		_yammyAnimation.Play();
	}
}
