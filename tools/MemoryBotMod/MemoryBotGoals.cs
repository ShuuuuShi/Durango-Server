using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DurangoMemoryBot
{
    internal enum BotGoalKind
    {
        Daily,
        Craft,
        Gather,
        Hunt,
        Level,
        Skill,
        Build
    }

    internal sealed class BotGoal
    {
        public BotGoalKind Kind;
        public string Target;
        public int Count;
        public string Label;
        public string Source;
        public float StartedAt;
    }

    /// <summary>
    /// กองเป้าหมาย: ล่างสุด = งานหลัก · บนสุด = สิ่งที่ต้องทำก่อน (เป้าสำรองหรือเป้าพิเศษ)
    /// </summary>
    internal static class MemoryBotGoals
    {
        private static readonly List<BotGoal> Stack = new List<BotGoal>();

        public static bool HasAny { get { return Stack.Count > 0; } }
        public static int Count { get { return Stack.Count; } }

        public static BotGoal Peek()
        {
            return Stack.Count == 0 ? null : Stack[Stack.Count - 1];
        }

        public static BotGoal PeekUnder()
        {
            return Stack.Count < 2 ? null : Stack[Stack.Count - 2];
        }

        public static void Clear()
        {
            Stack.Clear();
        }

        public static void EnsureDailyMain()
        {
            if (Stack.Count == 0 || Stack[0].Kind != BotGoalKind.Daily)
                Stack.Insert(0, Make(BotGoalKind.Daily, "all", 0, "ไล่เควสรายวันให้ครบ", "main"));
        }

        public static bool Need(BotGoalKind kind, string target, int count, string label, string source)
        {
            if (target == null) target = "";
            for (int i = 0; i < Stack.Count; i++)
            {
                BotGoal g = Stack[i];
                if (g.Kind == kind && string.Equals(g.Target, target, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            Stack.Add(Make(kind, target, count, label, source));
            return true;
        }

        public static void PushSpecial(BotGoalKind kind, string target, int count, string label)
        {
            EnsureDailyMain();
            Need(kind, target, count, label, "special");
        }

        public static void CompleteCurrent()
        {
            if (Stack.Count == 0) return;
            BotGoal done = Stack[Stack.Count - 1];
            Stack.RemoveAt(Stack.Count - 1);
            if (done.Kind != BotGoalKind.Daily && Stack.Count == 0) EnsureDailyMain();
        }

        public static string Describe()
        {
            if (Stack.Count == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Stack.Count; i++)
            {
                if (i > 0) sb.Append(" > ");
                sb.Append(Stack[i].Label ?? KindName(Stack[i].Kind));
            }
            return sb.ToString();
        }

        public static string ToJson()
        {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < Stack.Count; i++)
            {
                if (i > 0) sb.Append(',');
                BotGoal g = Stack[i];
                sb.Append("{\"kind\":").Append(MemoryBotProtocol.Quote(KindName(g.Kind)))
                    .Append(",\"target\":").Append(MemoryBotProtocol.Quote(g.Target ?? ""))
                    .Append(",\"count\":").Append(g.Count)
                    .Append(",\"source\":").Append(MemoryBotProtocol.Quote(g.Source ?? ""))
                    .Append(",\"label\":").Append(MemoryBotProtocol.Quote(g.Label ?? ""))
                    .Append('}');
            }
            return sb.Append(']').ToString();
        }

        public static bool TryParseKind(string text, out BotGoalKind kind)
        {
            kind = BotGoalKind.Daily;
            if (string.IsNullOrEmpty(text)) return false;
            string t = text.Trim().ToLowerInvariant();
            if (t == "daily" || t == "quest") { kind = BotGoalKind.Daily; return true; }
            if (t == "craft" || t == "make") { kind = BotGoalKind.Craft; return true; }
            if (t == "gather" || t == "collect") { kind = BotGoalKind.Gather; return true; }
            if (t == "hunt" || t == "survival" || t == "combat") { kind = BotGoalKind.Hunt; return true; }
            if (t == "level" || t == "exp") { kind = BotGoalKind.Level; return true; }
            if (t == "skill" || t == "skills") { kind = BotGoalKind.Skill; return true; }
            if (t == "build" || t == "construct") { kind = BotGoalKind.Build; return true; }
            return false;
        }

        public static string KindName(BotGoalKind kind)
        {
            switch (kind)
            {
                case BotGoalKind.Craft: return "craft";
                case BotGoalKind.Gather: return "gather";
                case BotGoalKind.Hunt: return "hunt";
                case BotGoalKind.Level: return "level";
                case BotGoalKind.Skill: return "skill";
                case BotGoalKind.Build: return "build";
                default: return "daily";
            }
        }

        private static BotGoal Make(BotGoalKind kind, string target, int count, string label, string source)
        {
            return new BotGoal
            {
                Kind = kind,
                Target = target ?? "",
                Count = count,
                Label = string.IsNullOrEmpty(label) ? KindName(kind) : label,
                Source = source ?? "sub",
                StartedAt = Time.time
            };
        }
    }
}
