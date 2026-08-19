using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class MiniGameDanceNote : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CFlyingSequence_003Ed__6 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MiniGameDanceNote _003C_003E4__this;

		public MiniGameDanceAsset.DanceNoteData data;

		public UIWidget target;

		public float startTime;

		public Action<float, MiniGameDanceNote, bool> destroyCallback;

		private Vector3 _003CspawnPos_003E5__2;

		private float _003CreachAt_003E5__3;

		private float _003CfadeSince_003E5__4;

		private Vector3 _003CpreviousVelocity_003E5__5;

		private float _003Ci_003E5__6;

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
		public _003CFlyingSequence_003Ed__6(int _003C_003E1__state)
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
			MiniGameDanceNote miniGameDanceNote = _003C_003E4__this;
			float elapsedTime;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				miniGameDanceNote.SetIcon(data);
				float f = (float)Math.PI;
				_003CspawnPos_003E5__2 = target.transform.position + new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * 4f;
				miniGameDanceNote.transform.position = _003CspawnPos_003E5__2;
				_003CreachAt_003E5__3 = startTime + data.TimeKey;
				elapsedTime = MiniGameDanceHelper.ElapsedTime;
				goto IL_010c;
			}
			case 1:
				_003C_003E1__state = -1;
				elapsedTime = MiniGameDanceHelper.ElapsedTime;
				goto IL_010c;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__6 += MiniGameDanceHelper.DeltaTime;
				goto IL_01ca;
			case 3:
				{
					_003C_003E1__state = -1;
					destroyCallback(data.TimeKey, miniGameDanceNote, arg3: true);
					return false;
				}
				IL_010c:
				if (elapsedTime < _003CreachAt_003E5__3)
				{
					float num2 = (_003CreachAt_003E5__3 - elapsedTime) / data.TransitionTime;
					miniGameDanceNote.transform.position = Vector3.Lerp(_003CspawnPos_003E5__2, target.transform.position, 1f - num2);
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				_003CfadeSince_003E5__4 = MiniGameDanceHelper.ElapsedTime;
				_003CpreviousVelocity_003E5__5 = (target.transform.position - _003CspawnPos_003E5__2) / data.TransitionTime;
				_003Ci_003E5__6 = _003CfadeSince_003E5__4;
				goto IL_01ca;
				IL_01ca:
				if (_003Ci_003E5__6 < _003CfadeSince_003E5__4 + 0.5f)
				{
					float num3 = (MiniGameDanceHelper.ElapsedTime - _003CfadeSince_003E5__4) / 0.5f;
					miniGameDanceNote.transform.Translate(_003CpreviousVelocity_003E5__5 * MiniGameDanceHelper.DeltaTime);
					miniGameDanceNote._arrowSprite.alpha = 1f - num3;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003E2__current = new WaitForSeconds(0.5f);
				_003C_003E1__state = 3;
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

	private const float FadeOutTime = 0.5f;

	private const float NoteSpawnDistance = 4f;

	[SerializeField]
	private UISprite _arrowSprite;

	[SerializeField]
	private UISprite _dotSprite;

	private ICoroutineBinder _sequence;

	public void Set(float startTime, MiniGameDanceAsset.DanceNoteData danceNoteData, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		base.gameObject.SetActive(value: true);
		this.StartCoroutine(ref _sequence, FlyingSequence(startTime, danceNoteData, target, destroyCallback));
	}

	private IEnumerator FlyingSequence(float startTime, MiniGameDanceAsset.DanceNoteData data, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFlyingSequence_003Ed__6(0)
		{
			_003C_003E4__this = this,
			startTime = startTime,
			data = data,
			target = target,
			destroyCallback = destroyCallback
		};
	}

	private void SetIcon(MiniGameDanceAsset.DanceNoteData data)
	{
		if ((data.Pattern & MiniGameDanceAsset.DanceNoteData.Type.Dot) != 0)
		{
			_dotSprite.enabled = true;
			_arrowSprite.enabled = false;
			_dotSprite.color = GetColorByArrow(data);
			_dotSprite.alpha = 1f;
		}
		else if ((data.Pattern & MiniGameDanceAsset.DanceNoteData.ArrowPattern) != 0)
		{
			_dotSprite.enabled = false;
			_arrowSprite.enabled = true;
			_arrowSprite.color = GetColorByArrow(data);
			_arrowSprite.alpha = 1f;
			float rotation = MiniGameDanceHelper.GetRotation(data.Pattern);
			_arrowSprite.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
		}
	}

	private Color GetColorByArrow(MiniGameDanceAsset.DanceNoteData danceNoteData)
	{
		return danceNoteData.Pattern switch
		{
			MiniGameDanceAsset.DanceNoteData.Type.Left => PresetColor.Aqua, 
			MiniGameDanceAsset.DanceNoteData.Type.Right => Color.yellow, 
			MiniGameDanceAsset.DanceNoteData.Type.Up => PresetColor.Lima, 
			MiniGameDanceAsset.DanceNoteData.Type.Down => PresetColor.WildStrawberry, 
			MiniGameDanceAsset.DanceNoteData.Type.Dot => PresetColor.SpringGreen, 
			_ => Color.white, 
		};
	}

	public void HitAndKillObject(float timeKey, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		this.StopCoroutine(_sequence);
		destroyCallback(timeKey, this, arg3: false);
	}
}
