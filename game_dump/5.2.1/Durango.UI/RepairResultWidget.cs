using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class RepairResultWidget : MonoBehaviour
{
	[Serializable]
	private struct Durability
	{
		public UILabel TextCurrent;

		public UILabel TextMax;

		public string FormatCurrent;

		public string FormatMax;
	}

	[SerializeField]
	private ItemIconTex _itemIconTexture;

	[SerializeField]
	private UISprite _iconArtifact;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private Durability _currentDurability;

	[SerializeField]
	private Durability _deltaDurability;

	[SerializeField]
	private Durability _newDurability;

	private bool _initialized;

	private string _formatName;

	private bool _showResult;

	public bool ShowResult
	{
		get
		{
			return _showResult;
		}
		set
		{
			if (_showResult != value)
			{
				if (value)
				{
					_tweenerPlayer.gameObject.SetActive(value: true);
					_tweenerPlayer.Play();
				}
				else
				{
					_tweenerPlayer.gameObject.SetActive(value: false);
				}
				_showResult = value;
			}
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			_formatName = _textName.text;
			_tweenerPlayer.gameObject.SetActive(value: false);
			_initialized = true;
		}
	}

	public void Refresh([NotNull] ItemData itemData)
	{
		_itemIconTexture.SetIcon(itemData);
		_textName.text = T._(_formatName, itemData.Name, itemData.Level);
		_itemIconTexture.gameObject.SetActive(value: true);
		_iconArtifact.gameObject.SetActive(value: false);
	}

	public void Refresh([NotNull] Artifact artifact)
	{
		_iconArtifact.spriteName = artifact.Blueprint.Icon;
		_textName.text = artifact.LocalizedName;
		_itemIconTexture.gameObject.SetActive(value: false);
		_iconArtifact.gameObject.SetActive(value: true);
	}

	public void RefreshDurability(Gauge gauge, bool isArtifact)
	{
		if (gauge != null)
		{
			float num = gauge.Get();
			float num2 = gauge.Max();
			float num3 = CalculateRepairedDurability(num2, isArtifact);
			RefreshDurabilityText(_currentDurability, num, num2);
			RefreshDurabilityText(_deltaDurability, num3 - num, num2 - num3);
			RefreshDurabilityText(_newDurability, num3, num3);
		}
	}

	private static void RefreshDurabilityText(Durability durability, float current, float max)
	{
		durability.TextCurrent.text = string.Format(durability.FormatCurrent, current);
		durability.TextMax.text = string.Format(durability.FormatMax, max);
	}

	private static float CalculateRepairedDurability(float maxDurability, bool isArtifact)
	{
		return ((!isArtifact) ? Singleton<Constants>.Instance.Repair.Item : Singleton<Constants>.Instance.Repair.Artifact).DurabilityResult.GetFailureDurability(maxDurability);
	}
}
