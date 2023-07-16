using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Dust : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));

        List<Thing> list = jobRitual.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Corpse corpse = (Corpse)list[i];
            if ((int)corpse.GetRotStage() >= 1)
            {
                corpse.DeSpawn();
                if (!corpse.Destroyed)
                    corpse.Destroy();

                if (!corpse.Discarded)
                    corpse.Discard();
            }
        }

        extraOutcomeDesc = def.letterInfoText;
    }
}
