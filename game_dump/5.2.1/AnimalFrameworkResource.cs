using System;
using System.Collections.Generic;
using System.IO;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

public class AnimalFrameworkResource : ScriptableObject, IClipCollectable
{
	private static readonly string[] _normalMotionKeys = new string[5] { "stand", "idle", "eat", "alert", "dead" };

	private static readonly string[] _normalMotion3StateKeys = new string[1] { "sleep_motions" };

	private static readonly string[] _weightedMotionKeys = new string[1] { "rest_motions" };

	private static readonly string[] _weighted3StateMotionKeys = new string[1] { "seq_rest_motions" };

	private static readonly string[] _moveMotionSetKeys = new string[1] { "move_motion_sets" };

	private static readonly string[] _combatMotionKeys = new string[5] { "battle_idle", "battle_stand", "evade", "groggy", "blow" };

	private static readonly string[] _combatDirectionalMotionKeys = new string[1] { "knock_back_motion_map" };

	private static readonly string[] _combatMotion3StateKeys = new string[1] { "knock_down_motions" };

	private static readonly string[] _attackMotionKeys = new string[4] { "attack_normal", "attack_strong", "attack_dash", "counter" };

	private static readonly string[] _activeActionMotionKeys = new string[1] { "active_default" };

	[SerializeField]
	[AniamlFrameworkField("평상시 모션")]
	public List<AnimationElem> normal_motions;

	[SerializeField]
	[AniamlFrameworkField("평상시 모션 - 3 스테이트")]
	public List<AnimationElem3State> normal_3states;

	[SerializeField]
	[AniamlFrameworkField("휴식 모션")]
	public List<AnimationElemWeighted> weighted_motions;

	[SerializeField]
	[AniamlFrameworkField("휴식 모션 - 3 스테이트")]
	public List<AnimationElemWeighted3State> weighted_3states;

	[SerializeField]
	[AniamlFrameworkField("이동 모션 셋")]
	public List<AnimationElemMoveSet> move_sets;

	[SerializeField]
	[AniamlFrameworkField("전투 모션")]
	public List<AnimationElem> comabt_motions;

	[SerializeField]
	[AniamlFrameworkField("전투 모션 - 4 방향")]
	public List<AnimationElemDirectional> combat_directions;

	[SerializeField]
	[AniamlFrameworkField("전투 모션 - 3 스테이트")]
	public List<AnimationElem3State> combat_3states;

	[SerializeField]
	[AniamlFrameworkField("공격 모션")]
	public List<AnimationElemAttack> combat_attacks;

	[SerializeField]
	[AniamlFrameworkField("액티브 스킬 모션")]
	public List<AnimationElem> active_skills;

	[SerializeField]
	[AniamlFrameworkField("기타 모션")]
	public List<AnimationElem> etc_motions;

	private Dictionary<string, AnimationElemBase> _animationElements;

	public void CollectClips(List<AnimationClip> clips)
	{
		DoAnimationElements(delegate(AnimationElemBase elem)
		{
			elem.CollectClips(clips);
		});
	}

	public void CreateNew(string frameworkName, [CanBeNull] List<string> animFbxFiles)
	{
		frameworkName = frameworkName.ToLower();
		MakeDefaultAnimationElem(frameworkName, _normalMotionKeys, ref normal_motions);
		MakeDefaultAnimationElem(frameworkName, _normalMotion3StateKeys, ref normal_3states);
		MakeDefaultAnimationElem(frameworkName, _weightedMotionKeys, ref weighted_motions);
		MakeDefaultAnimationElem(frameworkName, _weighted3StateMotionKeys, ref weighted_3states);
		MakeDefaultAnimationElem(frameworkName, _moveMotionSetKeys, ref move_sets);
		MakeDefaultAnimationElem(frameworkName, _combatMotionKeys, ref comabt_motions);
		MakeDefaultAnimationElem(frameworkName, _combatDirectionalMotionKeys, ref combat_directions);
		MakeDefaultAnimationElem(frameworkName, _combatMotion3StateKeys, ref combat_3states);
		if (animFbxFiles == null)
		{
			MakeDefaultAnimationElem(frameworkName, _attackMotionKeys, ref combat_attacks);
		}
		else
		{
			List<string> list = new List<string>();
			foreach (string animFbxFile in animFbxFiles)
			{
				if (animFbxFile.ContainsIgnoreCase("Attack"))
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(animFbxFile);
					if (fileNameWithoutExtension != null)
					{
						string item = fileNameWithoutExtension.ToLower().Replace("npc@", string.Empty);
						list.Add(item);
					}
				}
			}
			MakeDefaultAnimationElem(frameworkName, list.ToArray(), ref combat_attacks);
		}
		MakeDefaultAnimationElem(frameworkName, _activeActionMotionKeys, ref active_skills);
	}

	public static void MakeDefaultAnimationElem<T>(string frameworkName, string[] keys, ref List<T> pairs) where T : AnimationElemBase, new()
	{
		if (pairs == null)
		{
			pairs = new List<T>();
		}
		foreach (string key in keys)
		{
			T val = new T
			{
				key = key
			};
			val.CreateNew(frameworkName);
			pairs.Add(val);
		}
	}

	private void DoAnimationElements([NotNull] Action<AnimationElemBase> action)
	{
		DoAnimationElements(normal_motions, action);
		DoAnimationElements(normal_3states, action);
		DoAnimationElements(weighted_motions, action);
		DoAnimationElements(weighted_3states, action);
		DoAnimationElements(move_sets, action);
		DoAnimationElements(comabt_motions, action);
		DoAnimationElements(combat_directions, action);
		DoAnimationElements(combat_3states, action);
		DoAnimationElements(combat_attacks, action);
		DoAnimationElements(active_skills, action);
		DoAnimationElements(etc_motions, action);
	}

	private void DoAnimationElements<T>(List<T> elements, Action<AnimationElemBase> action) where T : AnimationElemBase
	{
		foreach (T element in elements)
		{
			action(element);
		}
	}

	[CanBeNull]
	public AnimationElemBase GetAnimationElements(string key)
	{
		if (_animationElements == null)
		{
			_animationElements = new Dictionary<string, AnimationElemBase>();
			DoAnimationElements(delegate(AnimationElemBase elem)
			{
				_animationElements.Add(elem.key, elem);
			});
		}
		if (_animationElements.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}
}
