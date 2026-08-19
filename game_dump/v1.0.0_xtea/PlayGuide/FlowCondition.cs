using ItemSystem;

namespace PlayGuide;

public abstract class FlowCondition
{
	private TagEvaluator _tagEval;

	private bool _isRegistered;

	public string Param { protected get; set; }

	protected TagEvaluator TagEval => (_tagEval == null) ? (_tagEval = new TagEvaluator(Param)) : _tagEval;

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
