using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using Messages;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public class StatusEffectTemplate
{
	[CompilerGenerated]
	private sealed class _003CGetEffects_003Ed__19 : IEnumerator<Messages.EffectDetail>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Messages.EffectDetail _003C_003E2__current;

		public StatusEffectTemplate _003C_003E4__this;

		public int level;

		private int _003Ci_003E5__2;

		private int _003CiMax_003E5__3;

		Messages.EffectDetail IEnumerator<Messages.EffectDetail>.Current
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
		public _003CGetEffects_003Ed__19(int _003C_003E1__state)
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
			StatusEffectTemplate statusEffectTemplate = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				_003CiMax_003E5__3 = KUtility.GetSize(statusEffectTemplate.Effects);
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < _003CiMax_003E5__3)
			{
				EffectDetail effectDetail = statusEffectTemplate.Effects[_003Ci_003E5__2];
				_003C_003E2__current = new Messages.EffectDetail
				{
					Type = effectDetail.Type,
					Key = effectDetail.Key,
					Value = effectDetail.GetValue(level)
				};
				_003C_003E1__state = 1;
				return true;
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

	[JsonProperty(PropertyName = "min_level")]
	public int MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "floating_icon")]
	public string FloatingIcon;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "icon_color")]
	public string IconColor;

	[JsonProperty(PropertyName = "skin_effect")]
	public string SkinEffect;

	[JsonProperty(PropertyName = "expiration_extendable")]
	public bool ExpirationExtendable;

	[JsonProperty(PropertyName = "service")]
	public bool Service;

	[JsonProperty(PropertyName = "ui_group_icon")]
	public string UIGroup;

	[JsonProperty(PropertyName = "screen_effect")]
	public string ScreenEffectName;

	[JsonProperty(PropertyName = "effects")]
	public EffectDetail[] Effects;

	private Expression _expression;

	[JsonProperty(PropertyName = "duration")]
	public string Duration { private get; set; }

	public float GetDuration(int level)
	{
		if (_expression == null)
		{
			_expression = ExpressionParser.Parse(Duration);
		}
		_expression.Parameters["level"] = level;
		return Convert.ToSingle(_expression.Evaluate());
	}

	public IEnumerator<Messages.EffectDetail> GetEffects(int level)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEffects_003Ed__19(0)
		{
			_003C_003E4__this = this,
			level = level
		};
	}
}
