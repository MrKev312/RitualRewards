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
        { DefDatabase<MemeDef>.GetNamed("Individualism"), new TraitPreferences(null, null, [TraitDefOf.Greedy, TraitDefOf.Jealous]) },
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

        bool isBest = outcome.BestPositiveOutcome(jobRitual);
        if ((!isBest && Rand.Chance(0.5f)) || (isBest && Rand.Chance(0.25f)))
        {
            extraOutcomeDesc = null;
            return;
        }

        List<Pawn> pawns = [.. totalPresence.Keys];
        List<MemeDef> memes = jobRitual.Ritual.ideo.memes;

        HashSet<TraitDef> unfavoredTraits = [];
        HashSet<TraitDef> ignoredTraits = [];
        foreach (MemeDef meme in memes)
        {
            unfavoredTraits.UnionWith(MemeTraitPreferences[meme].UnfavoredTraits);
            unfavoredTraits.UnionWith(MemeTraitPreferences[meme].IgnoredCommonUnfavoredTraits);
        }

        foreach (TraitDef trait in CommonTraitPreferences.UnfavoredTraits)
        {
            if (!ignoredTraits.Contains(trait))
                _ = unfavoredTraits.Add(trait);
        }

        List<Pawn> pawnsWithUnfavored = [.. pawns.Where(pawn => pawn.story.traits.allTraits.Any(trait => unfavoredTraits.Contains(trait.def)))];

        if (pawnsWithUnfavored.Count > 0)
        {
            Pawn pawn = pawnsWithUnfavored.RandomElement();
            Trait traitToRemove = pawn.story.traits.allTraits
                .Where(t => unfavoredTraits.Contains(t.def))
                .RandomElementByWeight(t => t.def.GetGenderSpecificCommonality(Gender.None));

            pawn.story.traits.RemoveTrait(traitToRemove);

            if (!traitToRemove.def.degreeDatas.NullOrEmpty() && traitToRemove.Degree < -1)
            {
                pawn.story.traits.GainTrait(new Trait(traitToRemove.def, traitToRemove.Degree + 1));
                extraOutcomeDesc = "MindTuneImprove".Translate(pawn.Name.ToStringShort, jobRitual.RitualLabel, traitToRemove.def.label);
            }
            else
            {
                extraOutcomeDesc = "MindTuneRemove".Translate(pawn.Name.ToStringShort, jobRitual.RitualLabel, traitToRemove.def.label);
            }

            return;
        }

        HashSet<TraitDef> favoredTraits = [];
        foreach (MemeDef meme in memes)
        {
            foreach (TraitDef trait in MemeTraitPreferences[meme].FavoredTraits)
            {
                if (!(trait.degreeDatas.NullOrEmpty() && unfavoredTraits.Contains(trait)))
                    _ = favoredTraits.Add(trait);
            }
        }

        Pawn pawnToGiveTrait = pawns.RandomElementByWeight(p => 1f / p.story.traits.allTraits.Count);
        int newTraitDegree = 0;
        TraitDef traitToAdd = favoredTraits.RandomElementByWeight(trait =>
        {
            if (!trait.degreeDatas.NullOrEmpty())
            {
                int highestDegree = trait.degreeDatas.Max(d => d.degree);
                if (pawnToGiveTrait.story.traits.HasTrait(trait) && pawnToGiveTrait.story.traits.GetTrait(trait).Degree == highestDegree)
                    return 0;
            }
            else if (pawnToGiveTrait.story.traits.HasTrait(trait))
            {
                return 0;
            }

            return trait.GetGenderSpecificCommonality(Gender.None);
        });

        if (traitToAdd != null)
        {
            if (traitToAdd.degreeDatas.NullOrEmpty())
            {
                Trait traitToRemove = pawnToGiveTrait.story.traits.allTraits.Find(t => t.def == traitToAdd);
                newTraitDegree = traitToRemove != null ? traitToRemove.Degree + 1 : 0;
                if (traitToRemove != null)
                    pawnToGiveTrait.story.traits.RemoveTrait(traitToRemove);

                extraOutcomeDesc = "MindTuneImprove".Translate(pawnToGiveTrait.Name.ToStringShort, jobRitual.RitualLabel, traitToAdd.label);
            }
            else
            {
                extraOutcomeDesc = "MindTuneRemove".Translate(pawnToGiveTrait.Name.ToStringShort, jobRitual.RitualLabel, traitToAdd.label);
            }

            pawnToGiveTrait.story.traits.GainTrait(new Trait(traitToAdd, newTraitDegree));
        }
        else
        {
            extraOutcomeDesc = null;
        }
    }
}