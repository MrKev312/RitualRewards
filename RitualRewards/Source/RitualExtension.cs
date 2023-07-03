using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RitualRewards;

public class RitualExtension : DefModExtension
{
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA1051 // Do not declare visible instance fields
    public List<MemeDef> ForbiddenMemeAny;
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1002 // Do not expose generic lists
}
