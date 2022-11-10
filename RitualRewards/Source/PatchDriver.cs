using HarmonyLib;
using Verse;

namespace RitualRewards;

[StaticConstructorOnStartup]
internal static class PatchDriver
{
    static PatchDriver()
    {
        Log.Message("RitualReward loaded!");
        Harmony harmony = new("RitualReward");
        harmony.PatchAll();
    }
}
