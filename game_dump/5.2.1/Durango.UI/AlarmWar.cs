using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class AlarmWar : MonoBehaviour
{
	public enum Type
	{
		Occupied,
		Unoccupied,
		ProtectedEnded,
		DefenceSuccess,
		AttackFail,
		OtherOccupied
	}

	[Serializable]
	private struct Effect
	{
		public TweenerPlayer Obj;

		public SoundEventType Sound;
	}

	private struct EffectObject
	{
		public bool Valid;

		public TweenerPlayer Object;

		public UILabel Subject;

		public UILabel Comment;

		public SoundEventType Sound;

		public void Play()
		{
			if (Object != null)
			{
				Object.gameObject.SetActive(value: true);
				Object.Play();
			}
			SoundManager.PlayEvent(Sound);
		}

		public void Stop()
		{
			if (Object != null)
			{
				Object.gameObject.SetActive(value: false);
			}
		}
	}

	[SerializeField]
	[EnumList(typeof(Type), false, 0, -1)]
	private Effect[] _effectPrefabs;

	private EffectObject[] _effects;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_effects = new EffectObject[_effectPrefabs.Length];
		}
	}

	[ExposedInEditor(null)]
	public void Show(Type type, object[] subjectArg = null, object[] commentArg = null)
	{
		Init();
		if (!_effects[(int)type].Valid)
		{
			EffectObject effectObject = default(EffectObject);
			effectObject.Valid = true;
			effectObject.Sound = _effectPrefabs[(int)type].Sound;
			TweenerPlayer obj = _effectPrefabs[(int)type].Obj;
			if (obj != null)
			{
				effectObject.Object = base.gameObject.AddChild(obj.gameObject).GetComponent<TweenerPlayer>();
				effectObject.Subject = GetChildLabel(effectObject.Object.transform, "Label_Subject");
				effectObject.Comment = GetChildLabel(effectObject.Object.transform, "Label_Comment");
			}
			_effects[(int)type] = effectObject;
		}
		for (int i = 0; i < _effects.Length; i++)
		{
			if (!_effects[i].Valid)
			{
				continue;
			}
			if (i == (int)type)
			{
				_effects[i].Play();
				GetEffectText(type, out var subject, out var comment);
				if (!string.IsNullOrEmpty(subject) && _effects[i].Subject != null)
				{
					_effects[i].Subject.text = ((subjectArg != null) ? T._(subject, subjectArg) : subject);
				}
				if (!string.IsNullOrEmpty(comment) && _effects[i].Comment != null)
				{
					_effects[i].Comment.text = ((commentArg != null) ? T._(comment, commentArg) : comment);
				}
			}
			else
			{
				_effects[i].Stop();
			}
		}
	}

	private UILabel GetChildLabel(Transform parent, string childName)
	{
		Transform transform = parent.Find(childName);
		if (transform == null)
		{
			return null;
		}
		UILabel component = transform.GetComponent<UILabel>();
		if (component == null)
		{
			return null;
		}
		return component;
	}

	private static void GetEffectText(Type type, out string subject, out string comment)
	{
		switch (type)
		{
		case Type.Occupied:
			subject = T._("거점 점령");
			comment = T._("거점을 점령했습니다!");
			break;
		case Type.Unoccupied:
			subject = T._("방어 실패");
			comment = T._("거점을 빼앗겼습니다.");
			break;
		case Type.ProtectedEnded:
			subject = T._("전쟁 시작");
			comment = T._("보호기간이 종료되었습니다.");
			break;
		case Type.DefenceSuccess:
			subject = T._("방어 성공");
			comment = T._("성공적으로 거점을 방어했습니다.");
			break;
		case Type.AttackFail:
			subject = T._("전쟁 종료");
			comment = T._("이 거점의 전쟁 기간이 종료되었습니다.");
			break;
		case Type.OtherOccupied:
			subject = T._("전쟁 종료");
			comment = T._("<em>{0}</em> 부족이 거점 점령에 성공하였습니다.");
			break;
		default:
			subject = null;
			comment = null;
			break;
		}
	}
}
