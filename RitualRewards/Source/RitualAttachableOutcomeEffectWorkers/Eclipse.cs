using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Eclipse : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        jobRitual.Map.gameConditionManager.GetActiveCondition<GameCondition_DisableElectricity>()?.End();
        GameCondition_NoSunlight cond = (GameCondition_NoSunlight)GameConditionMaker.MakeCondition(GameConditionDefOf.Eclipse, 120000);
        jobRitual.Map.GameConditionManager.RegisterCondition(cond);
        if (outcome.BestPositiveOutcome(jobRitual))
        {
            GameCondition_Aurora cond2 = (GameCondition_Aurora)GameConditionMaker.MakeCondition(GameConditionDefOf.Aurora, 30000);
            jobRitual.Map.GameConditionManager.RegisterCondition(cond2);
            extraOutcomeDesc = "EclipseWithAurora".Translate();
        }
        else
        {
            extraOutcomeDesc = def.letterInfoText;
        }
    }
}
