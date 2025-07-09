using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RitualRewards.RitualAttachableOutcomeEffectWorkers;

public class MentalTuning : RitualAttachableOutcomeEffectWorker
{
    private sealed class TraitPreferences(IEnumerable<TraitDef> favored = null, IEnumerable<TraitDef> unfavored = null, IEnumerable<TraitDef> ignored = null)
    {
        public readonly HashSet<TraitDef> FavoredTraits = [.. favored ?? []];
        public readonly HashSet<TraitDef> UnfavoredTraits = [.. unfavored ?? []];
        public readonly HashSet<TraitDef> IgnoredCommonUnfavoredTraits = [.. ignored ?? []];
    }

    private static readonly TraitPreferences CommonTraitPreferences = new(
        [TraitDefOf.Joyous, TraitDefOf.GreatMemory, TraitDef.Named("QuickSleeper"), TraitDef.Named("FastLearner"), TraitDef.Named("SpeedOffset"), TraitDefOf.Industriousness, TraitDef.Named("NaturalMood"), TraitDef.Named("Nerves"), TraitDef.Named("Immunity")],
        [TraitDef.Named("SlowLearner"), TraitDef.Named("SpeedOffset"), TraitDef.Named("NaturalMood"), TraitDef.Named("Nerves"), TraitDefOf.Industriousness, TraitDef.Named("Immunity"), TraitDefOf.Pyromaniac, TraitDefOf.Jealous, TraitDefOf.Greedy, TraitDefOf.Abrasive, TraitDef.Named("Gourmand")]
    );

    private static readonly Dictionary<MemeDef, TraitPreferences> MemeTraitPreferences = new()
    {
        { MemeDefOf.Tunneler, new TraitPreferences([TraitDefOf.Undergrounder]) },
        { MemeDefOf.MaleSupremacy, new TraitPreferences(null, [TraitDefOf.DislikesMen]) },
        { MemeDefOf.FemaleSupremacy, new TraitPreferences(null, [TraitDefOf.DislikesWomen]) },
        { DefDatabase<MemeDef>.GetNamed("Individualist"), new TraitPreferences(null, null, [TraitDefOf.Greedy, TraitDefOf.Jealous]) },
        { DefDatabase<MemeDef>.GetNamed("Cannibal"), new TraitPreferences([TraitDefOf.Psychopath], [TraitDefOf.Kind]) },
        { DefDatabase<MemeDef>.GetNamed("HighLife"), new TraitPreferences([TraitDefOf.DrugDesire], [TraitDefOf.DrugDesire]) },
        { DefDatabase<MemeDef>.GetNamed("Raider"), new TraitPreferences([TraitDefOf.Psychopath], [TraitDefOf.Kind]) }
    };

