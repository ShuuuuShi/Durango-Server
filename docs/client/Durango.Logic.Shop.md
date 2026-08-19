# namespace `Durango.Logic.Shop`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

2 ไฟล์

## `Durango.Logic.Shop/Commodity.cs`

557 บรรทัด

**class `Commodity`** — บรรทัด 18–556

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public string Id { get; private set; }` | public |
| 45 | `public Yaml.Commodity Data { get; private set; }` | public |
| 71 | `public ItemColor IconColor { get; private set; }` | public |
| 75 | `public string Currency { get; set; }` | public |
| 77 | `public Money Money { get; private set; }` | public |
| 79 | `public Shared.Purchaser.Tags SalesTag { get; private set; }` | public |
| 83 | `public CommodityCondition AcceptCondition { get; private set; }` | public |
| 85 | `public string IapProductId { get; private set; }` | public |
| 87 | `public List<ContentDescription> ContentDescriptions { get; private set; }` | public |
| 133 | `public bool IsProduct => !string.IsNullOrEmpty(IapProductId);` | public |
| 155 | `public Commodity(string id, [NotNull] Yaml.Commodity info)` | public |
| 202 | `public string GetTitle(string purchaseId = null)` | public |
| 212 | `public string GetDescription(string purchaseId = null)` | public |
| 226 | `private bool HasFirstPurchaseBonus()` |  |
| 231 | `public bool IsFirstPurchaseBonus(string purchaseId = null)` | public |
| 241 | `public int CompareTo(Commodity other)` | public |
| 257 | `public string GetCurrencyText(bool hasDiscountRatio)` | public |
| 280 | `public float GetDiscountRate()` | public |
| 289 | `public bool VoucherPurchasable()` | public |
| 294 | `public bool DlcPurchasable()` | public |
| 303 | `public bool DlcVisible()` | public |
| 312 | `public uint GetDlcId()` | public |
| 317 | `public ItemIcon GetIcon(bool large)` | public |
| 350 | `private string MakePurchasedCaption()` |  |
| 361 | `public string GetRemainingTime()` | public |
| 387 | `public bool IsPurchasable()` | public |
| 405 | `public bool IsVisible()` | public |
| 431 | `public bool IsQuestPurchase(CommodityCondition.Type? type = null)` | public |
| 444 | `public Purchase GetQuestPurchase(CommodityCondition.Type? type = null)` | public |
| 460 | `private ContentDescription GetContentDescription(string key)` |  |
| 480 | `private void AppendContentDescription(ShopContents contents)` |  |
| 507 | `private void AppendContents([CanBeNull] IEnumerable<CommodityContent> contents)` |  |
| 530 | `public bool TryGetPreviewContent(out ContentDescription conetnt)` | public |

---

## `Durango.Logic.Shop/Purchase.cs`

132 บรรทัด

**class `Purchase`** — บรรทัด 11–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public string Id { get; private set; }` | public |
| 15 | `public string CommodityId { get; private set; }` | public |
| 17 | `public double PurchasedAt { get; private set; }` | public |
| 19 | `public double? AcceptedAt { get; private set; }` | public |
| 21 | `public double ExpiresAt { get; private set; }` | public |
| 23 | `public ItemData Item { get; private set; }` | public |
| 25 | `public string Emotion { get; private set; }` | public |
| 27 | `public bool HasSubCommodities { get; private set; }` | public |
| 40 | `public void Set(Messages.Purchase msg)` | public |
| 70 | `public string GetName()` | public |
| 85 | `public ItemIcon GetIcon()` | public |
| 98 | `public double? GetSubAcceptedAt(string key)` | public |
| 111 | `public bool GetPayBackMileage(out int paybackMileage)` | public |
| 123 | `public string GetAcceptPurchaseDescription()` | public |

---
