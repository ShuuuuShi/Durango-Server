namespace PlayGuide;

public class NotifyGuideToDo : ToDoBase
{
	public GuideEvent GuideEvent { get; private set; }

	public NotifyGuideToDo(GuideEvent guideEvent)
	{
		GuideEvent = guideEvent;
	}

	public override bool OnClicked()
	{
		CallComplete();
		return true;
	}
}
