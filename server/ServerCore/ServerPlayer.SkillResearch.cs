using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Skill;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    private readonly HashSet<string> _completedCategoryResearch = new HashSet<string>(StringComparer.Ordinal);
    private Category _researchCategory = Category.Invalid;
    private int _researchTargetLevel;
    private double _researchStartedAt;
    private double _researchEndsAt;

    private static string ResearchKey(Category category, int targetLevel) => $"{(int)category}:{targetLevel}";

    private static int ResearchSeconds(Category category, int targetLevel)
    {
        if (category == Category.Survival) return 0;
        return targetLevel switch
        {
            20 => 300,
            25 => 1200,
            30 => 3600,
            35 => 7200,
            40 => 14400,
            45 => 43200,
            50 => 86400,
            55 => 172800,
            59 => 259200,
            _ => 0
        };
    }

    private void ResolveProficiency(Category category, int totalExp, out int level, out int expInLevel)
    {
        SkillCategoryData.Resolve(category, totalExp, CategoryLevelCap, out int rawLevel, out int rawExp);
        level = rawLevel;
        expInLevel = rawExp;
        if (!SkillCategoryData.TryGet(category, out SkillCategoryData.Curve curve)) return;

        int[] gates = { 20, 25, 30, 35, 40, 45, 50, 55, 59 };
        for (int i = 0; i < gates.Length; i++)
        {
            int target = gates[i];
            if (rawLevel < target || ResearchSeconds(category, target) <= 0
                || _completedCategoryResearch.Contains(ResearchKey(category, target))) continue;
            level = target - 1;
            expInLevel = curve.NeededAt(level);
            return;
        }
    }

    private bool IsReadyForResearch(Category category, out int targetLevel, out int seconds)
    {
        _categoryExp.TryGetValue(category, out int total);
        ResolveProficiency(category, total, out int level, out int exp);
        targetLevel = level + 1;
        seconds = ResearchSeconds(category, targetLevel);
        if (seconds <= 0 || Level <= level || !SkillCategoryData.TryGet(category, out SkillCategoryData.Curve curve)) return false;
        return exp >= curve.NeededAt(level) && !_completedCategoryResearch.Contains(ResearchKey(category, targetLevel));
    }

    private void HandleResearchSkillCategory(ResearchSkillCategory msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Skills || !SkillCategoryData.TryGet(msg.Category, out SkillCategoryData.Curve _))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (_researchCategory != Category.Invalid)
        {
            if (!msg.SkipCategory.HasValue || msg.SkipCategory.Value != _researchCategory)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            CompleteSkillResearch();
        }
        if (!IsReadyForResearch(msg.Category, out int target, out int seconds))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        _researchCategory = msg.Category;
        _researchTargetLevel = target;
        _researchStartedAt = Times.UnixTimeNow();
        _researchEndsAt = _researchStartedAt + seconds;
        MarkDirty();
        Send(default(OK), header.Seq);
        SendSkills();
    }

    private void HandleCancelSkillCategoryResearch(CancelSkillCategoryResearch msg, PacketHeader header)
    {
        if (_researchCategory == Category.Invalid || msg.SkillCategory != _researchCategory)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        ClearSkillResearch();
        MarkDirty();
        Send(default(OK), header.Seq);
        SendSkills();
    }

    private void HandleSkipSkillCategoryResearch(SkipSkillCategoryResearch msg, PacketHeader header)
    {
        if (_researchCategory == Category.Invalid || msg.SkillCategory != _researchCategory)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        CompleteSkillResearch();
        Send(default(OK), header.Seq);
    }

    private void ProcessSkillResearch(double now)
    {
        if (_researchCategory != Category.Invalid && _researchEndsAt > 0 && now >= _researchEndsAt)
        {
            CompleteSkillResearch();
        }
    }

    private void CompleteSkillResearch()
    {
        if (_researchCategory == Category.Invalid) return;
        Category completed = _researchCategory;
        int target = _researchTargetLevel;
        _completedCategoryResearch.Add(ResearchKey(completed, target));
        ClearSkillResearch();
        MarkDirty();
        Send(new Info { Text = $"วิจัยความชำนาญ {ProficiencyNameOf(completed)} เลเวล {target} สำเร็จ" });
        SendSkills();
        RefreshAbilities();
    }

    private void ClearSkillResearch()
    {
        _researchCategory = Category.Invalid;
        _researchTargetLevel = 0;
        _researchStartedAt = 0;
        _researchEndsAt = 0;
    }

    private SkillCategoryResearchTime? ResearchTimeFor(Category category, int level)
    {
        int seconds = ResearchSeconds(category, level + 1);
        if (seconds <= 0 || _completedCategoryResearch.Contains(ResearchKey(category, level + 1))) return null;
        return new SkillCategoryResearchTime
        {
            DefaultNeededTime = seconds,
            ReduceStatusEffects = null,
            ReduceRate = 0,
            ReduceUntil = 0
        };
    }

    private SkillCategoryResearching? ResearchingFor(Category category)
    {
        if (category != _researchCategory) return null;
        return new SkillCategoryResearching
        {
            StartedAt = _researchStartedAt,
            EndsAt = _researchEndsAt,
            SavedTime = 0,
            SkipCost = null
        };
    }

    private void ApplySkillResearchSave(PlayerSave save)
    {
        _completedCategoryResearch.Clear();
        if (save.CompletedCategoryResearch != null)
        {
            foreach (string key in save.CompletedCategoryResearch)
                if (!string.IsNullOrWhiteSpace(key)) _completedCategoryResearch.Add(key);
        }
        Category category = (Category)save.ResearchCategory;
        if (SkillCategoryData.TryGet(category, out SkillCategoryData.Curve _) && save.ResearchTargetLevel > 0 && save.ResearchEndsAt > 0)
        {
            _researchCategory = category;
            _researchTargetLevel = save.ResearchTargetLevel;
            _researchStartedAt = save.ResearchStartedAt;
            _researchEndsAt = save.ResearchEndsAt;
        }
        else ClearSkillResearch();
    }

    private void FillSkillResearchSave(PlayerSave save)
    {
        save.CompletedCategoryResearch = new List<string>(_completedCategoryResearch);
        save.ResearchCategory = (int)_researchCategory;
        save.ResearchTargetLevel = _researchTargetLevel;
        save.ResearchStartedAt = _researchStartedAt;
        save.ResearchEndsAt = _researchEndsAt;
    }
}
