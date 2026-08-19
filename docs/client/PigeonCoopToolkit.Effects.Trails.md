# namespace `PigeonCoopToolkit.Effects.Trails`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

1 ไฟล์

## `PigeonCoopToolkit.Effects.Trails/PlaneTrail.cs`

191 บรรทัด

**class `PlaneTrail`** — บรรทัด 9–190

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public FixOption Option { get; set; }` | public |
| 55 | `public Transform TipTransform { get; set; }` | public |
| 57 | `public void SetBaked(TrailBaker.TrailData data, CharacterBehavior character, float pushBase, float timePassed)` | public |
| 66 | `protected override void LateUpdate()` | Unity lifecycle |
| 82 | `private void LateUpdateBaked()` |  |
| 105 | `private void LateUpdateNormal()` |  |
| 137 | `private void AddSegment(Vector3 posBase, Vector3 posTip)` |  |
| 165 | `protected override void OnStartEmit()` |  |
| 181 | `protected override float GetDeltaTime()` |  |

   **enum `FixOption`** — บรรทัด 11

---
