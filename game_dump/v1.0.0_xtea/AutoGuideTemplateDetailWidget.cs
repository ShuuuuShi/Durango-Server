using AutoGuide;
using L10N;
using Messages;
using PlayGuide;
using UnityEngine;

public class AutoGuideTemplateDetailWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _templateName;

	[SerializeField]
	private UILabel _templateExplain;

	[SerializeField]
	private GameObject _guidedContext;

	[SerializeField]
	private UILabel _todoName;

	[SerializeField]
	private UILabel _todoExplain;

	[SerializeField]
	private DefaultSelectableButton _guidedButton;

	[SerializeField]
	private DefaultSelectableButton _actionButton;

	private Template _template;

	private void Awake()
	{
		_guidedButton.Clicked = delegate
		{
			if (_template != null)
			{
				GameSystem<AutoGuideSystem>.Instance().SetGuided(_template.Key, !_template.IsGuided());
			}
		};
		_actionButton.Clicked = delegate
		{
			if (_template != null)
			{
				GameSystem<AutoGuideSystem>.Instance().DoAction(_template);
			}
		};
	}

	public void Set(Template template)
	{
		_template = template;
		if (_template == null)
		{
			return;
		}
		_templateName.text = _template.TitleText;
		bool flag = _template.IsGuided();
		_guidedContext.SetActive(flag);
		((Component)_templateExplain).gameObject.SetActive(!flag);
		int num = 0;
		if (flag)
		{
			_todoName.text = T._("현재 단계: <em>{0}</em>", _template.PhaseName);
			ToDoBase toDo = template.GetToDo();
			_todoExplain.text = ((toDo == null) ? string.Empty : toDo.Tooltip);
			((Component)_actionButton).gameObject.SetActive(true);
			if (toDo is GetSlotItemToDo || toDo is GetItemToDo)
			{
				_actionButton.Text = T._("장터에서 찾기");
			}
			else if (toDo is BuildToDo || toDo is CraftToDo)
			{
				_actionButton.Text = T._("제작하기");
			}
			else if (toDo is LearnSkillToDo)
			{
				_actionButton.Text = T._("스킬 배우기");
			}
			else
			{
				((Component)_actionButton).gameObject.SetActive(false);
			}
			if (((Component)_actionButton).gameObject.activeSelf)
			{
				UIWidget component = ((Component)_actionButton).GetComponent<UIWidget>();
				num = component.width;
			}
		}
		else
		{
			string text = string.Empty;
			object goal = template.Goal;
			if (goal is BuildGoal buildGoal)
			{
				text = T._("<em>{0}</em> 숙련에 유리한 건설", LocalizeUtil.Get(buildGoal.Category));
			}
			else if (goal is CraftGoal craftGoal)
			{
				text = T._("<em>{0}</em> 숙련에 유리한 제작", LocalizeUtil.Get(craftGoal.Category));
			}
			else if (goal is SkillGoal skillGoal)
			{
				text = T._("목표 달성률 <em>{0:P1}</em> 증가", skillGoal.Progress);
			}
			else if (goal is HuntGoal)
			{
				UseActionTodo useActionTodo = (UseActionTodo)template.Todo;
				text = T._("<em>{0}</em>{0:-을} 사용 해보기", useActionTodo.ActionName);
			}
			_templateExplain.text = text;
		}
		_guidedButton.TextLabel.text = ((!flag) ? T._("안내 보기") : T._("안내 중지"));
		_guidedButton.SetStyle((!flag) ? DefaultSelectableButton.ButtonStyle.Yellow : DefaultSelectableButton.ButtonStyle.Gray);
		UIWidget component2 = ((Component)_guidedButton).GetComponent<UIWidget>();
		component2.rightAnchor.absolute = -18 - num;
		((Component)this).BroadcastMessage("UpdateAnchors");
	}
}
