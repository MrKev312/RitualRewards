using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Taunt : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));

        IncidentParms parms = new()
        {
            target = jobRitual.Map,
            points = jobRitual.Map.PlayerWealthForStoryteller / 500f
        };
        _ = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        extraOutcomeDesc = def.letterInfoText;
    }
}
