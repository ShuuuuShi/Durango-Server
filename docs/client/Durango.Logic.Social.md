# namespace `Durango.Logic.Social`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

12 ไฟล์

## `Durango.Logic.Social/ChatChannelInfo.cs`

163 บรรทัด

**class `ChatChannelInfo`** — บรรทัด 11–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private readonly Dictionary<string, ChannelInfo> _channelInfos = new Dictionary<string, ChannelInfo>();` |  |
| 28 | `public bool IsHidden(ChannelType channelType)` | public |
| 33 | `public bool IsHidden(ChatFilterType filterType)` | public |
| 38 | `public bool IsHidden(Conversation conv)` | public |
| 43 | `public bool ToggleHide(ChannelType channelType)` | public |
| 48 | `public bool ToggleHide(ChatFilterType filterType)` | public |
| 53 | `public bool ToggleHide(Conversation conv)` | public |
| 58 | `public static bool IsHideable(ChannelType channelType)` | public |
| 63 | `public double GetReadAt(string id)` | public |
| 68 | `public void SetReadAt(string id, double readAt)` | public |
| 79 | `public string GetCustomName(string id)` | public |
| 84 | `public void SetCustomName(string id, string name)` | public |
| 94 | `public void LoadStorage(Dictionary<string, byte[]> storage)` | public |
| 106 | `public void SaveStorage(Dictionary<string, Conversation> conversations)` | public |
| 138 | `private bool IsHidden(string id)` |  |
| 143 | `private bool ToggleHide(string id)` |  |
| 152 | `private ChannelInfo GetOrAdd(string id)` |  |

   **class `ChannelInfo`** — บรรทัด 13–20

---

## `Durango.Logic.Social/ChatFilterType.cs`

22 บรรทัด

**enum `ChatFilterType`** — บรรทัด 5

---

## `Durango.Logic.Social/ChatStruct.cs`

244 บรรทัด

**class `ChatStruct`** — บรรทัด 8–243

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private static readonly Color32 ColorMsgRadioDictation = new Color32(byte.MaxValue, 212, 156, byte.MaxValue);` |  |
| 24 | `private static readonly Color32 ColorMsgSystem = new Color32(119, byte.MaxValue, 85, byte.MaxValue);` |  |
| 26 | `private static readonly Color32 ColorMsgConversation = new Color32(byte.MaxValue, 122, 207, byte.MaxValue);` |  |
| 30 | `private static readonly Color32 ColorMsgNotice = new Color32(62, 186, 236, byte.MaxValue);` |  |
| 34 | `public static readonly Color32 ColorNameLocalPlayer = new Color32(122, 172, byte.MaxValue, byte.MaxValue);` | public |
| 36 | `private static readonly Color32 ColorNameNonSystem = new Color32(byte.MaxValue, 216, 91, byte.MaxValue);` |  |
| 38 | `private static readonly Color32 ColorNameSystem = new Color32(184, 184, 184, byte.MaxValue);` |  |
| 76 | `public bool Translated { get; private set; }` | public |
| 123 | `public string FindText()` | public |
| 133 | `private string FindTextInternal()` |  |
| 191 | `public Color GetMsgColor(Color defaultColor)` | public |
| 212 | `public Color GetNameColor()` | public |
| 234 | `public bool IsEventMessage()` | public |
| 239 | `public bool IsNoticeMessage()` | public |

   **enum `ChatMsgType`** — บรรทัด 10

---

## `Durango.Logic.Social/Conversation.cs`

235 บรรทัด

**class `Conversation`** — บรรทัด 15–234

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly HashSet<string> _entitySet = new HashSet<string>();` |  |
| 25 | `public bool PushEnabled { get; set; }` | public |
| 27 | `public string Id { get; private set; }` | public |
| 29 | `public List<ChatStruct> Messages { get; private set; }` | public |
| 103 | `public Conversation(string id, string entityId)` | public |
| 114 | `public Conversation(global::Messages.Conversation msg)` | public |
| 145 | `public bool GetTitle([NotNull] Action<string> onResult)` | public |
| 171 | `public void AddMessage(ChatStruct chat)` | public |
| 184 | `public void AddEntityIds(string[] ids)` | public |
| 189 | `public void RemoveEntityId(string id)` | public |
| 194 | `public string[] GetEntityIds()` | public |
| 199 | `public void FillEntityIds(HashSet<string> target)` | public |
| 204 | `public bool Contains(string entityId)` | public |
| 209 | `public double GetLastestUpdateTime()` | public |
| 214 | `private void UpdateNewCount()` |  |
| 229 | `public void MarkAsRead()` | public |

---

## `Durango.Logic.Social/Emoticon.cs`

40 บรรทัด

**class `Emoticon`** — บรรทัด 6–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public Emoticon(Yaml.Emoticon value)` | public |
| 19 | `public int CompareTo(Emoticon other)` | public |

