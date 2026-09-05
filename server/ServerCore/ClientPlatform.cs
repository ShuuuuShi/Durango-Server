using System;

namespace DurangoServer.Core;

/// <summary>
/// [3 ก.ย. 2026] ตัวช่วยเรื่อง "client คนนี้เป็นเครื่องแบบไหน" — ใช้ให้เซิร์ฟปรับสิ่งที่ส่งออกไปให้
/// เข้ากับ client ที่ไม่มีแพตช์ของเรา (เกมมือถือของแท้ 5.2.1 / PC รุ่นเก่า) โดยไม่ต้องแตะ APK
///
/// ที่มาของข้อมูลแต่ละอย่าง (ทั้งหมดเป็นของที่เกมต้นฉบับส่งมาอยู่แล้ว ไม่ต้องแพตช์ client):
///   · platform  — `/sessions` (ฟิลด์ `platform` จาก Platform.BuildSessionForm) และ `/entry?platform=`
///                 ค่าเป็น AssetBundlePlatform: "Android" / "WindowsPlayer" / "iOS"
///   · version   — packet Auth.ClientVersion (มือถือแท้ = "5.2.1" · PC ชุดเรา = "CustomClient 0.1.4")
///   · build     — query `build=` ที่ APK ชุดเราแปะเพิ่ม (แพตช์ literal "&amp;platform=" ใน global-metadata.dat)
///                 ⇒ เกมมือถือของแท้ล้วน ๆ จะไม่มีค่านี้
/// </summary>
public static class ClientPlatform
{
    /// <summary>คำนำหน้าข้อความบรอดแคสต์แบบกำหนดเวลา/ขนาด/สี — client รู้จักที่ GameManager.ShowAdminBroadcast</summary>
    public const string StyledBroadcastPrefix = "##bc|";

    public static bool IsAndroid(string platform)
    {
        return !string.IsNullOrEmpty(platform) && platform.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// client ชุดเราที่มี CustomClient เท่านั้นที่รู้จัก "##bc|" — ตัวอื่นเห็นเป็นข้อความดิบ
    /// เทียบเวอร์ชันเต็ม (MAJOR.MINOR.PATCH) กับ <paramref name="minVersion"/> เช่น "0.1.4"
    /// </summary>
    public static bool SupportsStyledBroadcast(string clientVersion, string minVersion)
    {
        if (string.IsNullOrWhiteSpace(clientVersion)) return false;
        if (clientVersion.IndexOf("CustomClient", StringComparison.OrdinalIgnoreCase) < 0) return false;
        if (!TryParseVersion(clientVersion, out int maj, out int min, out int patch)) return false;
        if (!TryParseVersion(minVersion ?? "", out int rMaj, out int rMin, out int rPatch)) return true;
        if (maj != rMaj) return maj > rMaj;
        if (min != rMin) return min > rMin;
        return patch >= rPatch;
    }

    /// <summary>
    /// "##bc|d=5|z=2|c=FF3333|ข้อความ" → "ข้อความ" (ข้อความที่ไม่ได้ขึ้นต้นด้วย ##bc| คืนค่าเดิม)
    /// กติกาเดียวกับ client: ไล่ตัดฟิลด์ "x=..." ทีละช่องจนเจอส่วนที่ไม่ใช่ฟิลด์
    /// </summary>
    public static string PlainBroadcastText(string payload)
    {
        if (payload == null || !payload.StartsWith(StyledBroadcastPrefix, StringComparison.Ordinal))
        {
            return payload;
        }
        string text = payload.Substring(StyledBroadcastPrefix.Length);
        int bar = text.IndexOf('|');
        while (bar >= 0)
        {
            string token = text.Substring(0, bar);
            if (token.IndexOf('=') != 1) break;
            text = text.Substring(bar + 1);
            bar = text.IndexOf('|');
        }
        return text;
    }

    /// <summary>ดึงเลข MAJOR.MINOR[.PATCH] ตัวแรกที่เจอในสตริง เช่น "android-0.1.4" → (0,1,4) · "5.2.1" → (5,2,1)</summary>
    public static bool TryParseVersion(string s, out int major, out int minor, out int patch)
    {
        major = minor = patch = -1;
        if (string.IsNullOrWhiteSpace(s)) return false;
        System.Text.RegularExpressions.Match m =
            System.Text.RegularExpressions.Regex.Match(s, @"(\d+)\.(\d+)(?:\.(\d+))?");
        if (!m.Success) return false;
        if (!int.TryParse(m.Groups[1].Value, out major) || !int.TryParse(m.Groups[2].Value, out minor)) return false;
        patch = m.Groups[3].Success && int.TryParse(m.Groups[3].Value, out int p) ? p : 0;
        return true;
    }

    /// <summary>
    /// นโยบายเวอร์ชันเดียวกับ PC (ClientModPolicy.IsClientVersionAllowed): เทียบแค่ MAJOR.MINOR
    /// ตัวท้าย = hotfix ไม่เช็ค · required ว่าง = ไม่บังคับ
    /// </summary>
    public static bool IsBuildAllowed(string requiredBuild, string build)
    {
        if (string.IsNullOrWhiteSpace(requiredBuild)) return true;
        if (!TryParseVersion(requiredBuild, out int rMaj, out int rMin, out _)) return true;
        if (!TryParseVersion(build ?? "", out int maj, out int min, out _)) return false;
        return rMaj == maj && rMin == min;
    }
}
