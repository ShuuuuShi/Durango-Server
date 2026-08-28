using System;
using System.IO;
using DurangoServer.Core;

namespace DurangoTestClient;

/// <summary>ตรวจ save schema, quarantine และ recovery โดยไม่ต้องเปิด network server</summary>
public static class SaveStoreCheck
{
    private sealed class TestSave : SaveEnvelope
    {
        public string Name { get; set; }
    }

    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok)
        {
            _passed++;
            Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}");
        }
    }

    public static int Run()
    {
        Console.WriteLine("=== save store check ===");
        string root = Path.Combine(Path.GetTempPath(), "durango-save-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string normal = Path.Combine(root, "normal.json");
            Check("เขียน current schema", SaveStore.Save(normal, new TestSave { Name = "current" }));
            TestSave current = SaveStore.Load<TestSave>(normal);
            Check("อ่าน current schema", current != null && current.Name == "current" && current.Version == SaveEnvelope.CurrentVersion,
                current == null ? "null" : "v" + current.Version);

            string legacy = Path.Combine(root, "legacy.json");
            File.WriteAllText(legacy, "{\"Version\":1,\"Name\":\"legacy\"}");
            TestSave migrated = SaveStore.Load<TestSave>(legacy);
            Check("อ่าน legacy schema", migrated != null && migrated.Name == "legacy" && migrated.Version == SaveEnvelope.CurrentVersion,
                migrated == null ? "null" : "v" + migrated.Version);
            Check("เขียน legacy ที่ migrate แล้ว", SaveStore.Save(legacy, migrated));
            string legacyJson = File.ReadAllText(legacy);
            Check("legacy ถูก persist เป็น current version", legacyJson.Contains("\"Version\": " + SaveEnvelope.CurrentVersion));

            string legacyZero = Path.Combine(root, "legacy-zero.json");
            File.WriteAllText(legacyZero, "{\"Name\":\"legacy-zero\"}");
            TestSave migratedZero = SaveStore.Load<TestSave>(legacyZero);
            Check("อ่าน v0 legacy schema", migratedZero != null && migratedZero.Name == "legacy-zero" && migratedZero.Version == SaveEnvelope.CurrentVersion);
            Check("เขียน v0 legacy ที่ migrate แล้ว", SaveStore.Save(legacyZero, migratedZero));

            string negative = Path.Combine(root, "negative.json");
            File.WriteAllText(negative, "{\"Version\":-1,\"Name\":\"negative\"}");
            Check("ปฏิเสธ negative schema", SaveStore.Load<TestSave>(negative) == null && !File.Exists(negative) && HasRejected(root, "negative.json"));

            string primary = Path.Combine(root, "primary.json");
            File.WriteAllText(primary, "{\"Version\":2,\"Name\":\"primary\"}");
            File.WriteAllText(primary + ".tmp", "{\"Version\":2,\"Name\":\"stale-temp\"}");
            TestSave primaryLoaded = SaveStore.Load<TestSave>(primary);
            Check("primary ชนะ stale temp", primaryLoaded != null && primaryLoaded.Name == "primary" && File.Exists(primary + ".tmp"));

            string corruptPrimary = Path.Combine(root, "corrupt-primary.json");
            File.WriteAllText(corruptPrimary, "broken");
            File.WriteAllText(corruptPrimary + ".tmp", "{\"Version\":2,\"Name\":\"manual-recovery\"}");
            Check("primary เสียไม่ promote temp โดยพลการ", SaveStore.Load<TestSave>(corruptPrimary) == null
                && !File.Exists(corruptPrimary) && File.Exists(corruptPrimary + ".tmp") && HasRejected(root, "corrupt-primary.json"));

            string future = Path.Combine(root, "future.json");
            File.WriteAllText(future, "{\"Version\":" + (SaveEnvelope.CurrentVersion + 1) + ",\"Name\":\"future\"}");
            Check("ปฏิเสธ future schema", SaveStore.Load<TestSave>(future) == null && !File.Exists(future) && HasRejected(root, "future.json"));

            string malformed = Path.Combine(root, "malformed.json");
            File.WriteAllText(malformed, "not json");
            Check("กักกัน malformed save", SaveStore.Load<TestSave>(malformed) == null && !File.Exists(malformed) && HasRejected(root, "malformed.json"));

            string recover = Path.Combine(root, "recover.json");
            File.WriteAllText(recover + ".tmp", "{\"Version\":1,\"Name\":\"recovered\"}");
            TestSave recovered = SaveStore.Load<TestSave>(recover);
            Check("กู้ valid temp save", recovered != null && recovered.Name == "recovered" && File.Exists(recover) && !File.Exists(recover + ".tmp"));

            string badTemp = Path.Combine(root, "bad-temp.json");
            File.WriteAllText(badTemp + ".tmp", "broken");
            Check("กักกัน invalid temp save", SaveStore.Load<TestSave>(badTemp) == null && !File.Exists(badTemp + ".tmp") && HasRejected(root, "bad-temp.json.tmp"));
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch (Exception e) { Console.WriteLine("[warn] ลบ temp ไม่สำเร็จ: " + e.Message); }
        }

        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    private static bool HasRejected(string root, string fileName)
    {
        return Directory.GetFiles(root, fileName + ".rejected-*").Length > 0;
    }
}
