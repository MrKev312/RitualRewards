using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards;

public static class Utilities
{
    public static string ReduceGameCondition(ICollection<GameCondition> currentConditions, HashSet<string> reducingGC, HashSet<string> endingGC, int divider, string reducingMessage, string endingMessage)
    {
        if (currentConditions is null)
            throw new ArgumentNullException(nameof(currentConditions));
        if (reducingGC is null)
            throw new ArgumentNullException(nameof(reducingGC));
        if (endingGC is null)
            throw new ArgumentNullException(nameof(endingGC));

        string text = string.Empty;
        foreach (GameCondition currentCondition in currentConditions)
        {
            if (reducingGC.Contains(currentCondition.def.defName))
            {
                text += reducingMessage.Translate(currentCondition.def.label);
                currentCondition.Duration /= divider;
            }

            if (endingGC.Contains(currentCondition.def.defName))
            {
                text += endingMessage.Translate(currentCondition.def.label);
                currentCondition.End();
            }
        }

        return text;
    }
}
