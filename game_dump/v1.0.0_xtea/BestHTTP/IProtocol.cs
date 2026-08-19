namespace BestHTTP;

public interface IProtocol
{
	bool IsClosed { get; }

	void HandleEvents();
}
