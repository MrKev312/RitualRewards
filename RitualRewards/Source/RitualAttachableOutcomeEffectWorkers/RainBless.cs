using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class RainBless : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        HashSet<string> reducingGC = new()
            { "ToxicFallout", "VolcanicWinter" };
        HashSet<string> endingGC = new()
            { "Drought" };
        List<GameCondition> list = new();
        jobRitual.Map.GameConditionManager.GetAllGameConditionsAffectingMap(jobRitual.Map, list);
        int divider = outcome.BestPositiveOutcome(jobRitual) ? 4 : 2;
        string text = Utilities.ReduceGameCondition(list, reducingGC, endingGC, divider, "RainBlessAffectedEvents", "RainBlessEndedEvents");
        GameCondition_ForceWeather gameCondition_ForceWeather = (GameCondition_ForceWeather)Activator.CreateInstance(typeof(GameCondition_ForceWeather));
        gameCondition_ForceWeather.startTick = Find.TickManager.TicksGame;
        gameCondition_ForceWeather.def = GameConditionDef.Named("RainBlessing");
        gameCondition_ForceWeather.Duration = outcome.BestPositiveOutcome(jobRitual) ? 120000 : 60000;
        gameCondition_ForceWeather.uniqueID = Find.UniqueIDsManager.GetNextGameConditionID();
        gameCondition_ForceWeather.weather = WeatherDef.Named("Rain");
        jobRitual.Map.GameConditionManager.RegisterCondition(gameCondition_ForceWeather);
        extraOutcomeDesc = def.letterInfoText + text;
    }
}
