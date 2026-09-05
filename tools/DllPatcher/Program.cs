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

    /// <summary>
    /// [ใหม่] บังคับพารามิเตอร์ URL/gatewayUrl ตัวแรกที่เข้าเมธอดนี้ให้ชี้เซิร์ฟเราแทน — ใช้กับ
    /// เมธอดสั้น ๆ ไม่มี branch ซับซ้อน (Clusters.RequestAccounts, GameManager.SetCluster) เท่านั้น
    /// ไม่แตะ resources.assets เลย, ไม่แตะ TitleMenuGroup state machine เลย (เสี่ยงน้อยกว่ามาก)
    /// ทำงานเฉพาะเมื่อตั้ง env DURANGO_AUTOCONNECT=&lt;ip&gt; (ไม่ตั้ง = พฤติกรรมเดิม 100%)
    /// </summary>
    static void PatchForceGatewayUrl(string typeName, string methodName, int paramIndex)
    {
        TypeDef type = module.Find(typeName, false);
        MethodDef method = type?.FindMethod(methodName);
        if (method == null || !method.HasBody)
        {
            Console.WriteLine("WARN: " + typeName + "." + methodName + " not found");
            return;
        }
        if (paramIndex >= method.Parameters.Count)
        {
            Console.WriteLine("WARN: param index out of range for " + typeName + "." + methodName);
            return;
        }
        Parameter targetParam = method.Parameters[paramIndex];

        AssemblyRef mscorlib = module.CorLibTypes.AssemblyRef;
        TypeRef envType = new TypeRefUser(module, "System", "Environment", mscorlib);
        MethodSig getEnvSig = MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String);
        MemberRef getEnv = new MemberRefUser(module, "GetEnvironmentVariable", getEnvSig, envType);

        TypeRef strType = new TypeRefUser(module, "System", "String", mscorlib);
        MethodSig isNullSig = MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String);
        MemberRef isNullOrEmpty = new MemberRefUser(module, "IsNullOrEmpty", isNullSig, strType);

        MethodSig concat3Sig = MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String, module.CorLibTypes.String, module.CorLibTypes.String);
        MemberRef concat3 = new MemberRefUser(module, "Concat", concat3Sig, strType);

        var body = method.Body;
        body.InitLocals = true;
        Local envLocal = new Local(module.CorLibTypes.String);
        body.Variables.Add(envLocal);

        Instruction first = body.Instructions[0];
        var ins = new[]
        {
            OpCodes.Ldstr.ToInstruction("DURANGO_AUTOCONNECT"),
            OpCodes.Call.ToInstruction(getEnv),
            OpCodes.Stloc.ToInstruction(envLocal),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Call.ToInstruction(isNullOrEmpty),
            OpCodes.Brtrue.ToInstruction(first),
            OpCodes.Ldstr.ToInstruction("http://"),
            OpCodes.Ldloc.ToInstruction(envLocal),
            OpCodes.Ldstr.ToInstruction(":8190"),
            OpCodes.Call.ToInstruction(concat3),
            OpCodes.Starg.ToInstruction(targetParam),
        };
        for (int i = ins.Length - 1; i >= 0; i--)
        {
            body.Instructions.Insert(0, ins[i]);
        }
        Console.WriteLine("patched " + typeName + "." + methodName + " param[" + paramIndex + "] with DURANGO_AUTOCONNECT gateway override");
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

    // After a gateway is entered from Main -> Visit Friend's Island, the old
    // client keeps the previous account cache. Refresh it before the title
    // state tries to create the login session; otherwise it reports 400 even
    // though knock/admission/entry all succeeded.
    static void PatchForceSetClustersAccountRefresh()
    {
        TypeDef title = module.Find("Durango.UI.TitleMenuUserControlBase", false);
        MethodDef force = title?.FindMethod("ForceSetClusters");
        MethodDef update = title?.FindMethod("UpdateServerAndPlayerInfo");
        if (force == null || !force.HasBody || update == null || !update.HasBody)
        {
            Console.WriteLine("WARN: ForceSetClusters/UpdateServerAndPlayerInfo not found");
            return;
        }

        for (int i = 0; i < force.Body.Instructions.Count; i++)
        {
            if (force.Body.Instructions[i].OpCode == OpCodes.Call &&
                force.Body.Instructions[i].Operand is IMethod called &&
                called.Name == "UpdateServerAndPlayerInfo")
            {
                Console.WriteLine("ForceSetClusters account refresh already present");
                return;
            }
        }

        Instruction ret = force.Body.Instructions.LastOrDefault(x => x.OpCode == OpCodes.Ret);
        if (ret == null)
        {
            Console.WriteLine("WARN: ForceSetClusters has no return");
            return;
        }
        int at = force.Body.Instructions.IndexOf(ret);
        force.Body.Instructions.Insert(at++, OpCodes.Ldarg_0.ToInstruction());
        if (update.Parameters.Count > 1)
        {
            force.Body.Instructions.Insert(at++, OpCodes.Ldc_I4_0.ToInstruction());
        }
        force.Body.Instructions.Insert(at, OpCodes.Call.ToInstruction(update));
        force.Body.SimplifyBranches();
        force.Body.OptimizeBranches();
        Console.WriteLine("patched TitleMenuUserControlBase.ForceSetClusters account refresh");
    }

    /// <summary>
    /// [แก้เอง] 29 ส.ค. 2026 — helper ที่ฝังเข้า DLL: อ่านที่อยู่เซิร์ฟ **จาก server.txt โดยตรง**
    /// (เจ้าของสั่ง: "การกำหนด ip เซิฟ แก้ให้อ่านจาก server.txt จริง ๆ ไม่ hardcode")
    ///
    /// ลำดับ: server.txt (บรรทัดแรกที่ไม่ใช่คอมเมนต์ #) → env DURANGO_AUTOCONNECT → null
    /// เติม "http://" ให้อัตโนมัติถ้าในไฟล์ใส่มาแค่ "ip:port" — เพราะ Cluster.GatewayUrlRoot ต้องมี scheme
    /// ห่อ try/catch ไว้ทั้งก้อน อ่านไฟล์พังยังไงก็ไม่ทำให้เกมล้มที่หน้าไตเติ้ล
    /// </summary>
    static MethodDef AddServerTargetHelper()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        if (server == null)
        {
            Console.WriteLine("WARN: Durango.Offline.Server not found for server.txt reader");
            return null;
        }
        MethodDef existing = server.FindMethod("GetServerTargetFromFile");
        if (existing != null)
        {
            Console.WriteLine("server.txt reader already present");
            return existing;
        }

        AssemblyRef mscorlib = module.CorLibTypes.AssemblyRef;
        SZArraySig strArr = new SZArraySig(module.CorLibTypes.String);

        TypeRef fileType = new TypeRefUser(module, "System.IO", "File", mscorlib);
        MemberRef fileExists = new MemberRefUser(module, "Exists",
            MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String), fileType);
        MemberRef readAllLines = new MemberRefUser(module, "ReadAllLines",
            MethodSig.CreateStatic(strArr, module.CorLibTypes.String), fileType);

        TypeRef strType = new TypeRefUser(module, "System", "String", mscorlib);
        MemberRef trim = new MemberRefUser(module, "Trim",
            MethodSig.CreateInstance(module.CorLibTypes.String), strType);
        MemberRef getLength = new MemberRefUser(module, "get_Length",
            MethodSig.CreateInstance(module.CorLibTypes.Int32), strType);
        MemberRef startsWith = new MemberRefUser(module, "StartsWith",
            MethodSig.CreateInstance(module.CorLibTypes.Boolean, module.CorLibTypes.String), strType);
        MemberRef concat = new MemberRefUser(module, "Concat",
            MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String, module.CorLibTypes.String), strType);

        TypeRef envType = new TypeRefUser(module, "System", "Environment", mscorlib);
        MemberRef getEnv = new MemberRefUser(module, "GetEnvironmentVariable",
            MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String), envType);

        TypeRef objType = new TypeRefUser(module, "System", "Object", mscorlib);

        MethodDef m = new MethodDefUser("GetServerTargetFromFile",
            MethodSig.CreateStatic(module.CorLibTypes.String),
            MethodImplAttributes.IL | MethodImplAttributes.Managed,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig);
        m.Body = new CilBody { InitLocals = true };
        Local vPath = new Local(module.CorLibTypes.String);
        Local vLines = new Local(strArr);
        Local vI = new Local(module.CorLibTypes.Int32);
        Local vT = new Local(module.CorLibTypes.String);
        Local vRet = new Local(module.CorLibTypes.String);
        foreach (Local l in new[] { vPath, vLines, vI, vT, vRet }) m.Body.Variables.Add(l);

        // labels
        Instruction tryStart = OpCodes.Ldstr.ToInstruction("server.txt");
        Instruction bodyStart = OpCodes.Ldloc.ToInstruction(vLines);
        Instruction next = OpCodes.Ldloc.ToInstruction(vI);
        Instruction loopChk = OpCodes.Ldloc.ToInstruction(vI);
        Instruction endTry = OpCodes.Leave.ToInstruction(OpCodes.Nop.ToInstruction());
        Instruction catchStart = OpCodes.Pop.ToInstruction();
        Instruction fallback = OpCodes.Ldstr.ToInstruction("DURANGO_AUTOCONNECT");
        Instruction retInstr = OpCodes.Ldloc.ToInstruction(vRet);
        Instruction storeHit = OpCodes.Ldloc.ToInstruction(vT);
        endTry.Operand = fallback;

        var il = m.Body.Instructions;
        il.Add(tryStart);                                     // "server.txt"
        il.Add(OpCodes.Stloc.ToInstruction(vPath));
        il.Add(OpCodes.Ldloc.ToInstruction(vPath));
        il.Add(OpCodes.Call.ToInstruction(fileExists));
        il.Add(OpCodes.Brfalse.ToInstruction(endTry));
        il.Add(OpCodes.Ldloc.ToInstruction(vPath));
        il.Add(OpCodes.Call.ToInstruction(readAllLines));
        il.Add(OpCodes.Stloc.ToInstruction(vLines));
        il.Add(OpCodes.Ldc_I4_0.ToInstruction());
        il.Add(OpCodes.Stloc.ToInstruction(vI));
        il.Add(OpCodes.Br.ToInstruction(loopChk));
        il.Add(bodyStart);                                    // lines
        il.Add(OpCodes.Ldloc.ToInstruction(vI));
        il.Add(OpCodes.Ldelem_Ref.ToInstruction());
        il.Add(OpCodes.Callvirt.ToInstruction(trim));
        il.Add(OpCodes.Stloc.ToInstruction(vT));
        il.Add(OpCodes.Ldloc.ToInstruction(vT));
        il.Add(OpCodes.Callvirt.ToInstruction(getLength));
        il.Add(OpCodes.Brfalse.ToInstruction(next));          // บรรทัดว่าง -> ข้าม
        il.Add(OpCodes.Ldloc.ToInstruction(vT));
        il.Add(OpCodes.Ldstr.ToInstruction("#"));
        il.Add(OpCodes.Callvirt.ToInstruction(startsWith));
        il.Add(OpCodes.Brtrue.ToInstruction(next));           // คอมเมนต์ -> ข้าม
        // เจอบรรทัดที่ใช้ได้: เติม http:// ถ้ายังไม่มี
        il.Add(storeHit);                                     // t
        il.Add(OpCodes.Ldstr.ToInstruction("http"));
        il.Add(OpCodes.Callvirt.ToInstruction(startsWith));
        Instruction doStore = OpCodes.Ldloc.ToInstruction(vT);
        il.Add(OpCodes.Brtrue.ToInstruction(doStore));
        il.Add(OpCodes.Ldstr.ToInstruction("http://"));
        il.Add(OpCodes.Ldloc.ToInstruction(vT));
        il.Add(OpCodes.Call.ToInstruction(concat));
        il.Add(OpCodes.Stloc.ToInstruction(vT));
        il.Add(doStore);                                      // t
        il.Add(OpCodes.Stloc.ToInstruction(vRet));
        il.Add(OpCodes.Leave.ToInstruction(retInstr));
        il.Add(next);                                         // i
        il.Add(OpCodes.Ldc_I4_1.ToInstruction());
        il.Add(OpCodes.Add.ToInstruction());
        il.Add(OpCodes.Stloc.ToInstruction(vI));
        il.Add(loopChk);                                      // i
        il.Add(OpCodes.Ldloc.ToInstruction(vLines));
        il.Add(OpCodes.Ldlen.ToInstruction());
        il.Add(OpCodes.Conv_I4.ToInstruction());
        il.Add(OpCodes.Blt.ToInstruction(bodyStart));
        il.Add(endTry);                                       // leave -> fallback
        il.Add(catchStart);                                   // pop
        il.Add(OpCodes.Leave.ToInstruction(fallback));
        il.Add(fallback);                                     // "DURANGO_AUTOCONNECT"
        il.Add(OpCodes.Call.ToInstruction(getEnv));
        il.Add(OpCodes.Stloc.ToInstruction(vRet));
        il.Add(retInstr);                                     // ret value
        il.Add(OpCodes.Ret.ToInstruction());

        m.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tryStart,
            TryEnd = catchStart,
            HandlerStart = catchStart,
            HandlerEnd = fallback,
            CatchType = objType,
        });
        m.Body.SimplifyBranches();
        m.Body.OptimizeBranches();
        server.Methods.Add(m);
        Console.WriteLine("added Durango.Offline.Server.GetServerTargetFromFile() (อ่าน server.txt ตรง ๆ)");
        return m;
    }

    static MethodDef _serverTarget;

    /// <summary>
    /// [แก้เอง] 30 ส.ค. 2026 — เปิดระบบ mod บน DLL ต้นฉบับ ด้วยการแทรก **คำสั่งเดียว**
    /// `ClientModLoader.LoadAll()` ที่ต้น `GameManager.Start()`
    ///
    /// จุดสำคัญ: ตัว loader (1,744 บรรทัด) **ไม่ได้ถูกยัดเข้า DLL ต้นฉบับ** แต่อยู่ใน assembly แยก
    /// `DurangoClientMods.dll` (ดู tools/ClientModHost/) ⇒ แตะต้นฉบับน้อยที่สุด และแก้ระบบ mod ทีหลัง
    /// ได้โดยไม่ต้องแพตช์เกมใหม่
    ///
    /// ไฟล์ที่ต้องวางคู่กันในโฟลเดอร์ Managed: DurangoClientMods.dll · DurangoClientModSdk.dll · 0Harmony.dll
    /// (ถ้าไฟล์หาย เกมจะโยน TypeLoadException ตอนบูต — จึงต้องแจกไปพร้อมกันเสมอ)
    /// </summary>
    static void PatchInjectModLoader()
    {
        TypeDef gameManager = module.Find("GameManager", false);
        MethodDef start = gameManager?.FindMethod("Start");
        if (start == null || !start.HasBody)
        {
            Console.WriteLine("WARN: GameManager.Start not found — ข้ามการเปิดระบบ mod");
            return;
        }
        if (start.Body.Instructions.Any(i => i.Operand is IMethod m && m.Name == "LoadAll"))
        {
            Console.WriteLine("mod loader hook already present");
            return;
        }

        // อ้างอิงข้ามไปยัง assembly ภายนอก DurangoClientMods
        AssemblyRefUser modAsm = new AssemblyRefUser("DurangoClientMods", new Version(0, 0, 0, 0));
        TypeRefUser loaderType = new TypeRefUser(module, string.Empty, "ClientModLoader", modAsm);
        MemberRefUser loadAll = new MemberRefUser(module, "LoadAll",
            MethodSig.CreateStatic(module.CorLibTypes.Void), loaderType);

        start.Body.Instructions.Insert(0, OpCodes.Call.ToInstruction(loadAll));
        start.Body.SimplifyBranches();
        start.Body.OptimizeBranches();
        Console.WriteLine("patched GameManager.Start -> ClientModLoader.LoadAll() (เปิดระบบ mod)");
    }

    /// <summary>
    /// [แก้เอง] 29 ส.ค. 2026 — บังคับ Server.AutoConnectTarget ให้คืนค่าว่างเสมอ
    /// (เจ้าของสั่ง: "จะเอาแบบเดิม กดปุ่มค่อยเชื่อมเซิร์ฟ" / "auto connect อีกละ")
    ///
    /// ต้นตอ: ใน DLL ฐาน (.bak) getter ตัวนี้คืนค่า env DURANGO_AUTOCONNECT ซึ่ง DurangoUpdater ตั้งให้จาก
    /// server.txt ⇒ TitleMenuUserControlBase.OnConfirm เห็นว่ามีค่า เลยยิง ConnectTo ทันทีตั้งแต่หน้าไตเติ้ล
    /// ⇒ **ข้ามหน้า Main UI ไปเข้าโปรล็อกเลย** ผู้เล่นไม่ได้เลือกโหมดเอง
    /// (DLL ชุดที่เคยแจกถูกบังคับให้คืน String.Empty อยู่แล้ว จึงมีเมนูให้เลือก — ทำให้เหมือนกัน)
    ///
    /// การต่อเซิร์ฟจริงไม่ได้หายไป — ย้ายไปตอนกดปุ่ม "Dinoworld Server" ซึ่งอ่าน server.txt ผ่าน
    /// GetServerTargetFromFile() แทน
    /// </summary>
    static void PatchDisableAutoConnectTarget()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        MethodDef getter = server?.FindMethod("get_AutoConnectTarget");
        if (getter == null || !getter.HasBody)
        {
            Console.WriteLine("WARN: Server.get_AutoConnectTarget not found");
            return;
        }
        MemberRef empty = new MemberRefUser(module, "Empty",
            new FieldSig(module.CorLibTypes.String),
            new TypeRefUser(module, "System", "String", module.CorLibTypes.AssemblyRef));
        getter.Body.Instructions.Clear();
        getter.Body.ExceptionHandlers.Clear();
        getter.Body.Variables.Clear();
        getter.Body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(empty));
        getter.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        Console.WriteLine("patched Server.AutoConnectTarget -> String.Empty (ไม่ auto-connect ที่หน้าไตเติ้ล)");
    }

    // [แก้เอง] 29 ส.ค. 2026 — ข้ามหน้า "Region / Get Your Own Tamed Island!" ตอนสร้างตัวละคร
    // (เจ้าของสั่ง: "หน้านี้เอาออกได้ไหม ข้ามไปเลย")
    //
    // ปลอดภัยเพราะ: (1) SelectPersonalRegion.Awake() สุ่มเลือกเกาะให้อยู่แล้ว SelectedRegionid จึงไม่ว่าง
    // (2) ฝั่งเซิร์ฟ CharacterService.Create() **แค่ log ค่า region เฉย ๆ ไม่ได้เอาไปใช้จริง** — โลก/เกาะ
    // มาจาก data/config.json ของเซิร์ฟ ⇒ ผู้เล่นเลือกอะไรก็ไม่มีผลกับเกมนี้อยู่แล้ว
    //
    // วิธี: ไม่ลบหน้าออกจาก _pages (เสี่ยงพัง index) แต่ทำ 2 อย่างใน OpenCreateCharacter
    //   1. บังคับหน้าเริ่มต้นเป็น index 1 (Preset = เพศ/อาชีพ) แทน 0 (Region)
    //   2. ตั้ง CanBack ของหน้า Preset เป็น false — กันผู้เล่นกด Back ย้อนกลับเข้าหน้า Region
    static void PatchSkipRegionSelect()
    {
        TypeDef group = module.Find("Durango.UI.EditPlayerDisplayGroup", false);
        MethodDef open = group?.FindMethod("OpenCreateCharacter");
        if (open == null || !open.HasBody)
        {
            Console.WriteLine("WARN: EditPlayerDisplayGroup.OpenCreateCharacter not found");
            return;
        }
        var code = open.Body.Instructions;

        // 1. ทุกจุดที่เขียนค่าลงตัวแปร "page" (local 1) ให้ push 1 แทนค่าเดิม
        //    (มี 2 จุด: ค่าเริ่มต้น page=0 และในลูปที่เจอ State.Region แล้ว page=i)
        int pageWrites = 0;
        for (int i = 1; i < code.Count; i++)
        {
            Local target = code[i].GetLocal(open.Body.Variables);
            bool isStoreToPage = target != null && target.Index == 1 &&
                (code[i].OpCode == OpCodes.Stloc_1 || code[i].OpCode == OpCodes.Stloc || code[i].OpCode == OpCodes.Stloc_S);
            if (!isStoreToPage) continue;
            if (code[i - 1].OpCode == OpCodes.Ldc_I4_1) continue;   // แพตช์ไปแล้ว
            code[i - 1].OpCode = OpCodes.Ldc_I4_1;
            code[i - 1].Operand = null;
            pageWrites++;
        }

        // 2. หน้าที่ 2 (Preset) — CanBack: true -> false
        int canBackSeen = 0;
        bool canBackPatched = false;
        for (int i = 1; i < code.Count; i++)
        {
            if (code[i].OpCode != OpCodes.Stfld) continue;
            if (!(code[i].Operand is IField f) || f.Name != "CanBack") continue;
            canBackSeen++;
            if (canBackSeen != 2) continue;                          // 1=Region, 2=Preset
            if (code[i - 1].OpCode == OpCodes.Ldc_I4_1)
            {
                code[i - 1].OpCode = OpCodes.Ldc_I4_0;
                code[i - 1].Operand = null;
                canBackPatched = true;
            }
            break;
        }

        open.Body.SimplifyBranches();
        open.Body.OptimizeBranches();
        Console.WriteLine(pageWrites > 0 || canBackPatched
            ? $"patched EditPlayerDisplayGroup to skip Region page (page writes={pageWrites}, preset CanBack={canBackPatched})"
            : "Region page skip already patched");
    }

    static void PatchStaleAutoConnectTarget()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        if (server == null)
        {
            Console.WriteLine("WARN: Durango.Offline.Server not found for auto-connect cleanup");
            return;
        }
        int replaced = 0;
        foreach (MethodDef method in server.Methods)
        {
            if (method.Name != ".cctor" || !method.HasBody)
            {
                continue;
            }
            foreach (Instruction instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr && string.Equals(instr.Operand as string, "192.168.1.34", StringComparison.Ordinal))
                {
                    instr.Operand = string.Empty;
                    replaced++;
                }
            }
        }
        Console.WriteLine(replaced > 0
            ? "patched stale Durango.Offline.Server auto-connect target to empty"
            : "stale auto-connect target already clean");
    }

    // The retail DLL creates the Online Server (For Test) entry, but its
    // confirmation callback only starts the embedded island.  Route that one
    // entry through the same server-backed ConnectTo path used by the source.
    static void PatchOnlineServerMenuRoute()
    {
        TypeDef closure = module.Find("Durango.Offline.Server/<>c__DisplayClass21_0", false);
        MethodDef confirm = closure?.Methods.FirstOrDefault(m => m.Name.Contains("b__2") && m.HasBody);
        TypeDef serverType = module.Find("Durango.Offline.Server", false);
        MethodDef connectTo = serverType?.FindMethod("ConnectTo");
        TypeDef preferences = module.Find("Preferences", false);
        MethodDef getString = preferences?.Methods.FirstOrDefault(m => m.Name == "GetString" && m.Parameters.Count == 3);
        FieldDef keyField = closure?.FindField("key");
        if (confirm == null || connectTo == null || getString == null || keyField == null)
        {
            Console.WriteLine("WARN: Online Server menu patch targets not found");
            return;
        }
        if (confirm.Body.Instructions.Any(i => i.Operand is IMethod called && called.Name == "ConnectTo"))
        {
            Console.WriteLine("Online Server menu route already patched");
            return;
        }

        TypeRef strType = new TypeRefUser(module, "System", "String", module.CorLibTypes.AssemblyRef);
        MemberRef equals = new MemberRefUser(module, "op_Equality",
            MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String, module.CorLibTypes.String), strType);
        MemberRef isNullOrEmpty = new MemberRefUser(module, "IsNullOrEmpty",
            MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String), strType);
        // 🐛 [แก้เอง] 29 ส.ค. 2026 — เดิม default ตรงนี้เป็น "127.0.0.1" ตายตัว ⇒ ผู้เล่นใหม่ที่ไม่เคยกรอก IP
        // เองผ่านเมนู "เยี่ยมชมเกาะเพื่อน" (Preferences "last_connect_ip" ยังว่าง) กด "Dinoworld Server" แล้ว
        // ต่อเข้าเครื่องตัวเองเปล่า ๆ ทุกครั้ง ⇒ เห็นเป็น "Cannot connect to the game" ทั้งที่เซิร์ฟจริงปกติดี
        // ⇒ ให้อ่าน env DURANGO_AUTOCONNECT ก่อน (DurangoUpdater ตั้งให้จาก server.txt ทุกครั้งที่เปิดเกม)
        // แล้วค่อย fallback เป็น Preferences เดิม — เปลี่ยน IP เซิร์ฟทีหลังแก้แค่ server.txt ไม่ต้องแพตช์ DLL ใหม่
        if (_serverTarget == null)
        {
            Console.WriteLine("WARN: server.txt reader missing — ข้ามการแพตช์เมนู Online Server");
            return;
        }
        confirm.Body.InitLocals = true;
        Local ipLocal = new Local(module.CorLibTypes.String);
        confirm.Body.Variables.Add(ipLocal);
        Instruction ret = confirm.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
        int at = confirm.Body.Instructions.IndexOf(ret);
        Instruction skip = ret;
        Instruction connectLabel = OpCodes.Ldloc.ToInstruction(ipLocal);
        Instruction prefFallback = OpCodes.Ldstr.ToInstruction("last_connect_ip");
        var ins = new[]
        {
            OpCodes.Ldarg_0.ToInstruction(),
            OpCodes.Ldfld.ToInstruction(keyField),
            OpCodes.Ldstr.ToInstruction("online"),
            OpCodes.Call.ToInstruction(equals),
            OpCodes.Brfalse.ToInstruction(skip),
            // 1) อ่าน server.txt ตรง ๆ (fallback ในตัวไปที่ env DURANGO_AUTOCONNECT) — ถ้ามีค่า ใช้เลย
            OpCodes.Call.ToInstruction(_serverTarget),
            OpCodes.Stloc.ToInstruction(ipLocal),
            OpCodes.Ldloc.ToInstruction(ipLocal),
            OpCodes.Call.ToInstruction(isNullOrEmpty),
            OpCodes.Brfalse.ToInstruction(connectLabel),
            // 2) fallback: IP ที่ผู้เล่นเคยกรอกเองในเมนู "เยี่ยมชมเกาะเพื่อน"
            prefFallback,
            OpCodes.Ldstr.ToInstruction("127.0.0.1"),
            OpCodes.Ldc_I4_0.ToInstruction(),
            OpCodes.Call.ToInstruction(getString),
            OpCodes.Stloc.ToInstruction(ipLocal),
            OpCodes.Ldloc.ToInstruction(ipLocal),
            OpCodes.Call.ToInstruction(isNullOrEmpty),
            OpCodes.Brfalse.ToInstruction(connectLabel),
            OpCodes.Ldstr.ToInstruction("127.0.0.1"),
            OpCodes.Stloc.ToInstruction(ipLocal),
            connectLabel,
            OpCodes.Call.ToInstruction(connectTo),
        };
        for (int i = 0; i < ins.Length; i++)
        {
            confirm.Body.Instructions.Insert(at + i, ins[i]);
        }
        confirm.Body.SimplifyBranches();
        confirm.Body.OptimizeBranches();
        confirm.Body.KeepOldMaxStack = false;
        Console.WriteLine("patched Online Server (For Test) menu route to external gateway");
    }

    static void PatchOnlineServerDisplayName()
    {
        int replaced = 0;
        // [แก้เอง] 30 ส.ค. 2026 — DLL ต้นฉบับ (Mono retail) ใช้ iterator ชื่อ `<GetServers>c__Iterator0`
        // และมีรายการเซิร์ฟแค่ "Creative Island" (key=free) เท่านั้น ส่วนตัวที่คอมไพล์ใหม่จากซอร์ส client/
        // (Roslyn) ใช้ `<GetServers>d__0` และมี "Online Server (For Test)" เพิ่มมา
        // ⇒ รองรับทั้งสองแบบ เพื่อให้แพตช์ได้ทั้ง DLL ต้นฉบับและ DLL ที่คอมไพล์ใหม่
        TypeDef iterator = module.Find("Durango.Offline.Servers/<GetServers>d__0", false)
                        ?? module.Find("Durango.Offline.Servers/<GetServers>c__Iterator0", false);
        MethodDef moveNext = iterator?.FindMethod("MoveNext");
        if (moveNext?.HasBody == true)
        {
            foreach (Instruction instr in moveNext.Body.Instructions)
            {
                // [แก้เอง] 29 ส.ค. 2026 — เดิมใส่ NGUI BBCode สี "[C2185B]...[-]" เพื่อให้ชื่อเป็นสีชมพูเข้ม
                // แต่ label บางตัว **ไม่ได้เปิด supportEncoding** จึงโชว์แท็กดิบ ๆ ให้เห็น เช่นปุ่ม Back
                // มุมซ้ายบนของหน้า Select Character (เจ้าของแจ้ง: "แก้มุมซ้ายบน")
                // ⇒ ใช้ข้อความล้วน ชื่อจะแสดงถูกต้องทุกหน้าจอ (แลกกับการไม่มีสีที่หน้าเลือกเซิร์ฟ)
                string s = instr.Operand as string;
                if (instr.OpCode == OpCodes.Ldstr && s != null &&
                    (s == "Online Server (For Test)" ||
                     s == "[C2185B]Dinoworld Server[-]" ||
                     s == "Dinoworld Server" ||
                     s.TrimEnd() == "Creative Island"))   // ← ชื่อรายการเดียวที่ DLL ต้นฉบับมี
                {
                    instr.Operand = "DurangoTH CustomServer";
                    replaced++;
                }
            }
        }
        Console.WriteLine(replaced > 0
            ? "patched Online Server display name to plain 'DurangoTH CustomServer'"
            : "Online Server display name already patched or not found");
    }

    static void PatchForceMobileUI()
    {
        TypeDef platformPc = module.Find("Durango.System.Platform_PC", false);
        MethodDef getter = platformPc?.FindMethod("get_UsePCUI");
        if (getter?.HasBody != true)
        {
            Console.WriteLine("WARN: Platform_PC.get_UsePCUI not found");
            return;
        }

        getter.Body.Instructions.Clear();
        getter.Body.ExceptionHandlers.Clear();
        getter.Body.Variables.Clear();
        getter.Body.InitLocals = false;
        getter.Body.Instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
        getter.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        getter.Body.KeepOldMaxStack = false;
        Console.WriteLine("patched Platform_PC.UsePCUI to force Mobile UI");
    }

    static void PatchTitleUiToPc()
    {
        TypeDef prefabMap = module.Find("UIPrefabMap", false);
        MethodDef getTitle = prefabMap?.FindMethod("GetTitle");
        FieldDef titlePc = prefabMap?.FindField("_titlePC");
        if (getTitle?.HasBody != true || titlePc == null)
        {
            Console.WriteLine($"WARN: UIPrefabMap.GetTitle PC target not found (method={getTitle != null}, field={titlePc != null})");
            return;
        }

        getTitle.Body.Instructions.Clear();
        getTitle.Body.ExceptionHandlers.Clear();
        getTitle.Body.Variables.Clear();
        getTitle.Body.InitLocals = false;
        getTitle.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
        getTitle.Body.Instructions.Add(OpCodes.Ldfld.ToInstruction(titlePc));
        getTitle.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        getTitle.Body.KeepOldMaxStack = false;
        Console.WriteLine("patched title/Main UI to use PC buttons while gameplay stays Mobile UI");
    }

    static void PatchMobileClickToWalk()
    {
        TypeDef playerController = module.Find("PlayerController", false);
        MethodDef onAwake = playerController?.FindMethod("OnAwake");
        if (onAwake?.HasBody != true)
        {
            Console.WriteLine("WARN: PlayerController.OnAwake not found");
            return;
        }

        var instructions = onAwake.Body.Instructions;
        for (int i = 0; i < instructions.Count - 2; i++)
        {
            if (instructions[i].OpCode != OpCodes.Call ||
                instructions[i].Operand is not IMethod instanceCall ||
                instanceCall.Name != "get_Instance" ||
                instructions[i + 1].OpCode != OpCodes.Callvirt ||
                instructions[i + 1].Operand is not IMethod uiModeCall ||
                uiModeCall.Name != "get_UsePCUI" ||
                instructions[i + 2].OpCode.Code is not Code.Brfalse and not Code.Brfalse_S)
            {
                continue;
            }

            instructions.RemoveAt(i + 2);
            instructions.RemoveAt(i + 1);
            instructions.RemoveAt(i);
            onAwake.Body.SimplifyBranches();
            onAwake.Body.OptimizeBranches();
            onAwake.Body.KeepOldMaxStack = false;
            Console.WriteLine("enabled mobile click-to-walk handler");
            return;
        }

        bool alreadyEnabled = instructions.Count(i =>
            i.OpCode == OpCodes.Callvirt &&
            i.Operand is IMethod called &&
            called.Name == "On") >= 2;
        Console.WriteLine(alreadyEnabled
            ? "mobile click-to-walk handler already enabled"
            : "WARN: PlayerController.OnAwake mobile handler branch not found");
    }

    static void PatchDisableCraftLayout()
    {
        TypeDef craftScreen = module.Find("CraftScreen", false);
        MethodDef getter = craftScreen?.FindMethod("get_Enabled");
        if (getter?.HasBody != true)
        {
            Console.WriteLine("WARN: CraftScreen.get_Enabled not found");
            return;
        }

        getter.Body.Instructions.Clear();
        getter.Body.ExceptionHandlers.Clear();
        getter.Body.Variables.Clear();
        getter.Body.InitLocals = false;
        getter.Body.Instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
        getter.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
        getter.Body.KeepOldMaxStack = false;
        Console.WriteLine("disabled CraftScreen custom layout and hot-reload path");
    }

    static void PatchOnlineServerAccountLookup()
    {
        TypeDef server = module.Find("Durango.Offline.Server", false);
        TypeDef cluster = module.Find("Durango.Logic.Clusters.Cluster", false);
        MethodDef ctor = server?.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.HasBody);
        MethodDef getCluster = server?.FindMethod("get_Cluster");
        MethodDef setAccount = cluster?.Methods.FirstOrDefault(m => m.Name == "set_OnRequestAccount");
        FieldDef gateway = cluster?.Fields.FirstOrDefault(f => f.Name == "GatewayUrlRoot");
        TypeDef closure = module.Find("Durango.Offline.Server/<>c__DisplayClass21_0", false);
        FieldDef keyField = closure?.FindField("key");
        if (ctor == null || getCluster == null || setAccount == null || gateway == null || keyField == null)
        {
            Console.WriteLine($"WARN: Online Server account lookup patch targets not found (ctor={ctor != null}, get={getCluster != null}, set={setAccount != null}, gateway={gateway != null}, key={keyField != null})");
            return;
        }
        if (ctor.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldstr && string.Equals(i.Operand as string, "http://127.0.0.1:8190", StringComparison.Ordinal)))
        {
            Console.WriteLine("Online Server account lookup already patched");
            return;
        }
        TypeRef strType = new TypeRefUser(module, "System", "String", module.CorLibTypes.AssemblyRef);
        MemberRef equals = new MemberRefUser(module, "op_Equality",
            MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String, module.CorLibTypes.String), strType);
        Instruction skip = OpCodes.Nop.ToInstruction();
        Instruction setter = ctor.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Callvirt && i.Operand is IMethod called && called.Name == "set_OnRequestAccount");
        if (setter == null)
        {
            Console.WriteLine("WARN: Server constructor account setter not found");
            return;
        }
        // 🐛 [แก้เอง] 29 ส.ค. 2026 — เดิม GatewayUrlRoot ตรงนี้ถูก hardcode เป็น localhost ⇒ การดึงข้อมูล
        // บัญชี/ตัวละครของปุ่ม "Dinoworld Server" วิ่งไปเครื่องผู้เล่นเอง ไม่ใช่เซิร์ฟจริง (คู่กับบั๊กใน
        // PatchOnlineServerMenuRoute) ⇒ อ่าน env DURANGO_AUTOCONNECT ก่อน เหมือนกัน แล้ว fallback ค่าเดิม
        MemberRef isNullOrEmptyUrl = new MemberRefUser(module, "IsNullOrEmpty",
            MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.String), strType);
        if (_serverTarget == null)
        {
            Console.WriteLine("WARN: server.txt reader missing — ข้ามการแพตช์ account lookup");
            return;
        }
        ctor.Body.InitLocals = true;
        Local urlLocal = new Local(module.CorLibTypes.String);
        ctor.Body.Variables.Add(urlLocal);
        Instruction haveUrl = OpCodes.Ldarg_0.ToInstruction();
        int at = ctor.Body.Instructions.IndexOf(setter) + 1;
        var ins = new[]
        {
            OpCodes.Ldloc_0.ToInstruction(),
            OpCodes.Ldfld.ToInstruction(keyField),
            OpCodes.Ldstr.ToInstruction("online"),
            OpCodes.Call.ToInstruction(equals),
            OpCodes.Brfalse.ToInstruction(skip),
            OpCodes.Call.ToInstruction(_serverTarget),
            OpCodes.Stloc.ToInstruction(urlLocal),
            OpCodes.Ldloc.ToInstruction(urlLocal),
            OpCodes.Call.ToInstruction(isNullOrEmptyUrl),
            OpCodes.Brfalse.ToInstruction(haveUrl),
            OpCodes.Ldstr.ToInstruction("http://127.0.0.1:8190"),
            OpCodes.Stloc.ToInstruction(urlLocal),
            haveUrl,
            OpCodes.Call.ToInstruction(getCluster),
            OpCodes.Ldloc.ToInstruction(urlLocal),
            OpCodes.Stfld.ToInstruction(gateway),
            OpCodes.Ldarg_0.ToInstruction(),
            OpCodes.Call.ToInstruction(getCluster),
            OpCodes.Ldnull.ToInstruction(),
            OpCodes.Callvirt.ToInstruction(setAccount),
            skip,
        };
        for (int i = 0; i < ins.Length; i++)
        {
            ctor.Body.Instructions.Insert(at + i, ins[i]);
        }
        ctor.Body.SimplifyBranches();
        ctor.Body.OptimizeBranches();
        Console.WriteLine("patched Online Server account lookup to external gateway");
    }

    static void PatchForceConnectMenuVisible()
    {
        TypeDef menuSystem = module.Find("MenuSystem", false);
        MethodDef isHidden = menuSystem?.FindMethod("IsHiddenMenu");
        if (isHidden == null || !isHidden.HasBody || !isHidden.IsStatic)
        {
            Console.WriteLine("WARN: MenuSystem.IsHiddenMenu not found");
            return;
        }

        Instruction originalFirst = isHidden.Body.Instructions[0];
        var prologue = new[]
        {
            OpCodes.Ldarg_0.ToInstruction(),
            OpCodes.Ldc_I4.ToInstruction(31),
            OpCodes.Bne_Un.ToInstruction(originalFirst),
            OpCodes.Ldc_I4_0.ToInstruction(),
            OpCodes.Ret.ToInstruction(),
        };
        for (int i = prologue.Length - 1; i >= 0; i--)
        {
            isHidden.Body.Instructions.Insert(0, prologue[i]);
        }
        isHidden.Body.SimplifyBranches();
        isHidden.Body.OptimizeBranches();
        Console.WriteLine("patched MenuSystem.IsHiddenMenu: Connect is always visible");
    }

    static void DumpMethods(string typeName, string namePart)
    {
        TypeDef type = module.Find(typeName, false);
        if (type == null)
        {
            Console.WriteLine("WARN: type not found: " + typeName);
            return;
        }
        foreach (MethodDef method in type.Methods)
        {
            if (!method.HasBody || (namePart != "*" && !method.Name.String.Contains(namePart, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }
            Console.WriteLine("METHOD " + method.FullName);
            foreach (Instruction instr in method.Body.Instructions)
            {
                string operand = instr.Operand switch
                {
                    null => "",
                    IMethod m => m.FullName,
                    IField f => f.FullName,
                    _ => instr.Operand.ToString()
                };
                Console.WriteLine($"  {instr.Offset:X4}: {instr.OpCode.Name,-12} {operand}");
            }
        }
    }

    static void Main(string[] args)
    {
        string dllPath = args[0];
        // "--autoconnect-only": แพตช์แค่จุดเดียว (ต่อเซิร์ฟตาม env DURANGO_AUTOCONNECT อัตโนมัติ)
        // ไม่แตะอย่างอื่นเลย — ใช้ตอนต้องการดีบักว่า DLL ต้นฉบับล้วน ๆ (แค่เปลี่ยนเส้นทาง) มีปัญหาไหม
        bool minimal = args.Length > 1 && args[1] == "--autoconnect-only";

        module = ModuleDefMD.Load(dllPath);

        if (args.Length > 1 && args[1] == "--dump")
        {
            DumpMethods(args.Length > 2 ? args[2] : "Durango.Offline.Server", args.Length > 3 ? args[3] : "*");
            return;
        }

        if (args.Length > 1 && args[1] == "--connect-menu-only")
        {
            PatchForceConnectMenuVisible();
            module.Write(dllPath + ".connect-menu.dll");
            Console.WriteLine("done (connect-menu-only) -> " + dllPath + ".connect-menu.dll");
            return;
        }

        // [แก้เอง] 30 ส.ค. 2026 — "--server-only": แพตช์ **เฉพาะเรื่องเซิร์ฟของเรา** บน DLL ต้นฉบับแท้
        // (เจ้าของสั่ง: "มาทำตัวใหม่เริ่มจาก original ให้เล่น offline ได้ก่อน" แล้วค่อย "ปรับหน้า Main UI
        // ให้แสดง server เราก่อน")
        //
        // ทำแค่ 5 อย่าง ไม่แตะพอร์ต/AppData path/Mobile UI/craft/สัตว์/ซ่อนเมนู — ของพวกนั้นคือชุดแพตช์
        // เต็มที่เคยทำให้เข้าโลกไม่ได้ ต้องใส่กลับทีละตัวแล้วเทสออฟไลน์ทุกครั้ง
        if (args.Length > 1 && args[1] == "--server-only")
        {
            PatchInjectModLoader();                         // ⭐ hook เดียวที่แตะไฟล์เกม — ที่เหลือเขียนเป็น mod

            // [แก้เอง] 30 ส.ค. 2026 — **เคยใส่ PatchConstField ย้ายพอร์ตตรงนี้ แล้วถอนออก**
            // เหตุผล: DefaultPort เป็น `const` ⇒ C# inline ค่าลงทุก call site ตอนคอมไพล์
            // แก้ค่า field ทีหลังจึงไม่มีผลเลย (เทสแล้ว SocketException ยังขึ้นเหมือนเดิม)
            // ทางที่ได้ผลจริงคือ Harmony prefix ที่ Durango.Offline.Listener::Start(int)
            // ซึ่ง "รับพอร์ตเป็น argument" ⇒ เปลี่ยนค่าตอน runtime ได้ — ทำใน mod DurangoOnlineMode แทน
            _serverTarget = AddServerTargetHelper();        // อ่าน server.txt ตรง ๆ
            PatchDisableAutoConnectTarget();                // ไม่ auto-connect ที่หน้าไตเติ้ล (เห็นเมนู)
            PatchOnlineServerDisplayName();                 // ชื่อ "DurangoTH CustomServer"
            PatchOnlineServerMenuRoute();                   // กดปุ่มแล้วต่อเซิร์ฟจาก server.txt
            PatchOnlineServerAccountLookup();               // ดึงบัญชี/ตัวละครจาก gateway ของเรา
            module.Write(dllPath + ".server-only.dll");
            Console.WriteLine("done (server-only) -> " + dllPath + ".server-only.dll");
            return;
        }

        if (minimal)
        {
            // PatchAutoConnect() แทรกที่ Server.BeginServer เฉย ๆ ไม่พอ — BeginServer ถูกเรียกแค่ตอน
            // เดินทางข้ามเกาะ (หลังเข้าเกมแล้ว) ไม่ใช่ตอน title screen เลย จึงต้อง patch เพิ่มอีก 2 จุด
            // ที่ title-screen flow ใช้จริง (ยืนยันจาก client/ source ก่อนแล้ว ไม่ได้เดา):
            //   Clusters.RequestAccounts(gatewayUrl,...)  — เรียกตอนเช็ค account (ปุ่ม "เริ่ม" ต้องรอสิ่งนี้)
            //   GameManager.SetCluster(key, url, mode)    — url ตรงนี้ถูกใช้ต่อใน /knock ทุกครั้ง
            PatchAutoConnect();
            PatchForceGatewayUrl("Durango.Logic.Clusters.Clusters", "RequestAccounts", 0);
            PatchForceGatewayUrl("GameManager", "SetCluster", 1);
            module.Write(dllPath + ".autoconnect.dll");
            Console.WriteLine("done (autoconnect-only) -> " + dllPath + ".autoconnect.dll");
            return;
        }

        PatchConstField("Durango.Offline.GameServer", "DefaultPort", 8391);
        PatchConstField("Durango.Offline.Gateway", "DefaultPort", 8390);
        ForceLocalAssetBundles();
        PatchAppDataBasePath();
        GuardTitleWidget();
        PatchSelfIpFilter();
        _serverTarget = AddServerTargetHelper();   // ต้องสร้างก่อน 2 แพตช์ที่เรียกใช้ด้านล่าง
        PatchDisableAutoConnectTarget();           // กันหน้าไตเติ้ลกระโดดข้าม Main UI
        // [แก้เอง] 29 ส.ค. 2026 — **ปิด PatchAutoConnect() โดยตั้งใจ** (เจ้าของสั่ง: "จะเอาแบบเดิม
        // กดปุ่มค่อยเชื่อมเซิร์ฟ") — แพตช์นี้ยิง ConnectTo ตั้งแต่ BeginServer ตอนเปิดเกม ทำให้หน้าไตเติ้ล
        // ข้ามเมนู "Select Server" ไปเป็น "Select Character" ทันที ผู้เล่นเลือกโหมดเองไม่ได้
        // การต่อเซิร์ฟจริงย้ายไปอยู่ที่ PatchOnlineServerMenuRoute แทน (ทำงานตอนกดปุ่ม "Dinoworld Server"
        // → อ่าน env DURANGO_AUTOCONNECT ที่ DurangoUpdater ตั้งจาก server.txt) ⇒ ได้ทั้งเมนูเดิมและ IP ที่ถูก
        // ถ้าจะเปิดกลับ ให้เอาคอมเมนต์บรรทัดล่างออก
        // PatchAutoConnect();
        PatchServerAnimalSpawn();
        PatchHideUnimplementedMenus();
        PatchForceSetClustersAccountRefresh();
        PatchStaleAutoConnectTarget();
        PatchOnlineServerMenuRoute();
        PatchOnlineServerDisplayName();
        PatchForceMobileUI();
        PatchTitleUiToPc();
        PatchMobileClickToWalk();
        PatchDisableCraftLayout();
        PatchOnlineServerAccountLookup();
        // [แก้เอง] 29 ส.ค. 2026 — **ปิดไว้ก่อน** เพราะต้องสงสัยว่าทำให้ "ค้างหน้าเข้าเกาะ"
        // SelectPersonalRegion.Show() มี gameObject.SetActive(true) ⇒ object เริ่มมาแบบ inactive
        // ⇒ ถ้าข้ามหน้านี้ Awake() (ที่เป็นคนสุ่มตั้ง SelectedRegionid) จะไม่เคยทำงาน
        // ⇒ ตัวละครถูกสร้างโดยไม่มี region ⇒ client เข้าเกาะไม่ได้
        // PatchSkipRegionSelect();

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
