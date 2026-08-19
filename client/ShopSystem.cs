using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Shop;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ShopSystem : GameSystem<ShopSystem>
{
	private struct NewAcceptablePurchase
	{
		public string PurchaseId;

		public string CommodityId;

		public string SubId;
	}

	private const string ReadCommoditiesKey = "read_commodities";

	private readonly Dictionary<string, Durango.Logic.Shop.Commodity> _commodityDict = new Dictionary<string, Durango.Logic.Shop.Commodity>();

	private AsyncCachedData<List<Durango.Logic.Shop.Commodity>> _purchasableCommodities;

	private bool _isAvailable;

	private readonly List<Durango.Logic.Shop.Purchase> _purchases = new List<Durango.Logic.Shop.Purchase>();

	private readonly Dictionary<string, AcceptableSubPurchase> _acceptableSubPurchases = new Dictionary<string, AcceptableSubPurchase>();

	private readonly HashSet<string> _readCommodities = new HashSet<string>();

	private readonly Dictionary<string, string> _userFirstPurchaseHistory = new Dictionary<string, string>();

	private ICoroutineBinder _specialDealsCheckBinder;

	[NotNull]
	public List<Durango.Logic.Shop.Purchase> Purchases => _purchases;

	public List<Durango.Logic.Shop.Commodity> PurchasableList => (_purchasableCommodities != null) ? _purchasableCommodities.GetCachedValue() : null;

	public bool HasSpecialDeals => SpecialDeals != null && SpecialDeals.Length > 0;

	public double SpecialDealsMinExpiresAt { get; private set; }

	[CanBeNull]
	public SpecialDeal[] SpecialDeals { get; private set; }

	[CanBeNull]
	public string FreshSpecialDealId { get; set; }

	public event Action PurchasesUpdated;

	public event Action AcceptableSubPurchasesUpdated;

	public event Action<string, string, string> AcceptableSubPurchaseItem;

	public event Action UserFirstPurchaseHistoryUpdated;

	public event Action ReadCommoditiesChanged;

	public event Action SpecialDealsUpdated;

	public event Action NewSpecialDealCommoditiesReleased;

	private void Awake()
	{
		Connections.Frontend.On<Purchases>(OnPurchases);
		Connections.Frontend.On<AcceptableSubPurchases>(OnAcceptableSubPurchases);
		Connections.Frontend.On<SpecialDeals>(OnSpecialDeals);
	}

	private void Start()
	{
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += CheckAvailable;
		GameSystem<OptionSystem>.Instance().AddOnChange("cashshop.ui_enabled", (Action<bool>)delegate
		{
			CheckAvailable();
		});
	}

	private void LoadReadCommodities()
	{
		string @string = Preferences.GetString("read_commodities", string.Empty, Preferences.Level.User);
		string[] array = Json.Read<string[]>(@string);
		_readCommodities.Clear();
		if (array != null)
		{
			_readCommodities.AddRange(array);
		}
		else
		{
			List<Durango.Logic.Shop.Commodity> list = ((_purchasableCommodities != null) ? _purchasableCommodities.GetCachedValue() : null);
			if (list != null)
			{
				_readCommodities.AddRange(list.Select((Durango.Logic.Shop.Commodity x) => x.Id));
			}
		}
		if (this.ReadCommoditiesChanged != null)
		{
			this.ReadCommoditiesChanged();
		}
	}

	private void SaveReadCommodities()
	{
		List<Durango.Logic.Shop.Commodity> list = ((_purchasableCommodities != null) ? _purchasableCommodities.GetCachedValue() : null);
		if (list != null && list.Count > 0)
		{
			_readCommodities.IntersectWith(list.Select((Durango.Logic.Shop.Commodity x) => x.Id));
		}
		string value = Json.Write(_readCommodities.ToArray());
		Preferences.SetString("read_commodities", value, Preferences.Level.User);
	}

	[ExposedInEditor(null)]
	private void ResetReadCommodities()
	{
		_readCommodities.Clear();
		SaveReadCommodities();
		if (this.ReadCommoditiesChanged != null)
		{
			this.ReadCommoditiesChanged();
		}
	}

	public void AddReadCommodities([CanBeNull] IList<Durango.Logic.Shop.Commodity> commodities)
	{
		if (commodities == null || commodities.Count == 0)
		{
			return;
		}
		int count = _readCommodities.Count;
		_readCommodities.UnionWith(commodities.Select((Durango.Logic.Shop.Commodity x) => x.Id));
		if (count != _readCommodities.Count)
		{
			SaveReadCommodities();
			if (this.ReadCommoditiesChanged != null)
			{
				this.ReadCommoditiesChanged();
			}
		}
	}

	public bool IsReadCommodity(string id)
	{
		return _readCommodities.Contains(id);
	}

	public string GetFirstPurchasedId(string id)
	{
		return _userFirstPurchaseHistory.Get(id);
	}

	private void CheckAvailable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.Shop, OptionSystem.IsShopEnabled());
		bool flag = GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop);
		if (_isAvailable != flag && flag)
		{
			InitCommodityList();
			Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
		}
		_isAvailable = flag;
	}

	private void OnReady()
	{
		GetPurchases();
		GetAcceptableSubPurchases();
		GetPurchasableCommodities(delegate
		{
			LoadReadCommodities();
		});
		GetUserFirstPurchaseHistory();
		GetSpecialDeals();
	}

	public void GetPurchases()
	{
		Connections.Frontend.Send(default(GetPurchases));
	}

	public void GetSpecialDeals()
	{
		Connections.Frontend.Send(default(GetSpecialDeals)).On(delegate(SpecialDeals msg, PacketHeader header)
		{
			SetSpecialDeals(msg);
		});
	}

	private void GetAcceptableSubPurchases()
	{
		Connections.Frontend.Send(default(GetAcceptableSubPurchases));
	}

	private void GetUserFirstPurchaseHistory()
	{
		Connections.Frontend.Send(default(GetUserFirstPurchaseHistory)).On(delegate(UserFirstPurchaseHistory msg, PacketHeader header)
		{
			_userFirstPurchaseHistory.Clear();
			UserFirstPurchase[] userFirstPurchaseHistory = msg._UserFirstPurchaseHistory;
			for (int i = 0; i < userFirstPurchaseHistory.Length; i++)
			{
				UserFirstPurchase userFirstPurchase = userFirstPurchaseHistory[i];
				_userFirstPurchaseHistory.Add(userFirstPurchase.CommodityId, userFirstPurchase.PurchaseId);
			}
			if (this.UserFirstPurchaseHistoryUpdated != null)
			{
				this.UserFirstPurchaseHistoryUpdated();
			}
		});
	}

	private void InitCommodityList()
	{
		Yaml.Commodities instance = Yaml.Util.Singleton<Yaml.Commodities>.Instance;
		_commodityDict.Clear();
		if (instance.PostedCommodities != null)
		{
			foreach (KeyValuePair<string, Yaml.Commodity> postedCommodity in instance.PostedCommodities)
			{
				_commodityDict.Add(postedCommodity.Key, new Durango.Logic.Shop.Commodity(postedCommodity.Key, postedCommodity.Value));
			}
		}
		if (!OptionSystem.IsTestCommoditiesOpened() || instance.TestCommodities == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Yaml.Commodity> testCommodity in instance.TestCommodities)
		{
			_commodityDict.Add(testCommodity.Key, new Durango.Logic.Shop.Commodity(testCommodity.Key, testCommodity.Value));
		}
	}

	public bool HasAcceptableSubPurchase(CommodityCondition.Type? type)
	{
		foreach (KeyValuePair<string, AcceptableSubPurchase> acceptableSubPurchase in _acceptableSubPurchases)
		{
			if (KUtility.GetSize(acceptableSubPurchase.Value.AcceptableSubIds) > 0)
			{
				if (!type.HasValue)
				{
					return true;
				}
				Durango.Logic.Shop.Commodity commodity = GetCommodity(acceptableSubPurchase.Key);
				if (commodity != null && commodity.IsQuestPurchase(type))
				{
					return true;
				}
			}
		}
		return false;
	}

	[CanBeNull]
	public AcceptableSubPurchase? GetAcceptableSubPurchase(string purchaseId)
	{
		Dictionary<string, AcceptableSubPurchase> acceptableSubPurchases = _acceptableSubPurchases;
		if (acceptableSubPurchases.TryGetValue(purchaseId, out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerable<KeyValuePair<string, AcceptableSubPurchase>> GetAcceptableSubPurchase()
	{
		return _acceptableSubPurchases;
	}

	private void OnAcceptableSubPurchases(AcceptableSubPurchases msg, PacketHeader header)
	{
		if (msg.Ids == null)
		{
			return;
		}
		List<NewAcceptablePurchase> list = new List<NewAcceptablePurchase>();
		AcceptableSubPurchase[] ids = msg.Ids;
		for (int i = 0; i < ids.Length; i++)
		{
			AcceptableSubPurchase value = ids[i];
			Durango.Logic.Shop.Commodity commodity = GetCommodity(value.CommodityId);
			if (commodity == null)
			{
				continue;
			}
			string text = null;
			for (int j = 0; j < commodity.SubCommodities.Count; j++)
			{
				Durango.Logic.Shop.Commodity commodity2 = commodity.SubCommodities[j];
				if (value.AcceptableSubIds != null && value.AcceptableSubIds.IndexOf(commodity2.Id) != -1)
				{
					text = commodity2.Id;
					break;
				}
			}
			if (!_acceptableSubPurchases.TryGetValue(value.PurchaseId, out var value2) || value2.AcceptableSubIds == null || value2.AcceptableSubIds.IndexOf(text) == -1)
			{
				list.Add(new NewAcceptablePurchase
				{
					PurchaseId = value.PurchaseId,
					CommodityId = value.CommodityId,
					SubId = text
				});
			}
			_acceptableSubPurchases[value.PurchaseId] = value;
		}
		if (this.AcceptableSubPurchasesUpdated != null)
		{
			this.AcceptableSubPurchasesUpdated();
		}
		if (this.AcceptableSubPurchaseItem != null)
		{
			for (int k = 0; k < list.Count; k++)
			{
				NewAcceptablePurchase newAcceptablePurchase = list[k];
				this.AcceptableSubPurchaseItem(newAcceptablePurchase.PurchaseId, newAcceptablePurchase.CommodityId, newAcceptablePurchase.SubId);
			}
		}
	}

	public void AcceptSubPurchase(string purchaseId, string subId, Action<bool> onResult = null)
	{
		int num = PurchaseIndexOf(purchaseId);
		if (num != -1)
		{
			Durango.Logic.Shop.Purchase purchase = _purchases[num];
			if (purchase.SubAcceptedAt == null)
			{
				purchase.SubAcceptedAt = new Dictionary<string, double>();
			}
			if (!purchase.SubAcceptedAt.ContainsKey(subId))
			{
				purchase.SubAcceptedAt.Add(subId, Connections.Frontend.GetPredictedServerTime());
			}
			Durango.Logic.Shop.Commodity commodity = GetCommodity(purchase.CommodityId);
			if (commodity != null && purchase.SubAcceptedAt.Count >= commodity.SubCommodities.Count)
			{
				_purchases.RemoveAt(num);
			}
		}
		if (_acceptableSubPurchases.TryGetValue(purchaseId, out var value))
		{
			if (value.AcceptableSubIds == null)
			{
				_acceptableSubPurchases.Remove(purchaseId);
			}
			else
			{
				int num2 = value.AcceptableSubIds.IndexOf(subId);
				if (num2 != -1)
				{
					if (value.AcceptableSubIds.Length == 1)
					{
						_acceptableSubPurchases.Remove(purchaseId);
					}
					else
					{
						value.AcceptableSubIds[num2] = value.AcceptableSubIds[value.AcceptableSubIds.Length - 1];
						Array.Resize(ref value.AcceptableSubIds, value.AcceptableSubIds.Length - 1);
						_acceptableSubPurchases[purchaseId] = value;
					}
				}
			}
		}
		if (this.AcceptableSubPurchasesUpdated != null)
		{
			this.AcceptableSubPurchasesUpdated();
		}
		Connections.Frontend.Send(new AcceptPurchase
		{
			PurchaseId = purchaseId,
			SubId = subId
		}).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (!flag)
			{
				GetPurchases();
				GetAcceptableSubPurchases();
			}
			if (onResult != null)
			{
				onResult(flag);
			}
		});
	}

	[CanBeNull]
	public Durango.Logic.Shop.Purchase GetPurchase(string id)
	{
		int num = PurchaseIndexOf(id);
		return (num != -1) ? _purchases[num] : null;
	}

	private int PurchaseIndexOf(string id)
	{
		for (int i = 0; i < _purchases.Count; i++)
		{
			if (_purchases[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	[CanBeNull]
	public Durango.Logic.Shop.Commodity GetCommodity([CanBeNull] string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		Durango.Logic.Shop.Commodity commodity = _commodityDict.Get(id);
		if (commodity == null)
		{
			return null;
		}
		return commodity;
	}

	public Durango.Logic.Shop.Commodity FindCommodityByProductId(string productId)
	{
		foreach (KeyValuePair<string, Durango.Logic.Shop.Commodity> item in _commodityDict)
		{
			if (item.Value.IapProductId == productId)
			{
				return item.Value;
			}
		}
		return null;
	}

	public void GetPurchasableCommodities(Action<List<Durango.Logic.Shop.Commodity>> callback, bool immediately = false)
	{
		if (_purchasableCommodities == null)
		{
			_purchasableCommodities = new AsyncCachedData<List<Durango.Logic.Shop.Commodity>>(delegate(List<Durango.Logic.Shop.Commodity> list, Action<List<Durango.Logic.Shop.Commodity>> response)
			{
				Connections.Frontend.Send(default(GetCommodities)).On(delegate(Messages.Commodities msg, PacketHeader header)
				{
					response(SetPurchasableList(list, msg.CommodityInfos));
				}).Rest(delegate
				{
					response(SetPurchasableList(list, null));
				});
			}, 10f);
		}
		_purchasableCommodities.Request(callback, immediately);
	}

	private List<Durango.Logic.Shop.Commodity> SetPurchasableList(List<Durango.Logic.Shop.Commodity> list, CommodityInfo[] infos)
	{
		if (list == null)
		{
			list = new List<Durango.Logic.Shop.Commodity>();
		}
		list.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(infos); i < size; i++)
		{
			Durango.Logic.Shop.Commodity commodity = GetCommodity(infos[i].Id);
			if (commodity != null)
			{
				commodity.CommodityInfo = infos[i];
				list.Add(commodity);
			}
		}
		return list;
	}

	private void OnPurchases(Purchases msg, PacketHeader header)
	{
		Messages.Purchase[] purchases = msg._Purchases;
		int num = 0;
		if (purchases != null)
		{
			Messages.Purchase[] array = purchases;
			for (int i = 0; i < array.Length; i++)
			{
				Messages.Purchase msg2 = array[i];
				int num2 = -1;
				for (int j = num; j < _purchases.Count; j++)
				{
					if (msg2.Id == _purchases[j].Id)
					{
						num2 = j;
						break;
					}
				}
				if (num2 == -1)
				{
					num2 = _purchases.Count;
					Durango.Logic.Shop.Purchase purchase = new Durango.Logic.Shop.Purchase();
					purchase.Set(msg2);
					_purchases.Add(purchase);
				}
				else
				{
					_purchases[num2].Set(msg2);
				}
				Durango.Logic.Shop.Purchase value = _purchases[num2];
				_purchases[num2] = _purchases[num];
				_purchases[num] = value;
				num++;
			}
		}
		if (_purchases.Count > num)
		{
			_purchases.RemoveRange(num, _purchases.Count - num);
		}
		_purchases.Sort((Durango.Logic.Shop.Purchase p1, Durango.Logic.Shop.Purchase p2) => Math.Sign(p2.PurchasedAt - p1.PurchasedAt));
		if (this.PurchasesUpdated != null)
		{
			this.PurchasesUpdated();
		}
	}

	public void PurchaseCommodity(Durango.Logic.Shop.Commodity commodity, Action<Purchased, bool> onSuccess, Action onFail)
	{
		bool withVoucher = InventorySystem.Wallet.PurchasableVoucherCount(commodity) > 0;
		((!commodity.VoucherPurchasable() || !withVoucher) ? Connections.Frontend.Send(new PurchaseCommodity
		{
			CommodityId = commodity.Id
		}) : Connections.Frontend.Send(new PurchaseCommodityWithVoucher
		{
			CommodityId = commodity.Id
		})).On(delegate(Purchased msg, PacketHeader header)
		{
			OnPurchased(commodity);
			if (onSuccess != null)
			{
				onSuccess(msg, withVoucher);
			}
		}).Rest(delegate
		{
			if (onFail != null)
			{
				onFail();
			}
		});
	}

	private void OnPurchased(Durango.Logic.Shop.Commodity commodity)
	{
		GetPurchases();
		if (commodity.IsQuestPurchase())
		{
			GetAcceptableSubPurchases();
		}
		if (!_userFirstPurchaseHistory.ContainsKey(commodity.Id))
		{
			_userFirstPurchaseHistory[commodity.Id] = "Unknown";
			GetUserFirstPurchaseHistory();
		}
	}

	public void AcceptPurchase(string purchaseId, Action<bool> callback)
	{
		int num = PurchaseIndexOf(purchaseId);
		if (num != -1)
		{
			_purchases.RemoveAt(num);
		}
		if (this.PurchasesUpdated != null)
		{
			this.PurchasesUpdated();
		}
		Connections.Frontend.Send(new AcceptPurchase
		{
			PurchaseId = purchaseId
		}).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (!flag)
			{
				GetPurchases();
			}
			if (callback != null)
			{
				callback(flag);
			}
		});
	}

	private void OnSpecialDeals(SpecialDeals msg, PacketHeader header)
	{
		SetSpecialDeals(msg);
		if (this.NewSpecialDealCommoditiesReleased != null)
		{
			this.NewSpecialDealCommoditiesReleased();
		}
	}

	private void SetSpecialDeals(SpecialDeals msg)
	{
		FreshSpecialDealId = GetFreshSpecialDealId(SpecialDeals, msg.Deals);
		SpecialDeals = msg.Deals;
		SpecialDealsMinExpiresAt = 0.0;
		if (SpecialDeals != null && SpecialDeals.Length > 0)
		{
			Array.Sort(SpecialDeals, (SpecialDeal x, SpecialDeal y) => x.ExpiresAt.CompareTo(y.ExpiresAt));
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			SpecialDealsMinExpiresAt = SpecialDeals.Min((SpecialDeal deal) => deal.ExpiresAt);
			double num = SpecialDealsMinExpiresAt - predictedServerTime;
			if (num > 0.0)
			{
				GameSystem<ShopSystem>.Instance().StartCoroutine(ref _specialDealsCheckBinder, CoCheckSpecialDeals((float)num));
			}
			else
			{
				GetSpecialDeals();
			}
		}
		if (this.SpecialDealsUpdated != null)
		{
			this.SpecialDealsUpdated();
		}
	}

	private IEnumerator CoCheckSpecialDeals(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		GetSpecialDeals();
	}

	private static string GetFreshSpecialDealId(SpecialDeal[] currentDeals, SpecialDeal[] newDeals)
	{
		if (KUtility.GetSize(currentDeals) > 0 && KUtility.GetSize(newDeals) > 0)
		{
			HashSet<string> existIds = new HashSet<string>(currentDeals.Select((SpecialDeal d) => d.CommodityId));
			return newDeals.FirstOrDefault((SpecialDeal d) => !existIds.Contains(d.CommodityId)).CommodityId;
		}
		return null;
	}

	public static void SendDurangoCoin(string targetPlayerId, int amount, Action onSuccess)
	{
		TransferDurangoCoin transferDurangoCoin = default(TransferDurangoCoin);
		transferDurangoCoin.Amount = amount;
		transferDurangoCoin.RecipientEntityId = targetPlayerId;
		TransferDurangoCoin msg = transferDurangoCoin;
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			if (Packet.IsSuccess(packet) && onSuccess != null)
			{
				onSuccess();
			}
		});
	}
}
