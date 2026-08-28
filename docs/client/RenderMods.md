# Client render, model และ AssetBundle mods

ระบบนี้ให้ม็อดฝั่ง client เปลี่ยนภาพที่ผู้เล่นเห็นได้โดยไม่แทนที่ `PlayerBehavior`, collider,
ตำแหน่ง หรือ state จากเซิร์ฟเวอร์ จึงยังใช้ movement/network/authority ของเกมเดิมครบ.

API อยู่ใน optional interface `IClientAssetOverrideApi`:

```csharp
public void OnLoad(IClientModApi api)
{
    IClientAssetOverrideApi assets = api as IClientAssetOverrideApi;
    if (assets == null) return;

    api.OnGameReady(delegate
    {
        if (!assets.LoadAssetBundle("appearance", "assets/my-mod.bundle", "EXPECTED_SHA256"))
            return;

        assets.ReplaceLocalPlayerModel(
            "main-model", "appearance", "assets/models/player.prefab", true);
    });
}
```

## Package layout

ใช้โฟลเดอร์แยกต่อม็อดเพื่อให้ relative path มี root ของตัวเอง:

```text
mods/
  my-render-mod/
    MyRenderMod.dll
    assets/
      my-mod.bundle
```

Loader ยังรองรับ layout เก่า `mods/MyMod.dll` แต่กรณีนั้น root ของ asset คือ `mods/` ร่วมกัน.
แนะนำให้ build bundle สำหรับ Windows ด้วย Unity 2017.4.34f1 ซึ่งตรงกับ runtime ของเกม.
ชื่อ asset ที่ส่งเข้า API คือชื่อภายใน bundle ไม่ใช่ path ของไฟล์ bundle.

`LoadAssetBundle` ปฏิเสธ absolute path และ `..` ที่หนีจาก package. ถ้าส่ง SHA-256 ที่ไม่ว่าง
ระบบจะตรวจ hash ก่อนเรียก Unity; hash ผิดจะไม่โหลด bundle. Bundle ID, instance ID และ override ID
ถูก namespace ด้วย mod ID จึงไม่ชนกันข้ามม็อด.

## API ที่ใช้ได้

- `ReplaceLocalPlayerModel` — instantiate prefab ใต้ local player, ซ่อน mesh เดิม และเลือก remap
  `SkinnedMeshRenderer.bones/rootBone` ตามชื่อกระดูกของ skeleton เกม เพื่อให้โมเดลใหม่ตาม animation เดิม.
- `ReplaceLocalPlayerMaterial` — เปลี่ยน material slot ของ renderer ที่เลือก.
- `ReplaceLocalPlayerTexture` — clone material เฉพาะ local player แล้วเปลี่ยน texture property เช่น `_MainTex`;
  ไม่แก้ shared material ของผู้เล่น/NPC ตัวอื่น.
- `SpawnPrefab` — spawn prefab ใน world coordinates หรือ attach กับ local player. ใช้กับ prop,
  particle, trail, light และ prefab ที่มี component อื่นได้.
- `PlayAudioClip` — เล่น `AudioClip` แบบ world/local-player, กำหนด volume และ loop ได้.
- `RestoreLocalPlayerAppearance`, `RestoreAllLocalPlayerAppearance`, `DestroySpawnedAsset` และ
  `UnloadAssetBundle` — คืนสภาพและปล่อยทรัพยากรโดยตรง.

`rendererSelector` ใช้ `*` เพื่อเลือก `MeshRenderer`/`SkinnedMeshRenderer` ทั้งหมด, ใช้ชื่อ GameObject
เช่น `Body` หรือ hierarchy path เช่น `Reference/Body`. `materialIndex` เริ่มจาก 0.

## Lifecycle และ fallback

- เรียก Unity/render API จาก main thread เท่านั้น. `OnLoad`, `OnGameReady`, hotkey, scene hook และ
  `OnUpdate` ของ loader ทำงานบน main thread.
- ถ้าเรียก appearance API ก่อน local player พร้อม ระบบจะเก็บ definition แล้ว apply ตอน player เกิด.
- เมื่อ local player เปลี่ยนจาก reconnect/scene ระบบ reapply appearance ให้อัตโนมัติ.
- `PlayerBehavior.Costume_ModelChanged` แจ้ง loader ให้ reapply หลังเกมเปลี่ยนชุดหรือ renderer.
- ถ้า bundle/asset/type/renderer ไม่ตรง API คืน `false` และเขียนเหตุผลลง `clientmods.log` โดยไม่ล้มเกม.
- เมื่อม็อด fail ระหว่าง load, ถูก disable หรือเกมปิด ระบบ restore renderer, destroy instance/material clone,
  unpatch method และ unload bundle ของม็อดนั้น.
- full-model override เป็น exclusive หนึ่งตัวในเวลาเดียวเพื่อไม่ให้หลายม็อดซ่อนโมเดลทับกัน.

Prefab ที่ต้อง remap skeleton ควรใช้ชื่อกระดูกตรงกับตัวเกม (`Bip001` หรือ `Dummy_root` และลูกของมัน).
ถ้าหาชื่อกระดูกตรงกันไม่ได้ prefab ยัง attach กับ player ได้ แต่ skinned mesh จะไม่ตาม animation เดิมครบ.

ดูตัวอย่าง buildable ที่ `tools/ExampleRenderMod/`.