---

## `Durango.Logic.Social/Emotion.cs`

15 บรรทัด

**enum `Emotion`** — บรรทัด 6

---

## `Durango.Logic.Social/EmotionBase.cs`

76 บรรทัด

**class `EmotionBase`** — บรรทัด 5–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public int? FavoriteIndex => (!Favorite \|\| !Available) ? null : _favoriteIndex;` | public |
| 34 | `public bool Favorite { get; private set; }` | public |
| 40 | `protected EmotionBase(string key, bool free, bool purchaseable)` |  |
| 51 | `public void ClearNotification()` | public |
| 56 | `public void SetFavorite(bool favorite, int? index = null)` | public |
| 62 | `public virtual bool IsSubscribe()` | public |
| 67 | `public void MarkAsChanged()` | public |
| 72 | `protected virtual void OnDirty()` |  |

---

## `Durango.Logic.Social/EmotionComparer.cs`

19 บรรทัด

**struct `EmotionComparer`** — บรรทัด 7–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public bool Equals(Emotion x, Emotion y)` | public |
| 14 | `public int GetHashCode(Emotion x)` | public |

---

## `Durango.Logic.Social/EmotionJson.cs`

9 บรรทัด

**struct `EmotionJson`** — บรรทัด 3–8

---

## `Durango.Logic.Social/Emotional.cs`

267 บรรทัด

**class `Emotional`** — บรรทัด 12–266

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public readonly Durango.Logic.Notification.Container EmoticonNotification = new Durango.Logic.Notification.Container();` | public |
| 18 | `public readonly Durango.Logic.Notification.Container MotionNotification = new Durango.Logic.Notification.Container();` | public |
| 20 | `private readonly List<Emoticon> _emoticons = new List<Emoticon>();` |  |
| 22 | `private readonly List<Motion> _motions = new List<Motion>();` |  |
| 36 | `public void Init(Emotions yaml)` | public |
| 63 | `public void Set(AvailableEmotions msg)` | public |
| 76 | `private void SetAvailables<T>(string[] availables, List<T> emotions, bool update) where T : EmotionBase` |  |
| 124 | `public void SetMotionFavorite(string key, bool isFavorite)` | public |
| 130 | `public void SetEmoticonFavorite(string key, bool isFavorite)` | public |
| 136 | `private void SetFavorite<T>(T emotion, List<T> emotions, bool isFavorite) where T : EmotionBase` |  |
| 168 | `private int EmoticonIndexOf(string key)` |  |
| 180 | `public Emoticon GetEmoticon(string key)` | public |
| 186 | `private int MotionIndexOf(string key)` |  |
| 198 | `public Motion GetMotion(string key)` | public |
| 204 | `public void SaveFavorites()` | public |
| 224 | `private static List<string> ToFavorites<T>(List<T> emotions) where T : EmotionBase` |  |
| 238 | `public void LoadFavorites(Dictionary<string, byte[]> storage)` | public |
| 249 | `private static void FromFavorites<T>(List<string> favorites, List<T> emotions) where T : EmotionBase` |  |

---

## `Durango.Logic.Social/Motion.cs`

119 บรรทัด

**class `Motion`** — บรรทัด 11–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public Motion(string key, Yaml.Motion value)` | public |
| 36 | `public int CompareTo(Motion other)` | public |
| 80 | `public bool IsEquipmentsMotion()` | public |
| 90 | `private bool IsEquipmentsMotion(EquipSystem.EquipPreset preset)` |  |
| 108 | `protected override void OnDirty()` |  |
| 114 | `public override bool IsSubscribe()` | public |

---

## `Durango.Logic.Social/PortraitEmotion.cs`

11 บรรทัด

**enum `PortraitEmotion`** — บรรทัด 3

---
