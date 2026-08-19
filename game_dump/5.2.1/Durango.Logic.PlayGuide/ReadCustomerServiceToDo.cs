using L10N;

namespace Durango.Logic.PlayGuide;

public class ReadCustomerServiceToDo : ToDoBase
{
	public ReadCustomerServiceToDo()
	{
		base.LocalText = T._("<em>고객센터</em>를 확인해주세요.");
	}

	public override void OnAddItem()
	{
		GameSystem<CustomerServiceSystem>.Instance().HasUnreadAnswerUpdated += CustomerService_HasUnreadAnswerChanged;
	}

	public override void OnRemoveItem()
	{
		GameSystem<CustomerServiceSystem>.Instance().HasUnreadAnswerUpdated -= CustomerService_HasUnreadAnswerChanged;
	}

	private void CustomerService_HasUnreadAnswerChanged()
	{
		if (!GameSystem<CustomerServiceSystem>.Instance().HasUnreadAnswer)
		{
			CallComplete();
		}
	}
}
