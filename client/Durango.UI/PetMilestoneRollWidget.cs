using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Item;
using Shared.Pet;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetMilestoneRollWidget : MonoBehaviour, IUIInitializable
{
	private struct RollItem
	{
		public float Begin;

		public float End;

		public Color BgColor;

		public Color Color;

		public float FillRatio;

		public string TagId;

		public Messages.PetActiveSkill Skill;

		public void FillInfoWidget(PetMilestoneSelectedInfoWidget widget)
		{
			if (!string.IsNullOrEmpty(TagId))
			{
				widget.Set(TagId);
			}
			else if (!string.IsNullOrEmpty(Skill.SkillId))
			{
				widget.Set(Skill);
			}
			else
			{
				widget.SetEmpty();
			}
		}
	}

	private struct ResultStruct
	{
		public MilestoneResult? Milestone;

		public DrawSkillResult? Skill;

		public bool IsResultItem(RollItem item)
		{
			if (Milestone.HasValue)
			{
				return item.TagId == Milestone.Value.SelectedTagId;
			}
			if (Skill.HasValue)
			{
				return Skill.Value.Skill.SkillId == item.Skill.SkillId && Skill.Value.Skill.Rank == item.Skill.Rank;
			}
			return false;
		}
	}

	[SerializeField]
	private PetMilestoneRollTermTexture _rollTermBase;

	[SerializeField]
	private UISprite _rollTermGradeBase;

	[SerializeField]
	private UISprite _rollTermSeparatorBase;

	private ListObjectPool<PetMilestoneRollTermTexture> _terms;

	private ListObjectPool<UISprite> _grades;

	private ListObjectPool<UISprite> _termSeparators;

	private readonly List<RollItem> _rollItems = new List<RollItem>();

	private bool? _stopRollFlag;

	private UIWidget _widget;

	private bool _isRolling;

	private readonly Observable<float> _rollSpeed = new Observable<float>();

	public Observable<float> RollSpeed => _rollSpeed;

	public event Action<string> TagFocused;

	public event Action<Messages.PetActiveSkill> SkillFocused;

	public event Action EmptyFocused;

	public event Action Unfocused;

	public event Action RollAnimationFinished;

	public event Action<MilestoneResult> MilestoneRollFinished;

	public event Action<DrawSkillResult> DrawSkillRollFinished;

	public event Action RollFailFinished;

	void IUIInitializable.Init()
	{
		_widget = GetComponent<UIWidget>();
		_terms = new ListObjectPool<PetMilestoneRollTermTexture>();
		_terms.BaseObject = _rollTermBase;
		_terms.UseBase = true;
		_terms.Clear();
		_grades = new ListObjectPool<UISprite>();
		_grades.BaseObject = _rollTermGradeBase;
		_grades.UseBase = true;
		_grades.Clear();
		_termSeparators = new ListObjectPool<UISprite>();
		_termSeparators.BaseObject = _rollTermSeparatorBase;
		_termSeparators.UseBase = true;
		_termSeparators.Clear();
	}

	public void Show(MilestoneCandidates candidates)
	{
		_rollItems.Clear();
		float num = 0f;
		Pair<string, float>[] array = candidates.Result;
		Array.Sort(array, PetUtil.TagCandidateComparison);
		for (int i = 0; i < array.Length; i++)
		{
			num += array[i].Item2;
		}
		if (num <= 0f)
		{
			array = new Pair<string, float>[1]
			{
				new Pair<string, float>(null, 1f)
			};
			num = 1f;
		}
		float num2 = 0f;
		for (int j = 0; j < array.Length; j++)
		{
			Pair<string, float> pair = array[j];
			float num3 = pair.Item2 / num;
			if (num3 <= 0f)
			{
				continue;
			}
			RollItem item = default(RollItem);
			item.TagId = pair.Item1;
			float num4 = (360f - (float)array.Length) * num3;
			item.Begin = num2 + 0.5f;
			item.End = num2 + num4 + 0.5f;
			num2 += num4 + 1f;
			item.FillRatio = 1f;
			if (string.IsNullOrEmpty(pair.Item1))
			{
				item.Color = new Color(0.1f, 0.1f, 0.1f);
				item.BgColor = item.Color.WithA(0.15f);
				item.FillRatio = 0.7f;
			}
			else
			{
				Yaml.Tag value;
				TagGrade tagGrade = (SingletonDict<string, Yaml.Tag>.TryGetValue(pair.Item1, out value) ? value.Grade : TagGrade.Negative);
				item.Color = TagData.GetGradeColor(tagGrade);
				item.BgColor = item.Color.WithA(0.15f);
				switch (tagGrade)
				{
				case TagGrade.Invalid:
				case TagGrade.Negative:
					item.FillRatio = 0.7f;
					break;
				case TagGrade.Neutral:
					item.FillRatio = 0.52f;
					break;
				case TagGrade.Positive:
					item.FillRatio = 0.34f;
					break;
				case TagGrade.Rare:
					item.FillRatio = 0.16f;
					break;
				}
			}
			_rollItems.Add(item);
		}
		_rollSpeed.Value = 0f;
		base.transform.localEulerAngles = Vector3.zero;
		StartRollAnimationCoroutine();
	}

	public void Show(List<Pair<Messages.PetActiveSkill, float>> activeSkillCandidates)
	{
		_rollItems.Clear();
		float num = 0f;
		for (int i = 0; i < activeSkillCandidates.Count; i++)
		{
			num += activeSkillCandidates[i].Item2;
		}
		if (num <= 0f)
		{
			return;
		}
		float num2 = 0f;
		for (int j = 0; j < activeSkillCandidates.Count; j++)
		{
			Pair<Messages.PetActiveSkill, float> pair = activeSkillCandidates[j];
			float num3 = pair.Item2 / num;
			if (num3 <= 0f)
			{
				continue;
			}
			RollItem item = default(RollItem);
			item.Skill = pair.Item1;
			float num4 = (360f - (float)activeSkillCandidates.Count) * num3;
			item.Begin = num2 + 0.5f;
			item.End = num2 + num4 + 0.5f;
			num2 += num4 + 1f;
			item.FillRatio = 1f;
			if (string.IsNullOrEmpty(pair.Item1.SkillId))
			{
				item.Color = new Color(0.1f, 0.1f, 0.1f);
				item.BgColor = item.Color.WithA(0.15f);
				item.FillRatio = 0.7f;
			}
			else
			{
				switch (pair.Item1.Rank)
				{
				case SkillRank.Invalid:
				case SkillRank.D:
				case SkillRank.C:
				case SkillRank.B:
				case SkillRank.A:
					item.Color = Color.white;
					item.FillRatio = 0.52f;
					break;
				case SkillRank.S:
					item.Color = PresetColor.UIYellow;
					item.FillRatio = 0.34f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				item.BgColor = item.Color.WithA(0.15f);
			}
			_rollItems.Add(item);
		}
		_rollSpeed.Value = 0f;
		base.transform.localEulerAngles = Vector3.zero;
		StartRollAnimationCoroutine();
	}

	public void StartRollAnimationCoroutine()
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(RollAnimationCoroutine(1f));
		}
	}

	private IEnumerator RollAnimationCoroutine(float duration)
	{
		_widget.alpha = 1f;
		float start = Time.time;
		float end = start + duration;
		_terms.Clear();
		_grades.Clear();
		_termSeparators.Clear();
		while (true)
		{
			float now = Time.time;
			float ratio2 = Mathf.Clamp01((now - start) / (end - start));
			ratio2 = 1f - (1f - ratio2) * (1f - ratio2);
			float angle = 360f * ratio2;
			GetRollRingSize(out var ringSize, out var ringPadding);
			for (int i = 0; i < _rollItems.Count; i++)
			{
				RollItem rollItem = _rollItems[i];
				if (angle < rollItem.Begin)
				{
					break;
				}
				float begin = rollItem.Begin;
				float num = Mathf.Min(rollItem.End, angle);
				PetMilestoneRollTermTexture orAdd = _terms.GetOrAdd(i);
				orAdd.DrawArc(90f - num, 90f - begin, 0f - (ringSize + ringPadding * 2f), 0f, rollItem.BgColor, 0f - (ringSize * rollItem.FillRatio + ringPadding), 0f - ringPadding, rollItem.Color);
				if (!(angle < rollItem.End))
				{
					UISprite orAdd2 = _grades.GetOrAdd(i);
					orAdd2.color = rollItem.Color;
					if (rollItem.BgColor.a <= 0f)
					{
						orAdd2.alpha = 0f;
					}
					float num2 = Mathf.Lerp(rollItem.Begin, rollItem.End, 0.5f);
					orAdd2.transform.localEulerAngles = new Vector3(0f, 0f, 0f - num2);
					num2 = 90f - num2;
					Vector3 vector = new Vector3(Mathf.Cos(num2 * ((float)Math.PI / 180f)), Mathf.Sin(num2 * ((float)Math.PI / 180f)));
					float num3 = (float)Mathf.Min(_widget.width, _widget.height) * 0.5f;
					orAdd2.transform.localPosition = (num3 - (ringSize + ringPadding * 2f) * 0.8f) * vector;
					orAdd2.transform.localScale = Vector3.one * (ringSize * 0.1f) / orAdd2.width;
					UISprite orAdd3 = _termSeparators.GetOrAdd(i);
					RollItem rollItem2 = _rollItems[(i + 1) % _rollItems.Count];
					float num4 = Mathf.Lerp(rollItem.End, (!(rollItem2.Begin < rollItem.End)) ? rollItem2.Begin : (rollItem2.Begin + 360f), 0.5f);
					orAdd3.transform.localEulerAngles = new Vector3(0f, 0f, 0f - num4);
					num4 = 90f - num4;
					Vector3 vector2 = new Vector3(Mathf.Cos(num4 * ((float)Math.PI / 180f)), Mathf.Sin(num4 * ((float)Math.PI / 180f)));
					float num5 = (float)Mathf.Min(_widget.width, _widget.height) * 0.5f;
					orAdd3.transform.localPosition = num5 * vector2;
					orAdd3.transform.localScale = Vector3.one * (ringSize * 0.25f) / orAdd3.height;
				}
			}
			_terms.Set(_terms.Count);
			_grades.Set(_grades.Count);
			_termSeparators.Set(_termSeparators.Count);
			if (ratio2 < 1f)
			{
				yield return null;
				continue;
			}
			break;
		}
		if (this.RollAnimationFinished != null)
		{
			this.RollAnimationFinished();
		}
	}

	public void PlayRoll([NotNull] Action<Action<object>> requestResult)
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(RollCoroutine(requestResult));
		}
	}

	public bool StopRoll()
	{
		if (_stopRollFlag.HasValue)
		{
			_stopRollFlag = true;
			return true;
		}
		return false;
	}

	private IEnumerator RollCoroutine([NotNull] Action<Action<object>> requestResult)
	{
		_isRolling = true;
		_stopRollFlag = false;
		ResultStruct? result = null;
		float? targetAngle = null;
		requestResult(delegate(object rs)
		{
			if (rs == null)
			{
				_stopRollFlag = true;
				result = default(ResultStruct);
			}
			else if (rs is MilestoneResult)
			{
				result = new ResultStruct
				{
					Milestone = (MilestoneResult)rs
				};
			}
			else if (rs is DrawSkillResult)
			{
				result = new ResultStruct
				{
					Skill = (DrawSkillResult)rs
				};
			}
		});
		float angle = base.transform.localEulerAngles.z;
		float speed = 180f;
		while (true)
		{
			yield return null;
			if (targetAngle.HasValue)
			{
				float num = Mathf.Repeat(targetAngle.Value - angle, 360f);
				float num2;
				while (true)
				{
					num2 = speed * speed / (2f * num);
					if (num2 < 360f)
					{
						break;
					}
					num += 360f;
				}
				speed -= num2 * Time.deltaTime;
			}
			angle = ((targetAngle.HasValue && !(speed > 1f)) ? targetAngle.Value : Mathf.Repeat(angle + Time.deltaTime * speed, 360f));
			base.transform.localEulerAngles = new Vector3(0f, 0f, angle);
			for (int i = 0; i < _rollItems.Count; i++)
			{
				RollItem value = _rollItems[i];
				if (value.Begin <= angle && angle < value.End)
				{
					OnItemFocused(value);
					break;
				}
			}
			if (speed <= 1f)
			{
				break;
			}
			_rollSpeed.Value = speed;
			if (!_stopRollFlag.HasValue || !_stopRollFlag.Value || !result.HasValue)
			{
				continue;
			}
			_stopRollFlag = null;
			for (int j = 0; j < _rollItems.Count; j++)
			{
				RollItem item = _rollItems[j];
				if (result.Value.IsResultItem(item))
				{
					targetAngle = Mathf.Lerp(item.Begin, item.End, UnityEngine.Random.value);
				}
			}
			if (!targetAngle.HasValue)
			{
				_isRolling = false;
				if (this.RollFailFinished != null)
				{
					this.RollFailFinished();
				}
				yield break;
			}
		}
		_rollSpeed.Value = 0f;
		yield return new WaitForSeconds(0.5f);
		_isRolling = false;
		if (!result.HasValue)
		{
			yield break;
		}
		if (result.Value.Milestone.HasValue)
		{
			if (this.MilestoneRollFinished != null)
			{
				this.MilestoneRollFinished(result.Value.Milestone.Value);
			}
		}
		else if (result.Value.Skill.HasValue && this.DrawSkillRollFinished != null)
		{
			this.DrawSkillRollFinished(result.Value.Skill.Value);
		}
	}

	private void GetRollRingSize(out float ringSize, out float ringPadding)
	{
		float num = (float)Mathf.Min(_widget.width, _widget.height) * 0.5f;
		ringSize = num / 5f;
		ringPadding = num / 40f;
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		if (_isRolling)
		{
			return;
		}
		if (press)
		{
			Vector2 vector = NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, base.transform);
			float magnitude = vector.magnitude;
			GetRollRingSize(out var ringSize, out var ringPadding);
			float num = (float)Mathf.Min(_widget.width, _widget.height) * 0.5f;
			float num2 = num - (ringSize + ringPadding * 2f);
			float num3 = num;
			if (magnitude < num2 || magnitude > num3)
			{
				return;
			}
			float num4 = Mathf.Repeat(90f - Mathf.Atan2(vector.y, vector.x) * 57.29578f, 360f);
			for (int i = 0; i < _rollItems.Count; i++)
			{
				RollItem value = _rollItems[i];
				if (value.Begin <= num4 && num4 < value.End)
				{
					OnItemFocused(value);
					break;
				}
			}
		}
		else if (this.Unfocused != null)
		{
			this.Unfocused();
		}
	}

	private void OnItemFocused(RollItem? item)
	{
		if (item.HasValue)
		{
			if (!string.IsNullOrEmpty(item.Value.TagId))
			{
				if (this.TagFocused != null)
				{
					this.TagFocused(item.Value.TagId);
				}
				return;
			}
			if (!string.IsNullOrEmpty(item.Value.Skill.SkillId))
			{
				if (this.SkillFocused != null)
				{
					this.SkillFocused(item.Value.Skill);
				}
				return;
			}
		}
		if (this.EmptyFocused != null)
		{
			this.EmptyFocused();
		}
	}
}
