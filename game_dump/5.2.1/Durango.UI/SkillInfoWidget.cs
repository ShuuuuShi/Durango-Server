using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class SkillInfoWidget : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CMoveToLaredPrevNode_003Ed__26 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Node skill;

		public float delay;

		public SkillInfoWidget _003C_003E4__this;

		private Node _003Cprev_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CMoveToLaredPrevNode_003Ed__26(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cprev_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			SkillInfoWidget skillInfoWidget = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Cprev_003E5__2 = skill.Parent.Get(skill.Level - 1);
				if (_003Cprev_003E5__2 != null)
				{
					_003C_003E2__current = new WaitForSeconds(delay);
					_003C_003E1__state = 2;
					return true;
				}
				break;
			case 2:
				_003C_003E1__state = -1;
				skillInfoWidget.SelectSkill(_003Cprev_003E5__2.Id, _003Cprev_003E5__2.Sub, _003Cprev_003E5__2.Level);
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private SkillTreeWidget _skillTree;

	[SerializeField]
	private SkillNodeInfoWidget _skillNodeInfo;

	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private SoundEventType _learnedSkillAudio;

	private bool _isInit;

	public SelectableButton LearnButton => _skillNodeInfo.LearnButton;

	public event Action<Node> OnLearnSkill
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

	public event Action<Node> OnUntrainSkill
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

	public event Action<Node> SkillSelected
	{
		add
		{
			_skillTree.SkillSelected += value;
		}
		remove
		{
			_skillTree.SkillSelected -= value;
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

	public void Show(IList<Bundle> skills)
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
		_skillNodeInfo.Hide();
	}

	public void SelectSkill(string id, string subId, int level)
	{
		_skillTree.SelectSkill(id, subId, level);
	}

	public void ScrollTo(string id, string subId, int level)
	{
		_skillTree.ScrollTo(id, subId, level);
	}

	public SkillTreeItem FindSkill(string id, string subId, int level)
	{
		return _skillTree.FindSkill(id, subId, level);
	}

	public void LearnAndSelectNextSkill([NotNull] Node skill)
	{
		Action<bool> onResult = null;
		if (GameSystem<LearningGuideSystem>.Instance().GetSkillLearningState(skill) == Learning.None)
		{
			onResult = delegate(bool success)
			{
				if (success)
				{
					SoundManager.PlayEvent(_learnedSkillAudio);
					Node node = skill.Parent.Get(skill.Level + 1);
					if (node != null)
					{
						SelectSkill(node.Id, node.Sub, node.Level);
					}
				}
			};
		}
		_skillNodeInfo.ShowLoadingRingToLearnButton();
		GameSystem<SkillSystem>.Instance().LearnSkill(skill.Parent, onResult);
	}

	public void UntrainAndSelectPreviousSkill([NotNull] Node skill, [CanBeNull] string voucherId = null)
	{
		_skillNodeInfo.ShowLoadingRingToLearnButton();
		GameSystem<SkillSystem>.Instance().UntrainSkill(skill.Parent, voucherId, delegate(bool ok)
		{
			if (ok)
			{
				StartCoroutine(MoveToLaredPrevNode(skill, 0f));
			}
			else
			{
				_skillNodeInfo.HideLoadingRingToLearnButton();
			}
		});
	}

	private IEnumerator MoveToLaredPrevNode(Node skill, float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CMoveToLaredPrevNode_003Ed__26(0)
		{
			_003C_003E4__this = this,
			skill = skill,
			delay = delay
		};
	}

	private void OnSelectSkill(Node skill)
	{
		bool isShow = _skillNodeInfo.IsShow;
		if (skill == null)
		{
			_skillNodeInfo.Hide();
		}
		else
		{
			_skillNodeInfo.Show(skill);
		}
		if (!isShow || !_skillNodeInfo.IsShow)
		{
			_layout.UpdateLayout();
			UIUtility.UpdateAnchors(base.transform);
			_skillTree.UpdateScrollMovement();
		}
	}
}
