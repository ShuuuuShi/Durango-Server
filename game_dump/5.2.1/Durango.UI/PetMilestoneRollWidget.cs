using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
				if (Skill.Value.Skill.SkillId == item.Skill.SkillId)
				{
					return Skill.Value.Skill.Rank == item.Skill.Rank;
				}
				return false;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass46_0
	{
		public PetMilestoneRollWidget _003C_003E4__this;

		public ResultStruct? result;

		internal void _003CRollCoroutine_003Eb__0(object rs)
		{
			if (rs == null)
			{
				_003C_003E4__this._stopRollFlag = true;
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
		}
	}

	[CompilerGenerated]
	private sealed class _003CRollAnimationCoroutine_003Ed__43 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetMilestoneRollWidget _003C_003E4__this;

		public float duration;

		private float _003Cstart_003E5__2;

		private float _003Cend_003E5__3;

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
		public _003CRollAnimationCoroutine_003Ed__43(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetMilestoneRollWidget petMilestoneRollWidget = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				petMilestoneRollWidget._widget.alpha = 1f;
				_003Cstart_003E5__2 = Time.time;
				_003Cend_003E5__3 = _003Cstart_003E5__2 + duration;
				petMilestoneRollWidget._terms.Clear();
				petMilestoneRollWidget._grades.Clear();
				petMilestoneRollWidget._termSeparators.Clear();
			}
			float num2 = Mathf.Clamp01((Time.time - _003Cstart_003E5__2) / (_003Cend_003E5__3 - _003Cstart_003E5__2));
			num2 = 1f - (1f - num2) * (1f - num2);
			float num3 = 360f * num2;
			petMilestoneRollWidget.GetRollRingSize(out var ringSize, out var ringPadding);
			for (int i = 0; i < petMilestoneRollWidget._rollItems.Count; i++)
			{
				RollItem rollItem = petMilestoneRollWidget._rollItems[i];
				if (num3 < rollItem.Begin)
				{
					break;
				}
				float begin = rollItem.Begin;
				float num4 = Mathf.Min(rollItem.End, num3);
				petMilestoneRollWidget._terms.GetOrAdd(i).DrawArc(90f - num4, 90f - begin, 0f - (ringSize + ringPadding * 2f), 0f, rollItem.BgColor, 0f - (ringSize * rollItem.FillRatio + ringPadding), 0f - ringPadding, rollItem.Color);
				if (!(num3 < rollItem.End))
				{
					UISprite orAdd = petMilestoneRollWidget._grades.GetOrAdd(i);
					orAdd.color = rollItem.Color;
					if (rollItem.BgColor.a <= 0f)
					{
						orAdd.alpha = 0f;
					}
					float num5 = Mathf.Lerp(rollItem.Begin, rollItem.End, 0.5f);
					orAdd.transform.localEulerAngles = new Vector3(0f, 0f, 0f - num5);
					num5 = 90f - num5;
					Vector3 vector = new Vector3(Mathf.Cos(num5 * ((float)Math.PI / 180f)), Mathf.Sin(num5 * ((float)Math.PI / 180f)));
					float num6 = (float)Mathf.Min(petMilestoneRollWidget._widget.width, petMilestoneRollWidget._widget.height) * 0.5f;
					orAdd.transform.localPosition = (num6 - (ringSize + ringPadding * 2f) * 0.8f) * vector;
					orAdd.transform.localScale = Vector3.one * (ringSize * 0.1f) / orAdd.width;
					UISprite orAdd2 = petMilestoneRollWidget._termSeparators.GetOrAdd(i);
					RollItem rollItem2 = petMilestoneRollWidget._rollItems[(i + 1) % petMilestoneRollWidget._rollItems.Count];
					float num7 = Mathf.Lerp(rollItem.End, (!(rollItem2.Begin < rollItem.End)) ? rollItem2.Begin : (rollItem2.Begin + 360f), 0.5f);
					orAdd2.transform.localEulerAngles = new Vector3(0f, 0f, 0f - num7);
					num7 = 90f - num7;
					Vector3 vector2 = new Vector3(Mathf.Cos(num7 * ((float)Math.PI / 180f)), Mathf.Sin(num7 * ((float)Math.PI / 180f)));
					float num8 = (float)Mathf.Min(petMilestoneRollWidget._widget.width, petMilestoneRollWidget._widget.height) * 0.5f;
					orAdd2.transform.localPosition = num8 * vector2;
					orAdd2.transform.localScale = Vector3.one * (ringSize * 0.25f) / orAdd2.height;
				}
			}
			petMilestoneRollWidget._terms.Set(petMilestoneRollWidget._terms.Count);
			petMilestoneRollWidget._grades.Set(petMilestoneRollWidget._grades.Count);
			petMilestoneRollWidget._termSeparators.Set(petMilestoneRollWidget._termSeparators.Count);
			if (num2 < 1f)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			if (petMilestoneRollWidget.RollAnimationFinished != null)
			{
				petMilestoneRollWidget.RollAnimationFinished();
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

	[CompilerGenerated]
	private sealed class _003CRollCoroutine_003Ed__46 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetMilestoneRollWidget _003C_003E4__this;

		public Action<Action<object>> requestResult;

		private _003C_003Ec__DisplayClass46_0 _003C_003E8__1;

		private float? _003CtargetAngle_003E5__2;

		private float _003Cangle_003E5__3;

		private float _003Cspeed_003E5__4;

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
		public _003CRollCoroutine_003Ed__46(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E8__1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetMilestoneRollWidget petMilestoneRollWidget = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass46_0();
				_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
				petMilestoneRollWidget._isRolling = true;
				petMilestoneRollWidget._stopRollFlag = false;
				_003C_003E8__1.result = null;
				_003CtargetAngle_003E5__2 = null;
				requestResult(delegate(object rs)
				{
					if (rs == null)
					{
						_003C_003E8__1._003C_003E4__this._stopRollFlag = true;
						_003C_003E8__1.result = default(ResultStruct);
					}
					else if (rs is MilestoneResult)
					{
						_003C_003E8__1.result = new ResultStruct
						{
							Milestone = (MilestoneResult)rs
						};
					}
					else if (rs is DrawSkillResult)
					{
						_003C_003E8__1.result = new ResultStruct
						{
							Skill = (DrawSkillResult)rs
						};
					}
				});
				_003Cangle_003E5__3 = petMilestoneRollWidget.transform.localEulerAngles.z;
				_003Cspeed_003E5__4 = 180f;
				goto IL_00b2;
			case 1:
			{
				_003C_003E1__state = -1;
				if (_003CtargetAngle_003E5__2.HasValue)
				{
					float num2 = Mathf.Repeat(_003CtargetAngle_003E5__2.Value - _003Cangle_003E5__3, 360f);
					float num3;
					while (true)
					{
						num3 = _003Cspeed_003E5__4 * _003Cspeed_003E5__4 / (2f * num2);
						if (num3 < 360f)
						{
							break;
						}
						num2 += 360f;
					}
					_003Cspeed_003E5__4 -= num3 * Time.deltaTime;
				}
				_003Cangle_003E5__3 = ((_003CtargetAngle_003E5__2.HasValue && !(_003Cspeed_003E5__4 > 1f)) ? _003CtargetAngle_003E5__2.Value : Mathf.Repeat(_003Cangle_003E5__3 + Time.deltaTime * _003Cspeed_003E5__4, 360f));
				petMilestoneRollWidget.transform.localEulerAngles = new Vector3(0f, 0f, _003Cangle_003E5__3);
				for (int i = 0; i < petMilestoneRollWidget._rollItems.Count; i++)
				{
					RollItem value = petMilestoneRollWidget._rollItems[i];
					if (value.Begin <= _003Cangle_003E5__3 && _003Cangle_003E5__3 < value.End)
					{
						petMilestoneRollWidget.OnItemFocused(value);
						break;
					}
				}
				if (!(_003Cspeed_003E5__4 <= 1f))
				{
					petMilestoneRollWidget._rollSpeed.Value = _003Cspeed_003E5__4;
					if (petMilestoneRollWidget._stopRollFlag.HasValue && petMilestoneRollWidget._stopRollFlag.Value && _003C_003E8__1.result.HasValue)
					{
						petMilestoneRollWidget._stopRollFlag = null;
						for (int j = 0; j < petMilestoneRollWidget._rollItems.Count; j++)
						{
							RollItem item = petMilestoneRollWidget._rollItems[j];
							if (_003C_003E8__1.result.Value.IsResultItem(item))
							{
								_003CtargetAngle_003E5__2 = Mathf.Lerp(item.Begin, item.End, UnityEngine.Random.value);
							}
						}
						if (!_003CtargetAngle_003E5__2.HasValue)
						{
							petMilestoneRollWidget._isRolling = false;
							if (petMilestoneRollWidget.RollFailFinished != null)
							{
								petMilestoneRollWidget.RollFailFinished();
							}
							return false;
						}
					}
					goto IL_00b2;
				}
				petMilestoneRollWidget._rollSpeed.Value = 0f;
				_003C_003E2__current = new WaitForSeconds(0.5f);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				{
					_003C_003E1__state = -1;
					petMilestoneRollWidget._isRolling = false;
					if (!_003C_003E8__1.result.HasValue)
					{
						return false;
					}
					if (_003C_003E8__1.result.Value.Milestone.HasValue)
					{
						if (petMilestoneRollWidget.MilestoneRollFinished != null)
						{
							petMilestoneRollWidget.MilestoneRollFinished(_003C_003E8__1.result.Value.Milestone.Value);
						}
					}
					else if (_003C_003E8__1.result.Value.Skill.HasValue && petMilestoneRollWidget.DrawSkillRollFinished != null)
					{
						petMilestoneRollWidget.DrawSkillRollFinished(_003C_003E8__1.result.Value.Skill.Value);
					}
					return false;
				}
				IL_00b2:
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRollAnimationCoroutine_003Ed__43(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRollCoroutine_003Ed__46(0)
		{
			_003C_003E4__this = this,
			requestResult = requestResult
		};
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
