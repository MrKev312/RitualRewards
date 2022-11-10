using System.Collections.Generic;
using System.Collections.ObjectModel;
using RimWorld;
using Verse;

namespace RitualRewards;

public class RitualExtension : DefModExtension
{
    public IList<MemeDef> ForbiddenMemeAny { get; }
}
