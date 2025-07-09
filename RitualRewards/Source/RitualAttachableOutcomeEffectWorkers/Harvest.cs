using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Harvest : RitualAttachableOutcomeEffectWorker
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

        float num = outcome.BestPositiveOutcome(jobRitual) ? 20 : 10;
        HashSet<string> hashSet = [];
        int num2 = 0;
        List<Zone> allZones = jobRitual.Map.zoneManager.AllZones;
        List<Zone> list = [.. (
            from x in allZones.AsParallel()
            where x.GetType() == typeof(Zone_Growing)
            select x).InRandomOrder()];
        foreach (Zone item in list)
        {
            string label = ((Zone_Growing)item).GetPlantDefToGrow().label;
            foreach (IntVec3 cell in item.cells)
            {
                Plant plant = cell.GetPlant(jobRitual.Map);
                if (plant == null)
                    continue;

                if (plant.def.label == label && plant.Growth < 1f)
                {
                    _ = hashSet.Add(plant.Label);
                    num2++;
                    if (plant.Growth > 0.5f)
                    {
                        num -= 1f - plant.Growth;
                        plant.Growth = 1f;
                    }
                    else
                    {
                        num -= 0.5f;
                        plant.Growth += 0.5f;
                    }
                }

                if (num < 0f)
                    break;
            }

            if (num < 0f)
                break;
        }

        extraOutcomeDesc = "HarvestResult".Translate(num2, string.Join(", ", hashSet));
    }
}
