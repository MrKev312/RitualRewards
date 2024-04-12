using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Trader : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));

        IncidentParms parms = new()
        {
            target = jobRitual.Map,
            sendLetter = false
        };
        _ = IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
        extraOutcomeDesc = def.letterInfoText;
    }
}
