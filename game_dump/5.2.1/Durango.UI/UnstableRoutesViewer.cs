using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Explore;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class UnstableRoutesViewer : AnimationWidget
{
	[SerializeField]
	private GameObject _prev;

	[SerializeField]
	private GameObject _next;

	[SerializeField]
	private UnstableRoutesBackground _background;

	[SerializeField]
	private ArchipelagoDiscoveryInfos _discoverInfo;

	[SerializeField]
	private TweenAlpha _restrictionWarning;

	[SerializeField]
	private UILabel _restrctionWarningLabel;

	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private GameObject _unstableFactorParent;

	[SerializeField]
	private CenterFixedScrollBar _unstableFactorScrollBar;

	[SerializeField]
	private UILabel _unstableLabel;

	[SerializeField]
	private UnstableFactorNode.Shape Selected;

	[SerializeField]
	private UnstableFactorNode.Shape Deselected;

	private float _prevOffsetRatio;

	private int _prevIndex;

	private bool _wasRestrictionArea;

	private readonly List<UnstableFactorNode> _unstableFactors = new List<UnstableFactorNode>();

	private void Awake()
	{
		_restrictionWarning.gameObject.SetActive(value: false);
		_scroll.Nodes.UseBase = false;
		_unstableLabel.text = T._("[i]불안정 지수[/i]") + "[icon=icon_question_big]";
		UIEventListener.Get(_unstableLabel.gameObject).onClick = WorldMapEnvWidget.ShowUnstableFactorTooltip;
		UIEventListener.Get(_prev).onClick = delegate
		{
			_scroll.MoveToNode(_prevIndex - 1, instant: false);
		};
		UIEventListener.Get(_next).onClick = delegate
		{
			_scroll.MoveToNode(_prevIndex + 1, instant: false);
		};
	}

	private void Update()
	{
		int num = _scroll.GetNodeCount() - 1;
		if (_scroll.Nodes.Count == 0)
		{
			return;
		}
		float offsetRatio = _scroll.OffsetRatio;
		if (!Mathf.Approximately(offsetRatio, _prevOffsetRatio))
		{
			_prevOffsetRatio = offsetRatio;
			for (int i = 0; i < _unstableFactorScrollBar.Nodes.Count; i++)
			{
				float value = Mathf.Abs(offsetRatio - (float)i);
				UnstableFactorNode.Shape shape = Selected.Lerp(value, Deselected);
				_unstableFactors[i].SetShape(shape);
			}
			int num2 = Mathf.Clamp((int)Mathf.Round(offsetRatio), 0, num);
			if (_prevIndex != num2)
			{
				SetUnstableFactorNodeEffect(num2, value: true);
				SetUnstableFactorNodeEffect(_prevIndex, value: false);
				_prevIndex = num2;
				_prev.SetActive(num2 > 0);
				_next.SetActive(num2 < num);
				ArchipelagoRoute archipelagoRoute = _scroll.GetNode(num2).GetComponent<Archipelago>().ArchipelagoRoute;
				SetDiscoverInfo(archipelagoRoute, num != 0);
				SetRestrictionWarning(archipelagoRoute);
			}
		}
	}

	private void SetUnstableFactorNodeEffect(int index, bool value)
	{
		if (0 <= index && index < _unstableFactors.Count)
		{
			_unstableFactors[index].SetEffect(value);
		}
	}

	private void SetDiscoverInfo(ArchipelagoRoute archipelagoRoute, bool isUnstableFactorVisible)
	{
		List<RegionTemplate> list = null;
		Route[] includedRoutes = archipelagoRoute.IncludedRoutes;
		if (includedRoutes != null)
		{
			list = new List<RegionTemplate>();
			Route[] array = includedRoutes;
			for (int i = 0; i < array.Length; i++)
			{
				Durango.Logic.Explore.Region region = array[i].Region();
				if (region != Durango.Logic.Explore.Region.UnknownRegion && region.Template != null)
				{
					list.Add(region.Template);
				}
			}
		}
		_discoverInfo.Set(list, archipelagoRoute, isUnstableFactorVisible);
	}

	private void SetRestrictionWarning(ArchipelagoRoute archipelagoRoute)
	{
		bool flag = !archipelagoRoute.IsAllConditionSatisfied();
		if (_wasRestrictionArea && !flag)
		{
			EventDelegate.Add(_restrictionWarning.onFinished, delegate
			{
				_restrictionWarning.gameObject.SetActive(_wasRestrictionArea);
			}, oneShot: true);
			_restrictionWarning.PlayReverse();
		}
		else if (!_wasRestrictionArea && flag)
		{
			_restrictionWarning.gameObject.SetActive(value: true);
			_restrictionWarning.PlayForward();
		}
		_wasRestrictionArea = flag;
		if (!flag)
		{
			return;
		}
		int acceptableGrade = Singleton<Pioneer>.Instance.GetAcceptableGrade(archipelagoRoute.UnstableFactor);
		Derived derived = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(archipelagoRoute.Biome, Derived.MaxHealth);
		int num = SingletonDict<int, Recommends>.Instance.Get(archipelagoRoute.UnstableFactor)?.RequiredResistanceLevel ?? 0;
		List<string> list = new List<string>();
		if (!archipelagoRoute.IsEpic)
		{
			if (acceptableGrade > 0)
			{
				string text = T._("<em>개인섬 개척도 {0}</em> 달성", acceptableGrade);
				text = ((!archipelagoRoute.IsPioneerGradeSatisfied()) ? text : ("[s]" + text + "[/s]"));
				list.Add(text);
			}
			if (num > 0)
			{
				string text2 = T._("<em>신체 {0} {1}</em> 달성", derived.GetName(), LocalizeUtil.FormatLevel(num));
				text2 = ((!archipelagoRoute.IsResistanceLevelSatisfied()) ? text2 : ("[s]" + text2 + "[/s]"));
				list.Add(text2);
			}
			if (archipelagoRoute.UnstableFactor > 1)
			{
				string text3 = T._("<em>개척 임무({0})</em> 완료", archipelagoRoute.UnstableFactor - 1);
				text3 = ((!archipelagoRoute.IsClearedUnstableFactorSatisfied()) ? text3 : ("[s]" + text3 + "[/s]"));
				list.Add(text3);
			}
		}
		if (archipelagoRoute.PrerequisiteQuest.HasValue)
		{
			QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(archipelagoRoute.PrerequisiteQuest.Value.Item1);
			if (questYml != null)
			{
				string text4 = T._("<em>선행 퀘스트 `{0}`</em> 완료", questYml.Subject);
				text4 = ((!archipelagoRoute.IsPrerequisiteQuestFinished()) ? text4 : ("[s]" + text4 + "[/s]"));
				list.Add(text4);
			}
		}
		_restrctionWarningLabel.text = string.Join("\n", list.ToArray());
		UIUtility.UpdateAnchors(_restrctionWarningLabel.transform);
	}

	public void Set(RegionTemplate template)
	{
		bool active = false;
		if (template.Role == Role.Outpost)
		{
			_scroll.Nodes.Set(1);
			Archipelago component = _scroll.Nodes[0].GetComponent<Archipelago>();
			Route[] outpostRoute = GameSystem<ExploreSystem>.Instance().GetOutpostRoute(template);
			component.SetRegion(template, outpostRoute);
		}
		else
		{
			List<ArchipelagoRoute> archipelagoRoutes = GameSystem<ExploreSystem>.Instance().GetArchipelagoRoutes(template.Level, template.MajorBiome());
			active = archipelagoRoutes.Count > 1;
			_scroll.Nodes.BeginLoad();
			_unstableFactorScrollBar.Nodes.BeginLoad();
			_unstableFactors.Clear();
			int num = ((_prevIndex != -1) ? _prevIndex : 0);
			foreach (ArchipelagoRoute item in archipelagoRoutes.OrderBy((ArchipelagoRoute route) => route.UnstableFactor))
			{
				Archipelago component2 = _scroll.Nodes.GetNext().GetComponent<Archipelago>();
				component2.SetArchipelago(item);
				UnstableFactorNode component3 = _unstableFactorScrollBar.Nodes.GetNext().GetComponent<UnstableFactorNode>();
				object obj;
				if (item.IsAllConditionSatisfied())
				{
					int unstableFactor = item.UnstableFactor;
					obj = unstableFactor.ToString();
				}
				else
				{
					obj = null;
				}
				string unstableFactor2 = (string)obj;
				bool flag = _unstableFactors.Count == num;
				_unstableFactors.Add(component3);
				component3.Set(unstableFactor2);
				component3.SetShape((!flag) ? Deselected : Selected);
				component3.SetEffect(flag);
				component3.SetMission(component2.HasAnyMission);
			}
			_scroll.Nodes.EndLoad();
			_unstableFactorScrollBar.Nodes.EndLoad();
			_unstableFactorScrollBar.UpdateLayout();
		}
		_scroll.ResetPosition();
		_scroll.MoveToNode(_prevIndex, instant: true);
		_prevIndex = -1;
		_prevOffsetRatio = float.MinValue;
		bool flag2 = _scroll.Nodes.Count == 1;
		_background.SetCompass(flag2);
		_scroll.GetComponent<UIScrollView>().enabled = !flag2;
		_unstableFactorParent.gameObject.SetActive(active);
		_discoverInfo.SetSubject(template.ApparentClimate, template.Level);
	}

	public void Show(float duration, float delay)
	{
		base.Delay = delay;
		base.Duration = duration;
		base.Alpha = 1f;
	}

	public void Hide(float duration)
	{
		base.Duration = duration;
		base.Alpha = 0f;
	}

	public Transform GetIslandTransform()
	{
		return _scroll.GetNode(_scroll.GetGoalNodeIndex()).GetComponent<Archipelago>().GetIslandTransform();
	}
}
