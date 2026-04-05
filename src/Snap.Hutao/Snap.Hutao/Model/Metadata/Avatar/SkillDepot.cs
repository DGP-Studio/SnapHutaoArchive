// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;
using System.Collections.Immutable;

namespace Snap.Hutao.Model.Metadata.Avatar;

internal sealed class SkillDepot
{
    public required Arkhe Arkhe { get; init; }

    public required ImmutableArray<ProudSkill> Skills { get; init; }

    public ImmutableArray<ProudSkill> SpecialSkills { get; init; }

    public required ProudSkill EnergySkill { get; init; }

    public required ImmutableArray<ProudSkill> Inherents { get; init; }

    public required ImmutableArray<Skill> Talents { get; init; }

    public ImmutableArray<ProudSkill> CompositeSkills { get => !field.IsDefault ? field : field = [.. EnumerateCompositeSkills()]; }

    // No Inherents && No SpecialSkills && 跳过 替换冲刺的技能
    public ImmutableArray<ProudSkill> CompositeSkillsNoInherents { get => !field.IsDefault ? field : field = [.. Skills.Where(s => s.Proud.Parameters.Count > 1), EnergySkill]; }

    private IEnumerable<ProudSkill> EnumerateCompositeSkills()
    {
        foreach (ProudSkill skill in Skills)
        {
            yield return skill;
        }

        yield return EnergySkill;

        if (SpecialSkills.IsDefaultOrEmpty)
        {
            foreach (ProudSkill inherent in Inherents)
            {
                yield return inherent;
            }
        }
        else
        {
            bool specialYield = false;
            foreach (ProudSkill inherent in Inherents)
            {
                // Insert SpecialSkills before the first Default display inherent skill
                if (inherent.Proud.Display is ProudSkillDisplay.Default && !specialYield)
                {
                    foreach (ProudSkill specialSkill in SpecialSkills)
                    {
                        yield return specialSkill;
                    }

                    specialYield = true;
                }

                yield return inherent;
            }
        }
    }
}