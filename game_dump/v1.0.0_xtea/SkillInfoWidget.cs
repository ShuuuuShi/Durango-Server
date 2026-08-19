using System;
using System.Collections.Generic;
using SkillData;
using UnityEngine;

public class SkillInfoWidget : MonoBehaviour
{
	[SerializeField]
	private SkillTreeWidget _skillTree;

	[SerializeField]
	private SkillNodeInfoWidget _skillNodeInfo;

	private bool _instantLayoutUpdate;

	private bool _isInit;

	public event Action<SkillNode> OnLearnSkill
	{
		add
		{
			_skillNodeInfo.OnLearnSkill += value;
		}
		remove
		{
			_skillNodeInfo.OnLearnSkill -= value;
		}
	}

	public event Action<SkillNode> OnUntrainSkill
	{
		add
		{
			_skillNodeInfo.OnUntrainSkill += value;
		}
		remove
		{
			_skillNodeInfo.OnUntrainSkill -= value;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_skillTree.SkillSelected += OnSelectSkill;
		}
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnSkillListUpdate;
	}

	private void OnSkillListUpdate()
	{
		_skillTree.UpdateData();
		_skillNodeInfo.UpdateData();
	}

	public void Show(IList<SkillBundle> skills)
	{
		Init();
		if (skills == null || skills.Count == 0)
		{
			_skillTree.Hide();
		}
		else
		{
			_skillTree.Set(skills);
		}
		_skillNodeInfo.Hide(instant: true);
	}

	public void SelectSkill(string id, string subId, int level, bool instant)
	{
		_instantLayoutUpdate = instant;
		_skillTree.SelectSkill(id, subId, level);
		_instantLayoutUpdate = false;
	}

	private void OnSelectSkill(SkillNode skill)
	{
		if (skill == null)
		{
			_skillNodeInfo.Hide(instant: false);
		}
		else
		{
			_skillNodeInfo.Show(skill);
		}
		_skillTree.Resize(_skillNodeInfo.IsShow ? (_skillNodeInfo.AnimWidget.Widget.width + 10) : 0, _instantLayoutUpdate);
	}
}
