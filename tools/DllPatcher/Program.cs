using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

class Patcher
{
    static ModuleDefMD module;

    static int PatchMethod(TypeDef type, string methodName, int from, int to)
    {
        int replaced = 0;
        if (type == null)
        {
            return 0;
        }
        foreach (MethodDef method in type.Methods)
        {
            if (method.Name != methodName || !method.HasBody)
            {
                continue;
            }
            foreach (Instruction instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldc_I4 && instr.GetLdcI4Value() == from)
                {
                    instr.Operand = to;
                    replaced++;
                }
            }
        }
        return replaced;
    }

    static void PatchConstField(string typeName, string fieldName, int value)
    {
        TypeDef type = module.Find(typeName, false);
        FieldDef field = type?.FindField(fieldName);
        if (field != null && field.HasConstant)
        {
            field.Constant.Value = value;
        }
    }

    static void ForceLocalAssetBundles()
    {
        TypeDef abm = module.Find("AssetBundleManager", false);
        if (abm == null)
        {
            return;
        }
        MethodDef init = abm.FindMethod("Initialize");
        FieldDef fld = abm.FindField("_infoHolderName");
        if (init == null || !init.HasBody || fld == null)
        {
            Console.WriteLine("AssetBundleManager patch targets missing");
            return;
        }
        var body = init.Body;
        body.Instructions.Insert(0, OpCodes.Stfld.ToInstruction(fld));
        body.Instructions.Insert(0, OpCodes.Ldstr.ToInstruction("Info.5.2.1"));
        body.Instructions.Insert(0, OpCodes.Ldarg_0.ToInstruction());
        Console.WriteLine("AssetBundleManager.Initialize patched to force local streaming assets");
    }

    static void GuardTitleWidget()
    {
        TypeDef type = module.Find("Durango.UI.Control.UITitleWidget_PC", false);
        if (type == null)
        {
            Console.WriteLine("UITitleWidget_PC not found");
            return;
        }
        FieldDef fld = type.FindField("_currencies");
        foreach (string methodName in new[] { "OnEnable", "OnStart" })
        {
            MethodDef method = null;
            foreach (MethodDef m in type.Methods)
            {
                if (m.Name == methodName && m.HasBody)
                {
                    method = m;
                    break;
                }
            }
            if (method == null)
            {
                Console.WriteLine("UITitleWidget_PC." + methodName + " not found");
                continue;
            }
            var body = method.Body;
            Instruction retInstr = OpCodes.Ret.ToInstruction();
            Instruction lOk = OpCodes.Ldlen.ToInstruction();
            var ins = new[]
            {
                OpCodes.Ldarg_0.ToInstruction(),
                OpCodes.Ldfld.ToInstruction(fld),
                OpCodes.Dup.ToInstruction(),
                OpCodes.Brtrue.ToInstruction(lOk),
                OpCodes.Pop.ToInstruction(),
                OpCodes.Ret.ToInstruction(),
                lOk,
                OpCodes.Ldc_I4_4.ToInstruction(),
                OpCodes.Blt.ToInstruction(retInstr),
            };
            for (int i = ins.Length - 1; i >= 0; i--)
            {
                body.Instructions.Insert(0, ins[i]);
            }
            body.Instructions.Add(retInstr);
            Console.WriteLine("guarded UITitleWidget_PC." + methodName);
        }
    }

    static void PatchAppDataBasePath()
    {
        TypeDef appData = module.Find("Durango.Utils.AppData", false);
        if (appData == null)
        {
            Console.WriteLine("AppData type not found");
            return;
        }
        MethodDef getter = null;
        foreach (MethodDef m in appData.Methods)
        {
            if (m.Name == "get_BasePath" && m.HasBody)
            {
                getter = m;
                break;
            }
        }
        if (getter == null)
        {
            Console.WriteLine("get_BasePath not found");
            return;
        }
        AssemblyRef mscorlib = module.CorLibTypes.AssemblyRef;
        TypeRef envType = new TypeRefUser(module, "System", "Environment", mscorlib);
        MethodSig getEnvSig = MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String);
        MemberRef getEnv = new MemberRefUser(module, "GetEnvironmentVariable", getEnvSig, envType);

        TypeRef strType = new TypeRefUser(module, "System", "String", mscorlib);
        MethodSig isNullSig = MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String);
        MemberRef isNullOrEmpty = new MemberRefUser(module, "IsNullOrEmpty", isNullSig, strType);

        var body = getter.Body;
        body.InitLocals = true;
        Local envLocal = new Local(module.CorLibTypes.String);
        body.Variables.Add(envLocal);

        Instruction first = body.Instructions[0];
        var ins = new[]
        {
            OpCodes.Ldstr.ToInstruction("DURANGO_APPDATA"),
            OpCodes.Call.ToInstruction(getEnv),
            OpCodes.Stloc.ToInstruction(envLocal),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Call.ToInstruction(isNullOrEmpty),
            OpCodes.Brtrue.ToInstruction(first),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Ret.ToInstruction(),
        };
        for (int i = ins.Length - 1; i >= 0; i--)
        {
            body.Instructions.Insert(0, ins[i]);
        }
        Console.WriteLine("patched AppData.get_BasePath with DURANGO_APPDATA env override");
    }

    static MethodDef AddIslandPortHelper()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        if (server == null)
        {
            Console.WriteLine("Server type not found");
            return null;
        }
        AssemblyRef mscorlib = module.CorLibTypes.AssemblyRef;
        TypeRef envType = new TypeRefUser(module, "System", "Environment", mscorlib);
        MethodSig getEnvSig = MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String);
        MemberRef getEnv = new MemberRefUser(module, "GetEnvironmentVariable", getEnvSig, envType);

        TypeRef strType = new TypeRefUser(module, "System", "String", mscorlib);
        MethodSig isNullSig = MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String);
        MemberRef isNullOrEmpty = new MemberRefUser(module, "IsNullOrEmpty", isNullSig, strType);

        TypeRef intType = new TypeRefUser(module, "System", "Int32", mscorlib);
        MethodSig tryParseSig = MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String, new ByRefSig(module.CorLibTypes.Int32));
        MemberRef tryParse = new MemberRefUser(module, "TryParse", tryParseSig, intType);

        MethodDef helper = new MethodDefUser(
            "GetIslandPort",
            MethodSig.CreateStatic(module.CorLibTypes.Int32),
            MethodImplAttributes.IL | MethodImplAttributes.Managed,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig);
        helper.Body = new CilBody();
        helper.Body.InitLocals = true;
        helper.Body.Variables.Add(new Local(module.CorLibTypes.String));
        helper.Body.Variables.Add(new Local(module.CorLibTypes.Int32));
        var instr = helper.Body.Instructions;
        Instruction retDefault = OpCodes.Ldc_I4.ToInstruction(8390);
        Instruction retValue = OpCodes.Ldloc_1.ToInstruction();
        instr.Add(OpCodes.Ldstr.ToInstruction("DURANGO_ISLAND_PORT"));
        instr.Add(OpCodes.Call.ToInstruction(getEnv));
        instr.Add(OpCodes.Stloc_0.ToInstruction());
        instr.Add(OpCodes.Ldloc_0.ToInstruction());
        instr.Add(OpCodes.Call.ToInstruction(isNullOrEmpty));
        instr.Add(OpCodes.Brtrue.ToInstruction(retDefault));
        instr.Add(OpCodes.Ldloc_0.ToInstruction());
        instr.Add(OpCodes.Ldloca_S.ToInstruction(helper.Body.Variables[1]));
        instr.Add(OpCodes.Call.ToInstruction(tryParse));
        instr.Add(OpCodes.Brfalse.ToInstruction(retDefault));
        instr.Add(OpCodes.Ldloc_1.ToInstruction());
        instr.Add(OpCodes.Ldc_I4_0.ToInstruction());
        instr.Add(OpCodes.Ble.ToInstruction(retDefault));
        instr.Add(retValue);
        instr.Add(OpCodes.Ret.ToInstruction());
        instr.Add(retDefault);
        instr.Add(OpCodes.Ret.ToInstruction());
        server.Methods.Add(helper);
        Console.WriteLine("added Durango.Offline.Server.GetIslandPort()");
        return helper;
    }

    static void PatchPortSite(TypeDef type, string methodName, int oldValue, MethodDef helper, bool plusOne)
    {
        if (type == null || helper == null)
        {
            return;
        }
        foreach (MethodDef method in type.Methods)
        {
            if (method.Name != methodName || !method.HasBody)
            {
                continue;
            }
            var instr = method.Body.Instructions;
            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Ldc_I4 && instr[i].GetLdcI4Value() == oldValue)
                {
                    var replacement = new[]
                    {
                        OpCodes.Call.ToInstruction(helper),
                    };
                    if (plusOne)
                    {
                        replacement = new[]
                        {
                            OpCodes.Call.ToInstruction(helper),
                            OpCodes.Ldc_I4_1.ToInstruction(),
                            OpCodes.Add.ToInstruction(),
                        };
                    }
                    instr[i] = replacement[0];
                    for (int k = 1; k < replacement.Length; k++)
                    {
                        instr.Insert(i + k, replacement[k]);
                    }
                    Console.WriteLine("patched {0}.{1} port site", type.FullName, methodName);
                    return;
                }
            }
        }
        Console.WriteLine("WARN: port site not found in {0}.{1}", type.FullName, methodName);
    }

    // เกมมีบั๊ก: รายการเซิร์ฟที่ค้นเจอด้วย UDP knock จะ "ข้ามเงียบ ๆ" ถ้า IP นั้นเป็น IP ของเครื่องตัวเอง
    // (MenuListGroupBase.TryKnockLoaclNetwork.OnSelectItem) ⇒ เทสบนเครื่องเดียวเลือกจากรายการไม่ได้เลย
    // แก้โดยบังคับให้ Enumerable.Contains คืน false เสมอ (pop, pop, ldc.i4.0)
    static void PatchSelfIpFilter()
    {
        TypeDef outer = module.Find("Durango.UI.MenuListGroupBase", false);
        TypeDef knock = null;
        if (outer != null)
        {
            foreach (TypeDef nested in outer.NestedTypes)
            {
                if (nested.Name == "TryKnockLoaclNetwork")
                {
                    knock = nested;
                    break;
                }
            }
        }
        if (knock == null)
        {
            Console.WriteLine("WARN: TryKnockLoaclNetwork not found");
            return;
        }
        foreach (MethodDef method in knock.Methods)
        {
            if (method.Name != "OnSelectItem" || !method.HasBody)
            {
                continue;
            }
            var instr = method.Body.Instructions;
            for (int i = 0; i < instr.Count; i++)
            {
                IMethod called = instr[i].Operand as IMethod;
                if (called == null || called.Name != "Contains")
                {
                    continue;
                }
                // แก้ instruction เดิมในที่ (ไม่สร้างตัวใหม่) เผื่อมี branch ชี้มาที่มันอยู่
                instr[i].OpCode = OpCodes.Pop;
                instr[i].Operand = null;
                instr.Insert(i + 1, OpCodes.Pop.ToInstruction());
                instr.Insert(i + 2, OpCodes.Ldc_I4_0.ToInstruction());
                method.Body.SimplifyBranches();
                method.Body.OptimizeBranches();
                Console.WriteLine("patched OnSelectItem: self-IP filter disabled");
                return;
            }
        }
        Console.WriteLine("WARN: Contains call not found in OnSelectItem");
    }

    // ต่อ server เองโดยไม่ต้องคลิกเมนู: ตั้ง env DURANGO_AUTOCONNECT=127.0.0.1
    // แทรกท้าย Server.BeginServer (จุดที่ _localPlayer ถูกเซ็ตแล้ว ซึ่ง ConnectTo ต้องใช้)
    // ยิงครั้งเดียวต่อ process — กันวนกลับมาที่ title ซ้ำ ๆ
    static void PatchAutoConnect()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        MethodDef begin = server?.FindMethod("BeginServer");
        MethodDef connectTo = server?.FindMethod("ConnectTo");
        if (begin == null || !begin.HasBody || connectTo == null)
        {
            Console.WriteLine("WARN: BeginServer/ConnectTo not found");
            return;
        }

        FieldDef flag = new FieldDefUser(
            "_autoConnected",
            new FieldSig(module.CorLibTypes.Boolean),
            FieldAttributes.Public | FieldAttributes.Static);
        server.Fields.Add(flag);

        AssemblyRef mscorlib = module.CorLibTypes.AssemblyRef;
        TypeRef envType = new TypeRefUser(module, "System", "Environment", mscorlib);
        MethodSig getEnvSig = MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String);
        MemberRef getEnv = new MemberRefUser(module, "GetEnvironmentVariable", getEnvSig, envType);

        TypeRef strType = new TypeRefUser(module, "System", "String", mscorlib);
        MethodSig isNullSig = MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String);
        MemberRef isNullOrEmpty = new MemberRefUser(module, "IsNullOrEmpty", isNullSig, strType);

        var body = begin.Body;
        body.InitLocals = true;
        Local envLocal = new Local(module.CorLibTypes.String);
        body.Variables.Add(envLocal);

        Instruction last = body.Instructions[body.Instructions.Count - 1];   // ret
        var ins = new[]
        {
            OpCodes.Ldsfld.ToInstruction(flag),
            OpCodes.Brtrue.ToInstruction(last),
            OpCodes.Ldstr.ToInstruction("DURANGO_AUTOCONNECT"),
            OpCodes.Call.ToInstruction(getEnv),
            OpCodes.Stloc.ToInstruction(envLocal),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Call.ToInstruction(isNullOrEmpty),
            OpCodes.Brtrue.ToInstruction(last),
            OpCodes.Ldc_I4_1.ToInstruction(),
            OpCodes.Stsfld.ToInstruction(flag),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Call.ToInstruction(connectTo),
        };
        int at = body.Instructions.Count - 1;
        for (int i = 0; i < ins.Length; i++)
        {
            body.Instructions.Insert(at + i, ins[i]);
        }
        body.SimplifyBranches();
        body.OptimizeBranches();
        Console.WriteLine("patched Server.BeginServer with DURANGO_AUTOCONNECT");
    }

    /// <summary>
    /// ให้ client สร้าง "ตัวสัตว์" จาก packet AppearAnimal ของ server
    ///
    /// ของเดิม: handler ของ AppearAnimal ใน AnimalManager ทำงานเฉพาะ entity ที่ _animals รู้จักอยู่แล้ว
    ///   if (_animals.TryGetValue(msg.EntityId, out var v)) { v?.Appear(); OnPostAppearAnimal(msg); }
    /// ไม่มี else ⇒ สัตว์ที่ server ส่งมาใหม่ "ไม่ถูกสร้างเลย" (ในเกมจริงสัตว์เป็นของฝั่ง client
    /// ตัวที่สร้างจริงคือ MakeAnimalObject ซึ่งถูกเรียกจากทาง offline เท่านั้น)
    ///
    /// patch: แทรก MakeAnimalObject(msg, Vector3.zero) ไว้ต้น handler
    /// - MakeAnimalObject กันซ้ำเองด้วย _ghosts.ContainsKey จึงเรียกซ้ำได้ไม่พัง
    /// - ตำแหน่งจริงถูกตั้งจาก msg.Move ภายใน MakeAnimalObject อยู่แล้ว (pos ที่ส่งไปเป็นแค่จุด Instantiate)
    /// </summary>
    static void PatchServerAnimalSpawn()
    {
        TypeDef mgr = module.Find("AnimalManager", false);
        if (mgr == null)
        {
            Console.WriteLine("WARN: AnimalManager not found");
            return;
        }
        MethodDef makeObj = mgr.FindMethod("MakeAnimalObject");
        if (makeObj == null || makeObj.Parameters.Count != 3)
        {
            Console.WriteLine("WARN: MakeAnimalObject(AppearAnimal, Vector3) not found");
            return;
        }
        // พารามิเตอร์ตัวที่ 2 คือ UnityEngine.Vector3 — ยืม type ref จากตรงนี้ไปทำ Vector3.zero
        TypeSig vec3 = makeObj.Parameters[2].Type;
        MemberRef vecZero = new MemberRefUser(module, "get_zero",
            MethodSig.CreateStatic(vec3), vec3.ToTypeDefOrRef());

        MethodDef handler = null;
        TypeDef holder = null;
        foreach (TypeDef t in new[] { mgr }.Concat(mgr.NestedTypes))
        {
            foreach (MethodDef m in t.Methods)
            {
                if (!m.HasBody || m.Parameters.Count < 2)
                {
                    continue;
                }
                bool takesAppear = m.Parameters.Any(p => p.Type != null && p.Type.TypeName == "AppearAnimal");
                if (!takesAppear)
                {
                    continue;
                }
                bool callsPost = m.Body.Instructions.Any(i => i.Operand is IMethod im && im.Name == "OnPostAppearAnimal");
                if (callsPost)
                {
                    handler = m;
                    holder = t;
                    break;
                }
            }
            if (handler != null)
            {
                break;
            }
        }
        if (handler == null)
        {
            Console.WriteLine("WARN: AppearAnimal handler not found");
            return;
        }

        // หา index ของพารามิเตอร์ msg (นับรวม this ถ้าเป็น instance method)
        int msgIndex = -1;
        for (int i = 0; i < handler.Parameters.Count; i++)
        {
            if (handler.Parameters[i].Type != null && handler.Parameters[i].Type.TypeName == "AppearAnimal")
            {
                msgIndex = i;
                break;
            }
        }

        // ตัวรับ (AnimalManager instance): ถ้า handler อยู่บน AnimalManager เองใช้ this ได้เลย
        // ถ้าเป็น closure class ต้องโหลดฟิลด์ที่เก็บ this ไว้ (<>4__this)
        var body = handler.Body;
        var prologue = new System.Collections.Generic.List<Instruction>();
        if (holder == mgr && !handler.IsStatic)
        {
            prologue.Add(OpCodes.Ldarg_0.ToInstruction());
        }
        else
        {
            FieldDef self = holder.Fields.FirstOrDefault(f => f.FieldType != null && f.FieldType.TypeName == "AnimalManager");
            if (self == null || handler.IsStatic)
            {
                Console.WriteLine("WARN: cannot reach AnimalManager instance from handler " + holder.Name + "." + handler.Name);
                return;
            }
            prologue.Add(OpCodes.Ldarg_0.ToInstruction());
            prologue.Add(OpCodes.Ldfld.ToInstruction(self));
        }
        prologue.Add(OpCodes.Ldarg.ToInstruction(handler.Parameters[msgIndex]));
        prologue.Add(OpCodes.Call.ToInstruction(vecZero));
        prologue.Add(OpCodes.Callvirt.ToInstruction(makeObj));

        for (int i = 0; i < prologue.Count; i++)
        {
            body.Instructions.Insert(i, prologue[i]);
        }
        body.SimplifyBranches();
        body.OptimizeBranches();
        Console.WriteLine($"patched {holder.Name}.{handler.Name} — สัตว์จาก server จะถูกสร้างจริงแล้ว");
    }

    /// <summary>
    /// ซ่อนเมนูของระบบที่ server ยังไม่ได้ทำ (Beta 1.0)
    ///
    /// `MenuSystem.IsHiddenMenu(MenuType)` เป็น static ที่ทุกที่ใช้ตัดสินว่าจะโชว์เมนูไหม
    /// เราแทรกโค้ดต้นเมทอด: ถ้า type อยู่ในรายการที่ยังไม่ได้ทำ ให้คืน true (ซ่อน) ทันที
    /// ที่เหลือปล่อยให้ logic เดิมทำงานต่อ
    ///
    /// ทำไมต้อง patch: ฝั่ง server สั่งซ่อนได้แค่เมนูเดียว (party.ui_enabled) — ตัวอื่นไม่มี binding
    /// (ดู client/OptionSystem.cs `_menuBindings`)
    /// </summary>
    static void PatchHideUnimplementedMenus()
    {
        TypeDef menuSystem = module.Find("MenuSystem", false);
        MethodDef isHidden = menuSystem?.FindMethod("IsHiddenMenu");
        if (isHidden == null || !isHidden.HasBody || !isHidden.IsStatic)
        {
            Console.WriteLine("WARN: MenuSystem.IsHiddenMenu not found");
            return;
        }

        // ค่าตาม enum MenuType (ลำดับใน client/MenuType.cs)
        // ซ่อน: ตลาด · เพื่อน · เมล · สารานุกรม · แคลน · ฝ่าย · ไทม์ไลน์ · เพ็ท · ที่ดิน · ร้านค้า
        //      · อีเวนต์ · ภารกิจ · ระบบสอนเล่น · ปาร์ตี้ · ประกาศ · เลือกตัวละคร · คอมมูนิตี้
        //      · offerwall · เกาะ PvP · เนื้อเรื่อง · เพลง
        int[] hidden =
        {
            4,  /*Market*/      5,  /*Social*/     6,  /*Mail*/       9,  /*Encyclopedia*/
            10, /*Clan*/        11, /*Faction*/    12, /*Timeline*/   13, /*Pet*/
            14, /*Estate*/      15, /*Shop*/       16, /*Event*/      17, /*Quest*/
            18, /*LearningGuide*/ 19, /*Party*/    20, /*Notice*/     21, /*PlayerSelection*/
            22, /*OfficialCommunity*/ 23, /*Offerwall*/ 24, /*PvpIsland*/ 25, /*Story*/
            30, /*Music*/       32, /*CharacterOnMenu*/ 33, /*MusicOnMenu*/ 34, /*StoryOnMenu*/
        };

        var body = isHidden.Body;
        Instruction retTrue = OpCodes.Ldc_I4_1.ToInstruction();
        var prologue = new System.Collections.Generic.List<Instruction>();
        for (int i = 0; i < hidden.Length; i++)
        {
            prologue.Add(OpCodes.Ldarg_0.ToInstruction());
            prologue.Add(OpCodes.Ldc_I4.ToInstruction(hidden[i]));
            prologue.Add(OpCodes.Beq.ToInstruction(retTrue));
        }
        for (int i = 0; i < prologue.Count; i++)
        {
            body.Instructions.Insert(i, prologue[i]);
        }
        body.Instructions.Add(retTrue);
        body.Instructions.Add(OpCodes.Ret.ToInstruction());
        body.SimplifyBranches();
        body.OptimizeBranches();
        Console.WriteLine($"patched MenuSystem.IsHiddenMenu — ซ่อน {hidden.Length} เมนูที่ยังไม่ได้ทำ");
    }

    static void Main(string[] args)
    {
        string dllPath = args[0];
        module = ModuleDefMD.Load(dllPath);

        PatchConstField("Durango.Offline.GameServer", "DefaultPort", 8391);
        PatchConstField("Durango.Offline.Gateway", "DefaultPort", 8390);
        ForceLocalAssetBundles();
        PatchAppDataBasePath();
        GuardTitleWidget();
        PatchSelfIpFilter();
        PatchAutoConnect();
        PatchServerAnimalSpawn();
        PatchHideUnimplementedMenus();

        MethodDef helper = AddIslandPortHelper();

        PatchPortSite(module.Find("Durango.Offline.GameServer", false), ".ctor", 8391, helper, true);
        PatchPortSite(module.Find("Durango.Offline.Gateway", false), ".ctor", 8390, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Server", false), ".ctor", 8390, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Player", false), "BackToStableIsland", 8390, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Player", false), "SailUnstableIsland", 8390, helper, false);

        // fallback: original values (if first patch never ran)
        PatchPortSite(module.Find("Durango.Offline.GameServer", false), ".ctor", 8191, helper, true);
        PatchPortSite(module.Find("Durango.Offline.Gateway", false), ".ctor", 8190, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Server", false), ".ctor", 8190, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Player", false), "BackToStableIsland", 8190, helper, false);
        PatchPortSite(module.Find("Durango.Offline.Player", false), "SailUnstableIsland", 8190, helper, false);

        module.Write(dllPath + ".patched.dll");
        Console.WriteLine("done -> " + dllPath + ".patched.dll");
    }
}
