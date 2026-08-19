using System.Collections.Generic;

namespace PlayGuide;

public class FlowStack
{
	public string Name;

	public bool Started;

	public Stack<FlowStackItem> Stack;

	public GuideRecoder Recoder;

	public ToDoCollection Notify;
}