    static MentalTuning()
    {
        foreach (MemeDef meme in DefDatabase<MemeDef>.AllDefs)
        {
            if (!MemeTraitPreferences.ContainsKey(meme))
                MemeTraitPreferences.Add(meme, new TraitPreferences());

            if (!meme.agreeableTraits.NullOrEmpty())
            {
                foreach (TraitRequirement trait in meme.agreeableTraits)
                {
                    _ = MemeTraitPreferences[meme].FavoredTraits.Add(trait.def);
                    _ = MemeTraitPreferences[meme].IgnoredCommonUnfavoredTraits.Add(trait.def);
                }
            }

            if (!meme.disagreeableTraits.NullOrEmpty())
            {
                foreach (TraitRequirement trait in meme.disagreeableTraits)
                    _ = MemeTraitPreferences[meme].UnfavoredTraits.Add(trait.def);
            }
        }

        if (ModLister.HasActiveModWithName("VanillaExpanded.VanillaTraitsExpanded"))
        {
            _ = CommonTraitPreferences.UnfavoredTraits.Add(TraitDef.Named("VTE_AbsentMinded"));
            _ = CommonTraitPreferences.UnfavoredTraits.Add(TraitDef.Named("VTE_Coward"));
            _ = CommonTraitPreferences.UnfavoredTraits.Add(TraitDef.Named("VTE_Clumsy"));
            _ = CommonTraitPreferences.UnfavoredTraits.Add(TraitDef.Named("VTE_Neat"));
            _ = CommonTraitPreferences.UnfavoredTraits.Add(TraitDef.Named("VTE_Rebel"));

            _ = MemeTraitPreferences[MemeDefOf.Tunneler].FavoredTraits.Add(TraitDef.Named("VTE_Groundbreaker"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("Rancher")].FavoredTraits.Add(TraitDef.Named("VTE_AnimalLover"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("Rancher")].UnfavoredTraits.Add(TraitDef.Named("VTE_AnimalHater"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")].FavoredTraits.Add(TraitDef.Named("VTE_AnimalLover"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")].UnfavoredTraits.Add(TraitDef.Named("VTE_AnimalHater"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("NaturePrimacy")].FavoredTraits.Add(TraitDef.Named("VTE_AnimalLover"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("NaturePrimacy")].UnfavoredTraits.Add(TraitDef.Named("VTE_AnimalHater"));
            _ = MemeTraitPreferences[DefDatabase<MemeDef>.GetNamed("HumanPrimacy")].UnfavoredTraits.Add(TraitDef.Named("VTE_AnimalHater"));
        }
    }

    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        if (totalPresence is null)
            throw new ArgumentNullException(nameof(totalPresence));
        if (jobRitual is null)
            throw new ArgumentNullException(nameof(jobRitual));
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        extraOutcomeDesc = null;
        bool isBest = outcome.BestPositiveOutcome(jobRitual);
        if (!Rand.Chance(isBest ? 0.75f : 0.5f))
        {
            return;
        }

        List<Pawn> pawns = [.. totalPresence.Keys];
        List<MemeDef> memes = jobRitual.Ritual.ideo.memes;

        HashSet<TraitDef> unfavoredTraits = [];
        HashSet<TraitDef> ignoredTraits = [];
        foreach (MemeDef meme in memes)
        {
            unfavoredTraits.UnionWith(MemeTraitPreferences[meme].UnfavoredTraits);
            ignoredTraits.UnionWith(MemeTraitPreferences[meme].IgnoredCommonUnfavoredTraits);
        }

        unfavoredTraits.UnionWith(CommonTraitPreferences.UnfavoredTraits.Where(t => !ignoredTraits.Contains(t)));

        List<Pawn> pawnsWithUnfavored = [.. pawns.Where(pawn => pawn.story.traits.allTraits.Any(trait => unfavoredTraits.Contains(trait.def)))];

        if (pawnsWithUnfavored.Count > 0)
        {
            Pawn pawn = pawnsWithUnfavored.RandomElement();
            Trait traitToAffect = pawn.story.traits.allTraits
                .Where(t => unfavoredTraits.Contains(t.def))
                .RandomElementByWeight(t => t.def.GetGenderSpecificCommonality(pawn.gender));

            pawn.story.traits.RemoveTrait(traitToAffect);

            if (!traitToAffect.def.degreeDatas.NullOrEmpty() && traitToAffect.Degree < -1)
            {
                pawn.story.traits.GainTrait(new Trait(traitToAffect.def, traitToAffect.Degree + 1));
                extraOutcomeDesc = "MindTuneImprove".Translate(pawn.Name.ToStringShort, jobRitual.RitualLabel, traitToAffect.def.LabelCap);
            }
            else
            {
                extraOutcomeDesc = "MindTuneRemove".Translate(pawn.Name.ToStringShort, jobRitual.RitualLabel, traitToAffect.def.LabelCap);
            }

            return;
        }

        HashSet<TraitDef> favoredTraits = [];
        foreach (MemeDef meme in memes)
        {
            favoredTraits.UnionWith(MemeTraitPreferences[meme].FavoredTraits);
        }

        favoredTraits.UnionWith(CommonTraitPreferences.FavoredTraits);

        Pawn pawnToGiveTrait = pawns.RandomElementByWeight(p => 1f / (p.story.traits.allTraits.Count + 1));

        TraitDef traitToAdd = favoredTraits.Where(traitDef =>
        {
            Trait existing = pawnToGiveTrait.story.traits.GetTrait(traitDef);
            if (existing == null)
                return true;
            if (traitDef.degreeDatas.NullOrEmpty())
                return false;
            return existing.Degree < traitDef.degreeDatas.Max(d => d.degree);
        }).ToList().RandomElementByWeightWithFallback(t => t.GetGenderSpecificCommonality(pawnToGiveTrait.gender));

        if (traitToAdd != null)
        {
            Trait existingTrait = pawnToGiveTrait.story.traits.GetTrait(traitToAdd);
            if (existingTrait != null)
            {
                // Upgrade existing trait
                int newDegree = existingTrait.Degree + 1;
                pawnToGiveTrait.story.traits.RemoveTrait(existingTrait);
                pawnToGiveTrait.story.traits.GainTrait(new Trait(traitToAdd, newDegree));
                extraOutcomeDesc = "MindTuneImprove".Translate(pawnToGiveTrait.Name.ToStringShort, jobRitual.RitualLabel, traitToAdd.LabelCap);
            }
            else
            {
                // Add new trait
                int newDegree = traitToAdd.degreeDatas.NullOrEmpty() ? 0 : traitToAdd.degreeDatas.Min(d => d.degree);
                pawnToGiveTrait.story.traits.GainTrait(new Trait(traitToAdd, newDegree));
                extraOutcomeDesc = "MindTuneAdd".Translate(pawnToGiveTrait.Name.ToStringShort, jobRitual.RitualLabel, traitToAdd.LabelCap);
            }
        }
        else
        {
            extraOutcomeDesc = "MindTuneNoFavoredTrait".Translate();
        }
    }
}