using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Aurora : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        int days = outcome.BestPositiveOutcome(jobRitual) ? 4 : 2;
        GameConditions.Aurora cond = (GameConditions.Aurora)GameConditionMaker.MakeCondition(GameConditionDefOf.Aurora, days * 60000);
        jobRitual.Map.GameConditionManager.RegisterCondition(cond);
        extraOutcomeDesc = def.letterInfoText + $" {days} days.";
    }
}
