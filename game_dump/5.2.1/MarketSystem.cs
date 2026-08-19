using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Yaml;
using Yaml.Util;

public class MarketSystem : GameSystem<MarketSystem>
{
	public const float RegistrationLimitedDurability = 0.9f;

	private int _sequenceCount;

	private readonly Observable<bool> _hasExpired = new Observable<bool>();

	private HashSet<string> _favoritesIds;

	public Category[] CategoryYamlData { get; private set; }

	public Observable<bool> HasExpired => _hasExpired;

	public bool HasCollectiblePayment { get; private set; }

	public bool IsFavoriteSystemOnline => _favoritesIds != null;

	public event Action SuccessItemBuy;

	public event Action<bool> OnProductCollectiblePaymentExists;

	public event Action<ProductSold> OnProductSold;

	public event Action<MarketPaymentReceived> OnProductPaymentReceived;

	public event Action<ProductStateUpdated> OnProductStateUpdated;

	private void Awake()
	{
		Connections.Frontend.On(delegate(MarketCollectablePaymentExists msg, PacketHeader header)
		{
			HasCollectiblePayment = msg.Exists;
			if (this.OnProductCollectiblePaymentExists != null)
			{
				this.OnProductCollectiblePaymentExists(HasCollectiblePayment);
			}
		});
		Connections.Frontend.On(delegate(ProductSold msg, PacketHeader header)
		{
			if (this.OnProductSold != null)
			{
				this.OnProductSold(msg);
			}
		});
		Connections.Frontend.On(delegate(MarketPaymentReceived msg, PacketHeader header)
		{
			if (this.OnProductPaymentReceived != null)
			{
				this.OnProductPaymentReceived(msg);
			}
		});
		Connections.Frontend.On(delegate(ProductStateUpdated msg, PacketHeader header)
		{
			if (this.OnProductStateUpdated != null)
			{
				this.OnProductStateUpdated(msg);
			}
		});
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += InitMarketCategoires;
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
	}

	private void OnReady()
	{
		GetExpiredProduct();
		GetFavoriteProduct(null);
	}

	public void GetExpiredProduct()
	{
		GetExpiredProducts msg = default(GetExpiredProducts);
		msg.Skip = 0;
		msg.Sort = null;
		GetProducts(Send(msg), delegate
		{
		});
	}

	private void InitMarketCategoires()
	{
		if (GameManager.IsPrologueMode)
		{
			return;
		}
		Dictionary<string, List<Prototype>> instance = SingletonDict<string, List<Prototype>>.Instance;
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		foreach (KeyValuePair<string, List<Prototype>> item in instance)
		{
			List<Prototype> value = item.Value;
			if (value == null)
			{
				continue;
			}
			foreach (Prototype item2 in value)
			{
				if (item2 == null || string.IsNullOrEmpty(item2.Category))
				{
					continue;
				}
				HashSet<string> hashSet = dictionary.Get(item2.Category);
				if (hashSet == null)
				{
					hashSet = new HashSet<string>();
					dictionary[item2.Category] = hashSet;
				}
				if (item2.SubCategories == null)
				{
					continue;
				}
				string[] subCategories = item2.SubCategories;
				foreach (string text in subCategories)
				{
					if (!string.IsNullOrEmpty(text))
					{
						hashSet.Add(text);
					}
				}
			}
		}
		List<Category> list = new List<Category>();
		foreach (KeyValuePair<string, HashSet<string>> item3 in dictionary)
		{
			Category category = new Category();
			category.MainCategory = new Category.Main(item3.Key);
			int count = item3.Value.Count;
			if (count > 0)
			{
				category.Subs = new Category.Sub[count];
				int num = 0;
				foreach (string item4 in item3.Value)
				{
					category.Subs[num++] = new Category.Sub(item4);
				}
			}
			list.Add(category);
		}
		CategoryYamlData = list.ToArray();
	}

	public static ReplyMessageHandlerRegistrar Send<T>(T msg)
	{
		if (OptionSystem.IsMarketEnabled())
		{
			return Connections.Frontend.Send(msg);
		}
		return default(ReplyMessageHandlerRegistrar);
	}

