using System.Collections.Generic;
using Shared.Ability;
using StatisticsData;
using UnityEngine;

public class CharacterInfoPage : MonoBehaviour
{
	[SerializeField]
	private CharacterWidget _characterWidget;

	[SerializeField]
	private UIWidget _abilityWidget;

	[SerializeField]
	private AbilityWidget _physicalStat;

	[SerializeField]
	private AbilityWidget _mentalStat;

	[SerializeField]
	private GameObject _openClanUITouchBox;

	[SerializeField]
	private GameObject _openSkillUITouchBox;

	[SerializeField]
	private GameObject _openAbilityUITouchBox;

	[SerializeField]
	private TweenerPlayer _showAnimation;

	private bool _isPlayShowAnimation;

	private UIWidget _widget;

	private bool _isInit;

	public CharacterWidget CharacterWidget
	{
		get
		{
			Init();
			return _characterWidget;
		}
	}

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

	private void Awake()
	{
		UIEventListener.Get(_openClanUITouchBox).onClick = delegate
		{
			UIManager.FindScript<ClanGroup>().Open();
		};
		UIEventListener.Get(_openSkillUITouchBox).onClick = delegate
		{
			UIManager.FindScript<SkillGroup>().Open();
		};
		UIEventListener.Get(_openAbilityUITouchBox).onClick = OnClickAbilityWidget;
		Init();
	}

	private void OnEnable()
	{
		_showAnimation.ResetToBeginning();
	}

	private void OnDisable()
	{
		_isPlayShowAnimation = false;
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_characterWidget.Init();
		}
	}

	private void OnClickAbilityWidget(GameObject go)
	{
		UIManager.FindScript<CharacterStatusGroup>().Open();
	}

	public void SetAbility(Dictionary<Basic, int> abilities)
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		Init();
		if (abilities != null)
		{
			KeyValuePair<Basic, string>[] array = new KeyValuePair<Basic, string>[Statistics.PhysicalAbility.Length];
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				Basic key = Statistics.PhysicalAbility[i];
				string value = abilities.Get(key, 0).ToString();
				ref KeyValuePair<Basic, string> reference = ref array[i];
				reference = new KeyValuePair<Basic, string>(key, value);
			}
			KeyValuePair<Basic, string>[] array2 = new KeyValuePair<Basic, string>[Statistics.MentalAbility.Length];
			int j = 0;
			for (int num2 = array2.Length; j < num2; j++)
			{
				Basic key2 = Statistics.MentalAbility[j];
				string value2 = abilities.Get(key2, 0).ToString();
				ref KeyValuePair<Basic, string> reference2 = ref array2[j];
				reference2 = new KeyValuePair<Basic, string>(key2, value2);
			}
			_physicalStat.Set(array);
			_mentalStat.Set(array2);
			int num3 = Mathf.Max(_physicalStat.Widget.height, _mentalStat.Widget.height);
			_abilityWidget.height = num3 + (int)Mathf.Abs(((Component)_physicalStat).transform.localPosition.y * 2f);
			UIUtility.UpdateAnchors(((Component)_abilityWidget).transform);
		}
	}

	public void ShowAnimation()
	{
		if (!_isPlayShowAnimation)
		{
			_isPlayShowAnimation = true;
			_showAnimation.Play();
		}
	}

	public void UpdateLayout()
	{
		_characterWidget.UpdateLayout();
		_abilityWidget.ResetAndUpdateAnchors();
	}
}
