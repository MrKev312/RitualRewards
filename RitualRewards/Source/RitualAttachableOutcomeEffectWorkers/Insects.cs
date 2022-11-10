using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class Insects : RitualAttachableOutcomeEffectWorker
{
    private const float bestOutcome = 10f;

    private const float regularOutcome = 5f;

    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        Map map = jobRitual.Map;
        IntVec3 intVec = FindRootTunnelLoc(map);
        if (intVec == IntVec3.Invalid)
        {
            extraOutcomeDesc = "InsectNoExit".Translate();
            return;
        }

        bool flag = outcome.BestPositiveOutcome(jobRitual);
        int num = flag ? (int)bestOutcome : (int)regularOutcome;
        float num2 = flag ? bestOutcome : regularOutcome;
        List<PawnKindDef> possibleInsectList = GetPossibleInsectList(map);
        while (num > 0 && num2 > 0f)
        {
            num--;
            num2 -= SpawnInsects(intVec, map, possibleInsectList);
        }

        extraOutcomeDesc = def.letterInfoText;
        letterLookTargets = new LookTargets(intVec, map);
    }

    private static float SpawnInsects(IntVec3 intVec, Map map, List<PawnKindDef> insectList)
    {
        IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 3);
        PawnKindDef pawnKindDef = insectList.RandomElement();
        Pawn newThing = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                kind: pawnKindDef,
                canGeneratePawnRelations: false,
                allowGay: false,
                allowFood: false));
        _ = GenSpawn.Spawn(newThing, loc, map, Rot4.Random);
        return pawnKindDef.RaceProps.baseBodySize;
    }

    private static List<PawnKindDef> GetPossibleInsectList(Map map)
    {
        static bool Validator(List<CompProperties> compList)
        {
            foreach (CompProperties comp in compList)
            {
                Type compClass = comp.compClass;
                if (compClass.Name is "CompUntameable" or "CompFloating")
                    return false;
            }

            return true;
        }

        return (
            from pawnKindDef in DefDatabase<PawnKindDef>.AllDefs.AsParallel()
            where 
                pawnKindDef.RaceProps.Insect &&
                !pawnKindDef.defName.StartsWith("VFEI_VatGrown", StringComparison.Ordinal) &&
                pawnKindDef.RaceProps.wildness <= 0.8 &&
                map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(pawnKindDef.race) &&
                Validator(pawnKindDef.race.comps)
            select pawnKindDef).ToList();
    }

    private static IntVec3 FindRootTunnelLoc(Map map)
    {
        if (InfestationCellFinder.TryFindCell(out IntVec3 cell, map))
            return cell;
        if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((vector) => vector.Standable(map) && !vector.Fogged(map), map, out cell))
            return cell;
        return IntVec3.Invalid;
    }
}