	public void RegisterCommodity([NotNull] IList<ItemData> items, long price, float duration)
	{
		RegisterMultipleProducts msg = default(RegisterMultipleProducts);
		msg.ItemIds = Util.ItemsToIds(items);
		msg.EachPrice = price;
		msg.Duration = duration;
		Send(msg);
	}

	public void BuyCommodity(string id, Action<bool> onResult)
	{
		BuyProduct msg = default(BuyProduct);
		msg.ProductId = id;
		Send(msg).All(delegate(Packet packet)
		{
			if (Packet.IsSuccess(packet) && this.SuccessItemBuy != null)
			{
				this.SuccessItemBuy();
			}
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public void UnregisterCommodity(string id, Action<bool> onResult)
	{
		UnregisterProduct msg = default(UnregisterProduct);
		msg.ProductId = id;
		Send(msg).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public void WithdrawCommodity(string id, Action<bool> onResult)
	{
		WithdrawProduct msg = default(WithdrawProduct);
		msg.ProductId = id;
		Send(msg).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public void GetProducts(ReplyMessageHandlerRegistrar wrappedMessage, [NotNull] Action<Products?> onResult)
	{
		SetProductsHandler(wrappedMessage, onResult);
	}

	private void SetProductsHandler(ReplyMessageHandlerRegistrar searchProductsRegistrar, [NotNull] Action<Products?> onResult)
	{
		if (OptionSystem.IsMarketEnabled())
		{
			int capturedSequence = IssueNewSequence();
			searchProductsRegistrar.On(delegate(Products products, PacketHeader header)
			{
				if (capturedSequence == _sequenceCount)
				{
					onResult(products);
				}
			}).Rest(delegate
			{
				if (capturedSequence == _sequenceCount)
				{
					onResult(null);
				}
			});
		}
		else
		{
			onResult(default(Products));
		}
	}

	private int IssueNewSequence()
	{
		_sequenceCount++;
		return _sequenceCount;
	}

	public void ToggleFavorite(string commodityId, [NotNull] Action<bool, bool, string> favoriteAdded)
	{
		if (_favoritesIds == null)
		{
			return;
		}
		bool isAddToFavorite = !IsFavorite(commodityId);
		ReplyMessageHandlerRegistrar searchProductsRegistrar;
		if (isAddToFavorite)
		{
			AddToFavoriteProducts msg = default(AddToFavoriteProducts);
			msg.ProductId = commodityId;
			searchProductsRegistrar = Send(msg);
		}
		else
		{
			RemoveFromFavoriteProducts msg2 = default(RemoveFromFavoriteProducts);
			msg2.ProductId = commodityId;
			searchProductsRegistrar = Send(msg2);
		}
		SetProductsHandler(searchProductsRegistrar, delegate(Products? products)
		{
			if (products.HasValue && products.Value._Products != null)
			{
				IEnumerable<string> items = products.Value._Products.Select((Product elem) => elem.Id);
				_favoritesIds.Clear();
				_favoritesIds.AddRange(items);
				favoriteAdded(arg1: true, isAddToFavorite, commodityId);
			}
			else
			{
				favoriteAdded(arg1: false, IsFavorite(commodityId), commodityId);
			}
		});
	}

	public bool IsFavorite(string commodityId)
	{
		if (_favoritesIds == null)
		{
			return false;
		}
		return _favoritesIds.Contains(commodityId);
	}

	public void GetFavoriteProduct(Action onResult)
	{
		if (IsFavoriteSystemOnline)
		{
			if (onResult != null)
			{
				onResult();
			}
			return;
		}
		GetProducts(Send(default(GetFavoriteProducts)), delegate(Products? products)
		{
			_favoritesIds = ((!products.HasValue || products.Value._Products == null) ? null : new HashSet<string>(products.Value._Products.Select((Product elem) => elem.Id)));
			if (onResult != null)
			{
				onResult();
			}
		});
	}
}
