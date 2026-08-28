# Native method overrides

ทั้ง server และ client รองรับ runtime detour แบบ `Prefix`, `Postfix` และ `Replace` ผ่าน Harmony.
ม็อด cast API หลักเป็น `IModMethodOverridesApi` หรือ `IClientMethodOverridesApi`.

Method ID ใช้รูปแบบ:

```text
Namespace.Type::Method(ParameterType1,ParameterType2)
```

ถ้าชื่อ method ไม่ overload สามารถละ parameter list ได้. Resolver จำกัด target อยู่ใน server assembly
สำหรับ server mod และ `Assembly-CSharp` สำหรับ client mod และคืน resolved signature ที่รวม full type,
parameter และ return type. Target ที่หาไม่พบ/กำกวม/เป็น constructor/abstract/generic/by-ref return จะ fail-closed.

ตัวอย่าง server Prefix ที่ข้าม original และกำหนดผลลัพธ์:

```csharp
IModMethodOverridesApi methods = api as IModMethodOverridesApi;
methods.RegisterMethodOverride(
    "Durango.Offline.ServerPlayer::StatusStaminaCostDelta()",
    ModMethodOverrideKind.Prefix,
    delegate(ModMethodOverrideContext context)
    {
        context.SetResult(0f);
        context.SkipOriginal = true;
    },
    100);
```

ตัวอย่าง client Postfix:

```csharp
IClientMethodOverridesApi methods = api as IClientMethodOverridesApi;
methods.RegisterMethodOverride(
    "PlayerBehavior::GetCurrentPosition()",
    ClientMethodOverrideKind.Postfix,
    delegate(ClientMethodOverrideContext context)
    {
        // context.Result อ่านผลเดิม; SetResult(...) ใช้เมื่อต้องการเปลี่ยนผล
    },
    0);
```

Handler แก้สมาชิกของ `Arguments`, เรียก `SetResult`, หรือกำหนด `SkipOriginal` ได้. Priority สูงทำงานก่อน;
priority เท่ากันเรียง mod ID และลำดับลงทะเบียนแบบ deterministic. หลาย Prefix/Postfix อยู่ร่วมกันได้ แต่
`Replace` มี owner ข้ามม็อดได้เพียงหนึ่งตัวต่อ method. Exception ของ handler ถูก isolate; Replace ที่ throw
จะ fallback ไป original. มี thread-local recursion guard ป้องกัน hook เรียก target เดิมวนกลับเข้าตัวเอง.

เมื่อ load phase ล้มเหลวหรือเรียก lifecycle disable ระบบ unpatch registration ของม็อด. Server แสดงรายการ,
จำนวน call/error และเวลารวมที่ `/admin/mods`; client ดู registration ผ่าน
`GetRegisteredMethodOverrides()` และ error ใน `clientmods.log`.

ข้อจำกัดที่ยังอยู่ใน TODO: build fingerprint/version pinning, capability/deny-list สำหรับ target สำคัญ,
hot reload ทั้ง assembly และ automated client runtime test. DLL mod เป็น trusted in-process code ไม่ใช่ sandbox.
