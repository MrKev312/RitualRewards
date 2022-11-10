using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Random : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        List<RitualAttachableOutcomeEffectWorker> list = new();
        foreach (RitualAttachableOutcomeEffectDef allDef in DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs)
        {
            if (allDef.defName is not "Random" and not "Aurora")
                list.Add(allDef.Worker);
        }

        RitualAttachableOutcomeEffectWorker ritualAttachableOutcomeEffectWorker = list.RandomElement();
        ritualAttachableOutcomeEffectWorker.Apply(totalPresence, jobRitual, outcome, out string extraOutcomeDesc2, ref letterLookTargets);
        extraOutcomeDesc = "Random".Translate(ritualAttachableOutcomeEffectWorker.def.label) + extraOutcomeDesc2;
    }
}
