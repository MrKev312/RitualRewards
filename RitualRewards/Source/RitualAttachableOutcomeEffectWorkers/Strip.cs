using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Strip : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        extraOutcomeDesc = string.Empty;
        bool flag = false;
        float chance = outcome.BestPositiveOutcome(jobRitual) ? 0.33f : 0.75f;
        IEnumerable<Pawn> enumerable =
            from p in jobRitual.Map.mapPawns.AllPawnsSpawned
            where p.Faction.HostileTo(Faction.OfPlayer)
            select p;
        if (!enumerable.Any())
            return;

        foreach (Pawn item in enumerable)
        {
            if (Rand.Chance(chance))
            {
                item.apparel.DropAll(item.PositionHeld, true, true);
                flag = true;
            }
        }

        if (flag)
            extraOutcomeDesc = def.letterInfoText + (outcome.BestPositiveOutcome(jobRitual) ? "stripGood".Translate() : "stripGreat".Translate());
    }
}
