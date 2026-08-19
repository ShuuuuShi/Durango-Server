using System;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class FavoriteIslandsNode : MonoBehaviour
{
	[SerializeField]
	private GameObject _add;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private GameObject _delete;

	[SerializeField]
	private UILabel _grade;

	[SerializeField]
	private UILabel _name;

	[SerializeField]
	private UILabel _freq;

	[SerializeField]
	private UILabel _level;

	[SerializeField]
	private UITexture _portrait;

	public void Set([NotNull] Action onAdded)
	{
		_contents.SetActive(value: false);
		_add.SetActive(value: true);
		UIEventListener.Get(base.gameObject).onClick = delegate
		{
			onAdded();
		};
	}

	public void Set(string entityId, Action onClicked, Action onDeleted)
	{
		_contents.SetActive(value: true);
		_add.SetActive(value: false);
		_name.text = string.Empty;
		_freq.text = string.Empty;
		_level.text = string.Empty;
		_grade.text = string.Empty;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(PlayerInfo info)
		{
			_name.text = info.Name;
			_freq.text = $"#{info.Freq:0000}";
			_level.text = LocalizeUtil.FormatLevel(info.Level);
			_grade.text = info.PioneerGrade.ToString();
			PortraitBuilder.Set(info.GetPortraitArgument(), _portrait);
		});
		UIEventListener.Get(base.gameObject).onClick = delegate
		{
			onClicked();
		};
		UIEventListener.Get(_delete).onClick = delegate
		{
			onDeleted();
		};
	}
}
