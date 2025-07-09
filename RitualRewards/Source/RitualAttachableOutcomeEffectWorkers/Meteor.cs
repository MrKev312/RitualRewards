using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Meteor : RitualAttachableOutcomeEffectWorker
{
#if V1_3 || V1_4
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#else
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#endif
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));

        IncidentParms parms = new()
        {
            target = jobRitual.Map
        };
        _ = IncidentDef.Named("MeteoriteImpact").Worker.TryExecute(parms);
        extraOutcomeDesc = def.letterInfoText;
    }
}
