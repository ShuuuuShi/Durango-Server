# namespace `Durango.Logic.Clusters`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `Durango.Logic.Clusters/Account.cs`

59 บรรทัด

**class `Account`** — บรรทัด 7–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public Pair<string, int> GetRecommendedPlayer()` | public |
| 39 | `public Pair<string, int> ApplyRecommendedPlayer(Pair<string, int> idToSlot)` | public |
| 45 | `public string GetPlayerInfoText(string id)` | public |

---

## `Durango.Logic.Clusters/Cluster.cs`

66 บรรทัด

**class `Cluster`** — บรรทัด 8–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static Cluster Null = new Cluster();` | public |
| 31 | `public Action<Action<Account>> OnRequestAccount { get; set; }` | public |
| 33 | `public Action<string> OnDeletePlayer { get; set; }` | public |
| 35 | `public Action<string> OnConfirm { get; set; }` | public |
| 37 | `public bool IsRecommendable { get; set; }` | public |
| 39 | `public Mode Mode { get; set; }` | public |
| 41 | `public string LocalPlayer { get; set; }` | public |
| 43 | `public string GetName([CanBeNull] string locale)` | public |
| 56 | `public bool IsInMaintenance()` | public |
| 61 | `public string GetMaintenanceText([CanBeNull] string locale, bool em = true)` | public |

---

## `Durango.Logic.Clusters/Clusters.cs`

306 บรรทัด

**class `Clusters`** — บรรทัด 14–305

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly Dictionary<string, Cluster> _clusters = new Dictionary<string, Cluster>();` |  |
| 41 | `private readonly Dictionary<string, Urls> _urls = new Dictionary<string, Urls>();` |  |
| 43 | `private readonly Dictionary<string, string[]> _urlLinks = new Dictionary<string, string[]>();` |  |
| 47 | `private readonly Dictionary<string, Account> _clusterToAccount = new Dictionary<string, Account>();` |  |
| 49 | `public string ArenaAuthUrl { get; private set; }` | public |
| 51 | `public string EngagementUrl { get; private set; }` | public |
| 57 | `public void LoadFromJson(string jsonString)` | public |
| 159 | `public void ForceSetCluster(string gateway)` | public |
| 168 | `public void GetOrRequestAccounts(string targetCluster, Action<Account> callback, bool forceUpdate = false)` | public |
| 207 | `public static void RequestAccounts(string gatewayUrl, Action<Account> callback)` | public |
| 228 | `public int GetPlayerCount(string clusterKey)` | public |
| 235 | `public Cluster GetCluster(string clusterKey)` | public |
| 242 | `public Account GetAccount(string clusterKey)` | public |
| 247 | `public string GetRecommendableCluster()` | public |
| 259 | `public string[] GetClusterKeys()` | public |
| 264 | `public IList<Urls> GetOutlinks()` | public |
| 291 | `public bool IsInMaintenance()` | public |
| 296 | `public string GetMaintenanceText([CanBeNull] string locale)` | public |
| 301 | `public void Clear()` | public |

   **class `ClusterSet`** — บรรทัด 16–35

---

## `Durango.Logic.Clusters/Maintenance.cs`

80 บรรทัด

**class `Maintenance`** — บรรทัด 10–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public Maintenance(Dictionary<string, string> name, string utc_start, string utc_end)` | public |
| 37 | `public bool IsInMaintenance()` | public |
| 43 | `public string GetMaintenanceText([CanBeNull] string locale, bool em)` | public |
| 48 | `private string GetName([CanBeNull] string locale)` |  |
| 61 | `private string GetPeriodText()` |  |
| 71 | `private static string GetTimeString(DateTime time, bool day)` |  |

---

## `Durango.Logic.Clusters/Mode.cs`

11 บรรทัด

**enum `Mode`** — บรรทัด 3

---

## `Durango.Logic.Clusters/PlayerInfo.cs`

39 บรรทัด

**class `PlayerInfo`** — บรรทัด 7–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public string GetPlayerInfoText()` | public |
| 35 | `static PlayerInfo()` |  |

---

## `Durango.Logic.Clusters/Urls.cs`

31 บรรทัด

**class `Urls`** — บรรทัด 7–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public string GetTitle([CanBeNull] string locale)` | public |

---
