using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Ambrosia : RitualAttachableOutcomeEffectWorker
{
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

        IncidentParms parms = new()
        {
            target = jobRitual.Map,
            points = outcome.BestPositiveOutcome(jobRitual) ? 1510 : 1005
        };
        IncidentDef incidentDef = IncidentDef.Named("SmallAmbrosiaSprout");
        if (!incidentDef.Worker.CanFireNow(parms))
        {
            extraOutcomeDesc = "AmbrosiaFailed";
            return;
        }

        _ = incidentDef.Worker.TryExecute(parms);
        extraOutcomeDesc = def.letterInfoText;
    }
}
