using Durango.Logic.Item;

namespace Durango.Logic.PlayGuide;

public abstract class FlowCondition
{
	private TagEvaluator _tagEval;

	private bool _isRegistered;

	public string Param { protected get; set; }

	public bool SkipLoad { get; set; }

	public bool CanRestart { get; set; }

	public FlowRegion Region { get; set; }

	protected TagEvaluator TagEval
	{
		get
		{
			if (_tagEval != null)
			{
				return _tagEval;
			}
			return _tagEval = new TagEvaluator(Param);
		}
	}

	public string Name { get; set; }

	public void TryRegister()
	{
		if (!_isRegistered)
		{
			_isRegistered = true;
			OnRegister();
		}
	}

	public void TryUnregister()
	{
		if (_isRegistered)
		{
			OnUnregister();
			_isRegistered = false;
		}
	}

	protected virtual void OnRegister()
	{
	}

	protected virtual void OnUnregister()
	{
	}

	public virtual void Process()
	{
	}

	protected void Interrupt()
	{
		GameSystem<PlayGuideSystem>.Instance().BeginFlow(Name);
	}
}
