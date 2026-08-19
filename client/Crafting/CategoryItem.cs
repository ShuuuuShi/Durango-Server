using Durango.Logic.Notification;

namespace Crafting;

public abstract class CategoryItem : INotificationable
{
	public string Id;

	public string Name;

	public string Category;

	public string Icon;

	public string Season;

	public bool IsSeasonalRegionBounded;

	private Toggle _notification;

	public bool Available { get; set; }

	public bool Like { get; set; }

	public Notification Notification
	{
		get
		{
			if (_notification == null)
			{
				_notification = new Toggle(Type.Normal, $"{GetType().Name}:{Id}");
			}
			return _notification;
		}
	}
}
