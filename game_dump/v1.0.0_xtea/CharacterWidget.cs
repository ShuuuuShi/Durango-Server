using System;
using System.Collections.Generic;
using System.Text;
using L10N;
using Player;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class CharacterWidget : MonoBehaviour
{
	[SerializeField]
	private UITexture _portrait;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Transform _mainContainer;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _categoryLevelLabel;

	[SerializeField]
	private UILabel _expLabel;

	[SerializeField]
	private UISprite _expGauge;

	[SerializeField]
	private UIWidget _clanWidget;

	[SerializeField]
	private UISpriteLabel _clanLabel;

	private string _expLabelFormat;

	private Vector3 _mainContainerPos;

	public void Init()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_mainContainerPos = _mainContainer.localPosition;
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_portrait).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(GameManager.PlayerId, delegate(PlayerInfo info)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				if (info.Valid)
				{
					ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
					profileTooltip.Set(info);
					profileTooltip.Show((UIWidget)_portrait, Vector2.up * ((float)_portrait.height * (1f - _portrait.pivotOffset.y) + 30f) + Vector2.right * 10f, 3600f);
				}
			});
		});
	}

	private void OnEnable()
	{
		if ((Object)(object)PlayerBehavior.LocalPlayer != (Object)null)
		{
			PortraitBuilder.Argument portraitArgument = PlayerBehavior.LocalPlayer.GetPortraitArgument();
			portraitArgument.Emotion = KSingleton<PlayerController>.Instance().MainStatus.PortraitEmotion;
			PortraitBuilder.Set(portraitArgument, _portrait);
		}
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += OnUpdateCategoryLevel;
		OnUpdateCategoryLevel();
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged -= OnUpdateCategoryLevel;
	}

	public void SetName(string playerName)
	{
		_nameLabel.text = playerName;
	}

	public void SetTitle(string playerTitle)
	{
		_titleLabel.text = playerTitle;
	}

	public void SetClan(string clanName)
	{
		if (string.IsNullOrEmpty(clanName))
		{
			((Component)_clanWidget).gameObject.SetActive(false);
			return;
		}
		((Component)_clanWidget).gameObject.SetActive(true);
		_clanLabel.text = T._("{0} 부족", clanName);
	}

	public void SetExp(int level, int current, int currentMax)
	{
		float num = (float)current / (float)currentMax;
		_expGauge.fillAmount = num;
		((Component)_expGauge).gameObject.SetActive(num > 0f);
		if (_expLabelFormat == null)
		{
			_expLabelFormat = _expLabel.text;
		}
		_expLabel.text = T._(_expLabelFormat, level, current, currentMax);
	}

	private void OnUpdateCategoryLevel(Category cat, int prev, int lv)
	{
		OnUpdateCategoryLevel();
	}

	private void OnUpdateCategoryLevel()
	{
		Array values = Enum.GetValues(typeof(Category));
		List<SkillCategory> list = new List<SkillCategory>();
		for (int i = 0; i < values.Length; i++)
		{
			SkillCategory skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory((Category)(int)values.GetValue(i));
			if (skillCategory != null)
			{
				list.Add(skillCategory);
			}
		}
		list.Sort(Comparison);
		StringBuilder stringBuilder = new StringBuilder();
		int j = 0;
		for (int num = Mathf.Min(3, list.Count); j < num; j++)
		{
			if (j > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.AppendFormat("[{1}] {0}", SkillUtil.CategoryLocalizeName(list[j].Category), SkillUtil.CategoryIcon(list[j].Category));
		}
		_categoryLevelLabel.text = stringBuilder.ToString().Trim();
	}

	public void UpdateLayout()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vector3 mainContainerPos = _mainContainerPos;
		mainContainerPos.y = ((!string.IsNullOrEmpty(_titleLabel.text)) ? _mainContainerPos.y : ((Component)_titleLabel).transform.localPosition.y);
		_mainContainer.localPosition = mainContainerPos;
	}

	private static int Comparison(SkillCategory c1, SkillCategory c2)
	{
		int num = c2.Level - c1.Level;
		if (num == 0)
		{
			num = c1.Category - c2.Category;
		}
		return num;
	}
}
