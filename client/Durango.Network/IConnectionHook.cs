namespace Durango.Network;

public interface IConnectionHook
{
	bool HookSendingMessage(Connection connection, uint sequence, object msg, bool noReply, uint replyOf);
}
