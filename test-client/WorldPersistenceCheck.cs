using System;
using System.Collections.Generic;
using System.IO;
using DurangoServer.Core;
using Shared.Item;
using Shared.Region;

namespace DurangoTestClient;

/// <summary>
/// S1: ตรวจว่า WorldSave roundtrip สำหรับ artifact materials, boxes และ artifacts ถูกต้อง
/// (ไม่ต้องเปิด server — สร้าง WorldSave จำลอง → เขียน → อ่าน → เปรียบเทียบ)
/// </summary>
public static class WorldPersistenceCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    public static int Run()
    {
        Console.WriteLine("=== world persistence check (S1) ===");
        string root = Path.Combine(Path.GetTempPath(), "durango-world-persist-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string worldPath = Path.Combine(root, "world.json");

            // --- round 1: artifact materials (reserved construction ledger) ---
            var save1 = new WorldSave
            {
                TerrainId = "ri35te",
                ArtifactMaterials = new Dictionary<string, Dictionary<string, List<ItemSave>>>
                {
                    ["artifact-bonfire-01"] = new Dictionary<string, List<ItemSave>>
                    {
                        ["main"] = new List<ItemSave>
                        {
                            new ItemSave { Id = "leaf-1", Prototype = "leaf", Name = "ใบไม้" },
                            new ItemSave { Id = "leaf-2", Prototype = "leaf", Name = "ใบไม้" },
                            new ItemSave { Id = "leaf-3", Prototype = "leaf", Name = "ใบไม้" },
                            new ItemSave { Id = "leaf-4", Prototype = "leaf", Name = "ใบไม้" },
                        }
                    }
                },
                Boxes = new Dictionary<string, List<ItemSave>>(),
                Artifacts = new List<ArtifactSave>
                {
                    new ArtifactSave
                    {
                        EntityId = "artifact-bonfire-01",
                        EntityType = 9001,
                        BlueprintId = "bonfire",
                        TileX = 42, TileY = 177,
                        BuildingState = 0, // Occupied
                        FounderEntityId = "persist-owner"
                    }
                }
            };

            Check("save artifact materials", SaveStore.Save(worldPath, save1));
            WorldSave loaded1 = SaveStore.Load<WorldSave>(worldPath);
            Check("load artifact materials", loaded1 != null);
            bool ledgerOk = loaded1 != null
                && loaded1.ArtifactMaterials.ContainsKey("artifact-bonfire-01")
                && loaded1.ArtifactMaterials["artifact-bonfire-01"].ContainsKey("main")
                && loaded1.ArtifactMaterials["artifact-bonfire-01"]["main"].Count == 4
                && loaded1.ArtifactMaterials["artifact-bonfire-01"]["main"][0].Prototype == "leaf";
            Check("artifact materials roundtrip preserves slot and items", ledgerOk);

            bool artifactOk = loaded1 != null
                && loaded1.Artifacts.Count == 1
                && loaded1.Artifacts[0].EntityId == "artifact-bonfire-01"
                && loaded1.Artifacts[0].BlueprintId == "bonfire"
                && loaded1.Artifacts[0].BuildingState == 0
                && loaded1.Artifacts[0].FounderEntityId == "persist-owner";
            Check("artifact state roundtrip preserves blueprint/state/owner", artifactOk);

            // --- round 2: box contents (storage) ---
            var save2 = new WorldSave
            {
                TerrainId = "ri35te",
                ArtifactMaterials = new Dictionary<string, Dictionary<string, List<ItemSave>>>(),
                Boxes = new Dictionary<string, List<ItemSave>>
                {
                    ["artifact-box-01"] = new List<ItemSave>
                    {
                        new ItemSave { Id = "stone-1", Prototype = "stone", Name = "หิน" },
                        new ItemSave { Id = "leaf-10", Prototype = "leaf", Name = "ใบไม้" },
                    }
                },
                Artifacts = new List<ArtifactSave>
                {
                    new ArtifactSave
                    {
                        EntityId = "artifact-box-01",
                        EntityType = 6171,
                        BlueprintId = "fur_box_03_leaf",
                        TileX = 43, TileY = 177,
                        BuildingState = 2, // Built
                        FounderEntityId = "persist-owner"
                    }
                }
            };

            File.Delete(worldPath);
            Check("save box contents", SaveStore.Save(worldPath, save2));
            WorldSave loaded2 = SaveStore.Load<WorldSave>(worldPath);
            Check("load box contents", loaded2 != null);
            bool boxOk = loaded2 != null
                && loaded2.Boxes.ContainsKey("artifact-box-01")
                && loaded2.Boxes["artifact-box-01"].Count == 2
                && loaded2.Boxes["artifact-box-01"][0].Prototype == "stone"
                && loaded2.Boxes["artifact-box-01"][1].Prototype == "leaf";
            Check("box contents roundtrip preserves item list", boxOk);

            // --- round 3: empty materials + empty boxes (no leak) ---
            var save3 = new WorldSave { TerrainId = "ri35te" };
            File.Delete(worldPath);
            Check("save empty world", SaveStore.Save(worldPath, save3));
            WorldSave loaded3 = SaveStore.Load<WorldSave>(worldPath);
            bool emptyOk = loaded3 != null
                && (loaded3.ArtifactMaterials == null || loaded3.ArtifactMaterials.Count == 0)
                && (loaded3.Boxes == null || loaded3.Boxes.Count == 0)
                && (loaded3.Artifacts == null || loaded3.Artifacts.Count == 0);
            Check("empty world loads cleanly", emptyOk);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }

        Console.WriteLine($"=== world persistence result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
