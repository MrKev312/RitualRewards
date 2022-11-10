using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Random : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        IEnumerable<RitualAttachableOutcomeEffectWorker> list =
            from defs in DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs
            where defs.defName is not "Random" and not "Aurora"
            select defs.Worker;

        RitualAttachableOutcomeEffectWorker ritualAttachableOutcomeEffectWorker = list.RandomElement();
        ritualAttachableOutcomeEffectWorker.Apply(totalPresence, jobRitual, outcome, out string extraOutcomeDesc2, ref letterLookTargets);
        extraOutcomeDesc = "Random".Translate(ritualAttachableOutcomeEffectWorker.def.label) + extraOutcomeDesc2;
    }
}
