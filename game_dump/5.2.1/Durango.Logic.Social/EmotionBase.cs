using Durango.Logic.Notification;

namespace Durango.Logic.Social;

public abstract class EmotionBase : INotificationable
{
	public readonly string Key;

	public readonly bool Free;

	private readonly bool _purchaseable;

	private bool _available;

	private int? _favoriteIndex;

	private readonly Toggle _notification;

	public bool Available
	{
		get
		{
			return _available;
		}
		set
		{
			_available = value;
			OnDirty();
		}
	}

	public int? FavoriteIndex
	{
		get
		{
			if (Favorite && Available)
			{
				return _favoriteIndex;
			}
			return null;
		}
	}

	public bool Favorite { get; private set; }

	public bool Visible
	{
		get
		{
			if (!Available)
			{
				return _purchaseable;
			}
			return true;
		}
	}

	public Durango.Logic.Notification.Notification Notification => _notification;

	protected EmotionBase(string key, bool free, bool purchaseable)
	{
		Key = key;
		Free = free;
		_purchaseable = purchaseable;
		Available = Free;
		SetFavorite(favorite: true);
		_notification = new Toggle(Type.Important, GetType().Name + ":" + Key);
		_notification.Refresh();
	}

	public void ClearNotification()
	{
		_notification.On = false;
	}

	public void SetFavorite(bool favorite, int? index = null)
	{
		Favorite = favorite;
		_favoriteIndex = index;
	}

	public virtual bool IsSubscribe()
	{
		return Favorite;
	}

	public void MarkAsChanged()
	{
		OnDirty();
	}

	protected virtual void OnDirty()
	{
	}
}
