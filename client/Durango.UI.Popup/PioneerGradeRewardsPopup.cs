using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class PioneerGradeRewardsPopup : TooltipBase
{
	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private UISprite _barBg;

	[SerializeField]
	private UISprite _bar;

	[SerializeField]
	private UISprite _circle;

	[SerializeField]
	private UILabel _bottomLabel;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void FillData()
	{
		UpdateNodes();
		UpdateBar();
		_bottomLabel.text = ((GameSystem<StatisticsSystem>.Instance().Level < 60) ? T._("<em>개척도는 캐릭터 {0} 부터 개인통신소</em> <weak><help>{1}</help></weak> <em>건물</em><weak>을 통해 올릴 수 있습니다.</weak>", LocalizeUtil.FormatLevel(60), T._("{0} 이 되면 개인 통신소를 건설할 수 있습니다.", LocalizeUtil.FormatLevel(60))) : T._("<weak>개인섬 개척도가 높을수록 고유 건물들을 통해 다양한 효과들을 누릴 수 있습니다</weak>"));
	}

	private void UpdateNodes()
	{
		PioneerGradeInfo cur = GameSystem<EstateSystem>.Instance().PioneerGradeInfo;
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		PioneerGradeReward[] rewards = Singleton<PioneerGradeRewards>.Instance.Rewards;
		foreach (PioneerGradeReward pioneerGradeReward in rewards)
		{
			GameObject next = nodes.GetNext();
			int grade = pioneerGradeReward.Grade;
			UILabel uILabel = next.FindComponent<UILabel>("GradeLabel");
			uILabel.text = T._("{0}등급", grade);
			uILabel.color = ((grade != cur.Grade) ? Color.white : Color.black);
			UISprite uISprite = next.FindComponent<UISprite>("GradeBG");
			uISprite.color = ((grade != cur.Grade) ? PresetColor.UIBlackAlpha40 : PresetColor.UIYellow);
			UILabel uILabel2 = next.FindComponent<UILabel>("Description");
			uILabel2.text = ((grade <= cur.Grade) ? pioneerGradeReward.Texts.After : pioneerGradeReward.Texts.Before);
		}
		nodes.EndLoad();
		UIUtility.UpdateAnchors(_scrollView.transform);
		_scrollView.UpdateLayout();
		int index = Singleton<PioneerGradeRewards>.Instance.Rewards.IndexOf((PioneerGradeReward x) => x.Grade == cur.Grade);
		_scrollView.MoveToVisibleArea(index, instant: true);
	}

	private void UpdateBar()
	{
		PioneerGradeInfo cur = GameSystem<EstateSystem>.Instance().PioneerGradeInfo;
		int num = Singleton<PioneerGradeRewards>.Instance.Rewards.IndexOf((PioneerGradeReward x) => x.Grade == cur.Grade);
		float y = 0f - (_scrollView.GetNodeOffset(num) + 70f);
		_circle.transform.localPosition = _circle.transform.localPosition.WithY(y);
		int nextGradePoint = Singleton<Pioneer>.Instance.GetNextGradePoint(cur.Grade);
		float t = ((nextGradePoint <= 0) ? 0f : (cur.Point / (float)nextGradePoint));
		float y2 = _barBg.transform.localPosition.y;
		_barBg.height = (int)(_scrollView.ContentsLength + y2 * 2f);
		float num2 = Mathf.Lerp(_scrollView.GetNodeOffset(num), _scrollView.GetNodeOffset(num + 1), t) + 70f + (float)_circle.height + y2;
		_bar.height = (int)num2;
	}
}
