using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class WindBless : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        HashSet<string> reducingGC = ["HeatWave", "GlobalWarming"];
        List<GameCondition> list = [];
        jobRitual.Map.GameConditionManager.GetAllGameConditionsAffectingMap(jobRitual.Map, list);
        int divider = outcome.BestPositiveOutcome(jobRitual) ? 4 : 2;
        string text = Utilities.ReduceGameCondition(list, reducingGC, null, divider, "WindBlessAffectedEvents", string.Empty);
        GameCondition_ForceWeather gameCondition_ForceWeather = Activator.CreateInstance<GameCondition_ForceWeather>();
        gameCondition_ForceWeather.startTick = Find.TickManager.TicksGame;
        gameCondition_ForceWeather.def = GameConditionDef.Named("WindBlessing");
        gameCondition_ForceWeather.Duration = outcome.BestPositiveOutcome(jobRitual) ? 120000 : 60000;
        gameCondition_ForceWeather.uniqueID = Find.UniqueIDsManager.GetNextGameConditionID();
        gameCondition_ForceWeather.weather = WeatherDef.Named("Windy");
        jobRitual.Map.GameConditionManager.RegisterCondition(gameCondition_ForceWeather);
        extraOutcomeDesc = def.letterInfoText + text;
    }
}
