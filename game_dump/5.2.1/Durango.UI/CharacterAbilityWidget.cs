using System;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CharacterAbilityWidget : UIWidget
{
	[SerializeField]
	private KScrollView _scrollView;

	private RepresentType[] _types;

	private bool _isReset = true;

	private bool _isInit;

	private bool IsResistanceAdded => _scrollView.Nodes.Count > _types.Length;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			ListObjectPool nodes = _scrollView.Nodes;
			_types = Enums<RepresentType>.Greater(RepresentType.Invalid);
			nodes.BeginLoad();
			RepresentType[] types = _types;
			foreach (RepresentType representType in types)
			{
				GameObject next = nodes.GetNext();
				next.FindComponent<UILabel>("Key").text = "[icon=" + IconMap.Get(representType) + ":1.25]  " + representType.GetName();
				next.GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
				UIEventListener uIEventListener = UIEventListener.Get(next);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickRepresentType));
			}
			nodes.EndLoad();
			AddResistance();
			GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += AddResistance;
		}
	}

	private void AddResistance()
	{
		if (_isInit && !IsResistanceAdded && Yaml.Util.Singleton<Constants>.Instance.MaxLevels.Player <= GameSystem<StatisticsSystem>.Instance().Level)
		{
			GameObject obj = _scrollView.Nodes.Add();
			string text = string.Format("[icon=icon_skin_hardness:1.25]  {0}", T._("신체 저항 레벨"));
			obj.FindComponent<UILabel>("Key").text = text;
			obj.GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				CharacterInfoGroup.ShowResistanceInfoPopup();
			});
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_isReset = true;
		}
	}

	public void Refersh()
	{
		Init();
		Statistics? statistics = GameSystem<StatisticsSystem>.Instance().Statistics;
		int size = KUtility.GetSize(_types);
		for (int i = 0; i < size; i++)
		{
			UILabel uILabel = _scrollView.Nodes[i].FindComponent<UILabel>("Value");
			float num = (statistics.HasValue ? statistics.Value.RepresentPowers.Get(_types[i], 0f) : 0f);
			uILabel.text = ((int)num).ToString();
		}
		if (IsResistanceAdded)
		{
			_scrollView.Nodes[size].FindComponent<UILabel>("Value").text = GameSystem<StatisticsSystem>.Instance().GetAverageResistanceLevel().ToString();
		}
		_scrollView.Reposition(_isReset, !_isReset);
		_isReset = false;
	}

	private void OnClickRepresentType(GameObject obj)
	{
		int num = _scrollView.Nodes.IndexOf(obj);
		if (num != -1)
		{
			CharacterInfoGroup.ShowRepresentTypePopup(_types[num]);
		}
	}
}
