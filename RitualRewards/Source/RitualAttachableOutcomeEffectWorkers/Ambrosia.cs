using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Ambrosia : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
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
