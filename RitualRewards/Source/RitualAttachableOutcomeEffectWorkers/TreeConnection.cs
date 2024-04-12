using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class TreeConnection : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (totalPresence is null)
            throw new ArgumentNullException(nameof(totalPresence));

        foreach (Pawn key in totalPresence.Keys)
        {
            foreach (Thing connectedThing in key.connections.ConnectedThings)
            {
                if (connectedThing.def.defName == "Plant_TreeGauranlen")
                    connectedThing.TryGetComp<CompTreeConnection>().ConnectionStrength = 1f;
            }
        }

        extraOutcomeDesc = def.letterInfoText;
    }
}
