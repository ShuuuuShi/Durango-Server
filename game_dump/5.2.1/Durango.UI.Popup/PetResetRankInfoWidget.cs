using System.Collections.Generic;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Animal;
using UnityEngine;

namespace Durango.UI.Popup;

public class PetResetRankInfoWidget : AnimationWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _rankLabel;

	[SerializeField]
	private UILabel _tagLabel;

	[SerializeField]
	private TweenerPlayer _loopEffectTweener;

	[SerializeField]
	private TweenerPlayer _effectTweener;

	public void Set(string title, PetRank rank, IList<string> tags, bool effectOn)
	{
		_titleLabel.text = title;
		_rankLabel.text = rank.ToString();
		_rankLabel.color = ((rank != PetRank.S) ? Color.white : PresetColor.UIYellow);
		string text = null;
		if (KUtility.GetSize(tags) > 0)
		{
			using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
			StringBuilder value = reusable.Value;
			foreach (string tag in tags)
			{
				if (value.Length > 0)
				{
					value.Append(", ");
				}
				value.AppendFormat("<tag>{0}</tag>", tag);
			}
			text = value.ToString();
		}
		else
		{
			text = string.Format("<tag>{0}</tag>", T._("보너스 속성 없음"));
		}
		_tagLabel.text = text;
		if (_loopEffectTweener.gameObject.SetActiveAnd(effectOn))
		{
			_loopEffectTweener.Play();
		}
	}

	public void PlayEffect(float delay)
	{
		_effectTweener.Play(delay);
	}
}
