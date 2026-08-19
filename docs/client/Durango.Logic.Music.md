# namespace `Durango.Logic.Music`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

3 ไฟล์

## `Durango.Logic.Music/Concert.cs`

158 บรรทัด

**class `Concert`** — บรรทัด 10–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 94 | `public string Host { get; private set; }` | public |
| 97 | `public Track[] Tracks { get; private set; }` | public |
| 99 | `public Concert(PropKey bandstand)` | public |
| 109 | `public bool IsPlayable(out int playableCount)` | public |
| 127 | `public void Set(Bandstand bandstand)` | public |
| 149 | `public bool IsHost()` | public |

   **class `Track`** — บรรทัด 12–88

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public Track(int index)` | public |
   | 37 | `public bool IsPlayable(out bool notReady)` | public |
   | 50 | `public void SetPlayer(string id)` | public |
   | 55 | `public void SetTimbre(string timbre)` | public |
   | 60 | `public void SetMusicName(string musicName)` | public |
   | 65 | `public void GetPlayerInfo([NotNull] Action<Durango.Player.PlayerInfo> callback)` | public |

---

## `Durango.Logic.Music/Music.cs`

338 บรรทัด

**class `Music`** — บรรทัด 14–337

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public Music()` | public |
| 33 | `public static Music Create(Messages.Music m)` | public |
| 50 | `public static Music CreateFromMabinogiMML(string text)` | public |
| 66 | `public static Music CreateFromMs2MML(string text)` | public |
| 100 | `private static Music CreateFromMML(IEnumerable<string> tracks)` |  |
| 156 | `public static Music Create(Sequence sequence)` | public |
| 162 | `public static Music Create(Sequence sequence, ref Dictionary<int, int?> timbre)` | public |
| 259 | `public int GetLastTick()` | public |
| 273 | `public Messages.Music ToMessage()` | public |
| 283 | `public byte[] ToBytes()` | public |
| 303 | `public int TimerToTick(float timer)` | public |
| 308 | `public float TickToTimer(int tick)` | public |
| 313 | `public static int CompareMusic(KeyValuePair<MusicId, Messages.Music> m1, KeyValuePair<MusicId, Messages.Music> m2)` | public |

---

## `Durango.Logic.Music/Note.cs`

15 บรรทัด

**struct `Note`** — บรรทัด 3–14

---
