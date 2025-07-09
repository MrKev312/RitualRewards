using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Heal : RitualAttachableOutcomeEffectWorker
{
    private const float BestOutcomeHealChance = 0.3f;
    private const float RegularOutcomeHealChance = 0.1f;

#if V1_3 || V1_4
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#else
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#endif
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));
        if (totalPresence is null)
            throw new ArgumentNullException(nameof(totalPresence));

        bool isBest = outcome.BestPositiveOutcome(jobRitual);
        float chance = isBest ? BestOutcomeHealChance : RegularOutcomeHealChance;
        if (!Rand.Chance(chance))
        {
            extraOutcomeDesc = null;
            return;
        }

#if V1_3 || V1_4
        List<Pawn> candidates = [.. totalPresence.Keys
            .Where(pawn => pawn.health.hediffSet.hediffs
            .Any(hediff =>
                hediff is not Hediff_MissingPart &&
                hediff.Visible &&
                hediff.Severity > 0f))];
#else
        List<Pawn> candidates = [.. totalPresence.Keys.Where(pawn => HealthUtility.TryGetWorstHealthCondition(pawn, out _, out _, null))];
#endif

        if (candidates.Count > 0)
        {
            Pawn healTarget = candidates.RandomElement();
            _ = HealthUtility.FixWorstHealthCondition(healTarget);
            extraOutcomeDesc = string.Format(CultureInfo.InvariantCulture, def.letterInfoText, healTarget.LabelShort);
        }
        else
        {
            extraOutcomeDesc = "HealFailNoTarget".Translate();
        }
    }
}