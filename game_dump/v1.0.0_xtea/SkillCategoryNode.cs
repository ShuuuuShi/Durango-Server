using System.Collections.Generic;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class SkillCategoryNode : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLebel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private GameObject _maxLvObject;

	[SerializeField]
	private GameObject _maxExpObject;

	[SerializeField]
	private UISprite _gaugeUpper;

	[SerializeField]
	private GameObject _selector;

	[SerializeField]
	private UIWidget _researchingArrow;

	[SerializeField]
	private GameObject _researchAlarm;

	[SerializeField]
	private UITweener _newMaker;

	public SkillCategory Category { get; private set; }

	public void Set(Category category)
	{
		Category = GameSystem<SkillSystem>.Instance().GetSkillCategory(category);
		UpdateData();
	}

	public void UpdateData()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		if (Category == null)
		{
			return;
		}
		_nameLebel.text = SkillUtil.CategoryLocalizeName(Category.Category);
		_iconSprite.spriteName = SkillUtil.CategoryIcon(Category.Category);
		UIUtility.ResizeToSquare(_iconSprite);
		((Component)_researchingArrow).gameObject.SetActive(false);
		if (Category.IsResearching())
		{
			_levelLabel.onPostFill = OnPostFill;
			_levelLabel.text = string.Format("{0}[_{2}][c]{3}{1}[-][/c]", LocalizeUtil.FormatLevel(Category.Level), Category.Level + 1, _researchingArrow.width, UIManager.ColorBBCode(PresetColor.UIYellow));
		}
		else
		{
			_levelLabel.onPostFill = null;
			_levelLabel.text = LocalizeUtil.FormatLevel(Category.Level);
		}
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		GameSystem<SkillSystem>.Instance().GetCategoryExp(Category.Category, out var current, out var max);
		_gaugeUpper.fillAmount = Mathf.Clamp01((float)current / (float)max);
		((Component)((Component)_gaugeUpper).transform.parent).gameObject.SetActive(current < max || Category.Level < level);
		_researchAlarm.SetActive(current >= max && Category.Level < level && !Category.IsResearching());
		_maxLvObject.SetActive(Category.Level > 0 && max < 0);
		_maxExpObject.SetActive(current >= max && Category.Level >= level);
		bool flag = false;
		bool flag2 = false;
		List<SkillBundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		for (int i = 0; i < skills.Count; i++)
		{
			if (skills[i].Category == Category.Category)
			{
				if (!flag2 && skills[i].HasNew())
				{
					flag2 = true;
				}
				if (!flag && skills[i].GetLearnableCount() > 0)
				{
					flag = true;
				}
			}
		}
		((Component)_newMaker).gameObject.SetActive(flag);
		if (flag)
		{
			((Component)_newMaker).GetComponent<UIRect>().UpdateAnchors();
			if (flag2)
			{
				float num = Time.time % (_newMaker.duration * 2f);
				if (num > _newMaker.duration)
				{
					_newMaker.tweenFactor = 2f * _newMaker.duration - num;
					_newMaker.PlayReverse();
				}
				else
				{
					_newMaker.tweenFactor = num;
					_newMaker.PlayForward();
				}
			}
			else
			{
				_newMaker.PlayForward();
				_newMaker.ResetToBeginning();
				((Behaviour)_newMaker).enabled = false;
			}
		}
		Color tint = ((Category.Level != 0) ? Color.white : (Color.white * 0.5f));
		tint.a = 1f;
		base.ColorComp.SetTint(tint);
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		for (int i = 0; i + 3 < uvs.size; i += 4)
		{
			if (uvs[i] == uvs[i + 1] && uvs[i + 1] == uvs[i + 2] && uvs[i + 2] == uvs[i + 3] && (!(verts[i] == verts[i + 1]) || !(verts[i + 1] == verts[i + 2]) || !(verts[i + 2] == verts[i + 3])))
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(float.MaxValue, float.MaxValue);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(float.MinValue, float.MinValue);
			for (int j = 0; j < 4; j++)
			{
				Vector3 val3 = verts[num + j];
				val.x = Mathf.Min(val3.x, val.x);
				val.y = Mathf.Min(val3.y, val.y);
				val2.x = Mathf.Max(val3.x, val2.x);
				val2.y = Mathf.Max(val3.y, val2.y);
			}
			((Component)_researchingArrow).gameObject.SetActive(true);
			((Component)_researchingArrow).transform.localPosition = Vector3.Lerp(Vector2.op_Implicit(val), Vector2.op_Implicit(val2), 0.5f) + ((Component)widget).transform.localPosition;
		}
	}

	protected override void OnSelected(bool isSelect)
	{
		base.OnSelected(isSelect);
		_selector.SetActive(isSelect);
	}
}
