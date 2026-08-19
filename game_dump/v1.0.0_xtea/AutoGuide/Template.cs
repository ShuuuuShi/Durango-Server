using Messages;
using PlayGuide;
using Shared.Etc;
using Shared.Guide;

namespace AutoGuide;

public class Template
{
	private readonly ToDoCollection _currentToDo = new ToDoCollection
	{
		NPCType = NPCType.Lama,
		ToDoList = new ToDoBase[1]
	};

	private bool _isGuided;

	public string TitleText { get; set; }

	public bool LastSelected { get; set; }

	public OfferType Key { get; private set; }

	public object Goal { get; private set; }

	public object Todo { get; private set; }

	public TemplateType Type { get; private set; }

	public Difficulty Difficulty { get; private set; }

	public int Point { get; private set; }

	public string PhaseName { get; set; }

	public Template(OfferType key, TodoTemplate template)
	{
		Key = key;
		Goal = template.Goal;
		Todo = template.CurrentTodo;
		Type = template.Type;
		Difficulty = template.Difficulty;
		Point = template.Point;
	}

	public ToDoBase GetToDo()
	{
		return _currentToDo.ToDoList[0];
	}

	public void SetToDo(ToDoBase todo)
	{
		bool isGuided = _isGuided;
		SetGuided(guided: false);
		_currentToDo.ToDoList[0] = todo;
		if (todo != null)
		{
			_currentToDo.Title = TitleText;
			_currentToDo.Order = (int)Key;
			if (isGuided)
			{
				SetGuided(guided: true);
			}
		}
	}

	public bool IsGuided()
	{
		return _isGuided;
	}

	public void SetGuided(bool guided)
	{
		_isGuided = guided;
		if (GetToDo() != null)
		{
			if (_isGuided)
			{
				GameSystem<ToDoListSystem>.Instance().Add(_currentToDo, immediately: true);
			}
			else
			{
				GameSystem<ToDoListSystem>.Instance().Remove(_currentToDo, immediately: true);
			}
		}
	}

	public void Destroy()
	{
		SetToDo(null);
	}
}
