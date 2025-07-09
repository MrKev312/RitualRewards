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

    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
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

        List<Pawn> candidates = [.. totalPresence.Keys.Where(pawn => HealthUtility.TryGetWorstHealthCondition(pawn, out _, out _, null))];

        if (candidates.Count > 0)
        {
            Pawn healTarget = candidates.RandomElement();
            _ = HealthUtility.FixWorstHealthCondition(healTarget);
            extraOutcomeDesc = string.Format(CultureInfo.InvariantCulture, def.letterInfoText, healTarget.Name);
        }
        else
        {
            extraOutcomeDesc = "Sinnamon_HealFailNoTarget".Translate();
        }
    }
}