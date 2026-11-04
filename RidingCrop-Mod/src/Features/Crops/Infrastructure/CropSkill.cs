using System;
using Jotunn.Configs;
using Jotunn.Managers;
using KydlCore.Common;
using UnityEngine;

namespace RidingCrop;

/// <summary>The RidingCrop skill that all crops train (instead of the Clubs skill).</summary>
internal static class CropSkill
{
    // Unique skill id. DO NOT CHANGE after release: it generates the skill's number, and players would lose their progress.
    private const string SkillIdentifier = "com.kydldev.ridingcrop.ridingcrop";

    // Skill name shown in the Skills window. Jotunn's "raiseskill" / "resetskill" console patch matches this name
    // case-insensitively, so the dev command is:  raiseskill ridingcrop 50
    // Changing the NAME is safe: saved progress is tied to SkillIdentifier above, not to this text.
    private const string SkillName = "RidingCrop";

    /// <summary>The registered skill type, or None when registering failed (the crops then use the Clubs skill).</summary>
    internal static Skills.SkillType Type { get; private set; } = Skills.SkillType.None;

    /// <summary>The skill's config (its icon is filled in once the first crop icon is rendered).</summary>
    internal static SkillConfig Config { get; private set; }

    /// <summary>Registers the skill. Must run before the game creates its Skills object (Plugin.Awake).</summary>
    internal static void Register()
    {
        try
        {
            Config = new SkillConfig
            {
                Identifier = SkillIdentifier,
                Name = SkillName,
                Description = "Skill with the riding crop. The higher it is, the harder the crop hits.",
                IncreaseStep = 1f,
            };
            Type = SkillManager.Instance.AddSkill(Config);
            ModLog.Verbose("CropSkill: skill '" + SkillName + "' registered (id " + (int)Type + ")");
        }
        catch (Exception ex)
        {
            ModLog.Error("CropSkill: could not register the '" + SkillName + "' skill; the crops will use the Clubs skill. " + ex.Message);
            Config = null;
            Type = Skills.SkillType.None;
        }
    }
}
