using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Strip : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        extraOutcomeDesc = string.Empty;
        bool flag = false;
        float chance = outcome.BestPositiveOutcome(jobRitual) ? 0.33f : 0.75f;
        List<Pawn> enumerable = [.. jobRitual.Map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.Faction.HostileTo(Faction.OfPlayer))];

        if (enumerable.Count == 0)
            return;

        foreach (Pawn item in enumerable)
        {
            if (Rand.Chance(chance))
            {
                item.apparel.DropAll(item.PositionHeld);
                flag = true;
            }
        }

        if (flag)
            extraOutcomeDesc = outcome.BestPositiveOutcome(jobRitual) ? "stripGood".Translate() : "stripGreat".Translate();
    }
}