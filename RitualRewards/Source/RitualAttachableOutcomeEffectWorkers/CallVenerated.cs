using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class CallVenerated : RitualAttachableOutcomeEffectWorker
{
    private static Pawn SpawnAnimal(PawnKindDef kind, Gender? gender, IntVec3 loc, Map map)
    {
        PawnGenerationRequest request = new(
            kind: kind,
            context: PawnGenerationContext.NonPlayer,
            fixedGender: gender,
            allowDowned: true,
            canGeneratePawnRelations: false,
            colonistRelationChanceFactor: 0);
        Pawn pawn = PawnGenerator.GeneratePawn(request);
        _ = GenSpawn.Spawn(pawn, loc, map, Rot4.Random);

        return pawn;
    }

    private static float SelectionChance(PawnKindDef k)
    {
        return 1f / k.race.BaseMarketValue;
    }

    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));
        if (letterLookTargets is null)
            throw new ArgumentNullException(nameof(letterLookTargets));

        extraOutcomeDesc = string.Empty;
        bool isBestOutcome = outcome.BestPositiveOutcome(jobRitual);
        if (!isBestOutcome && Rand.Chance(0.7f))
            return;

        Map map = jobRitual.Map;
        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal))
        {
            extraOutcomeDesc = "VeneratedAnimalFailNoEntry".Translate();
            return;
        }

        if (jobRitual.Ritual.ideo.VeneratedAnimals.Count == 0)
        {
            extraOutcomeDesc = "VeneratedAnimalFailEmptyList".Translate();
            return;
        }

        IEnumerable<PawnKindDef> source =
            from x in DefDatabase<PawnKindDef>.AllDefs
            where jobRitual.Ritual.ideo.VeneratedAnimals.Contains(x.race)
            select x;

        PawnKindDef pawnKindDef;
        if (!isBestOutcome)
        {
            pawnKindDef = source.Where(x => map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race))
                                .RandomElementByWeight(SelectionChance);

            if (pawnKindDef == null)
            {
                extraOutcomeDesc = "VeneratedAnimalFailTemperature".Translate(outcome.label);
                return;
            }
        }
        else
        {
            pawnKindDef = source.RandomElementByWeight(SelectionChance);
        }

        if (isBestOutcome)
        {
            if (map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(pawnKindDef.race))
            {
                extraOutcomeDesc = "VeneratedAnimalCalledPair".Translate(pawnKindDef.label);
                letterLookTargets.targets.Add(SpawnAnimal(pawnKindDef, Gender.Male, result, map));
                letterLookTargets.targets.Add(SpawnAnimal(pawnKindDef, Gender.Female, result, map));
            }
            else
            {
                extraOutcomeDesc = "VeneratedAnimalCalledBadWeather".Translate(pawnKindDef.label);
                letterLookTargets.targets.Add(SpawnAnimal(pawnKindDef, null, result, map));
            }
        }
        else
        {
            letterLookTargets.targets.Add(SpawnAnimal(pawnKindDef, null, result, map));
            extraOutcomeDesc = "VeneratedAnimalCalled".Translate(pawnKindDef.label);
        }
    }
}