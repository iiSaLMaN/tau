﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Tau.Beatmaps;
using osu.Game.Rulesets.Tau.Configuration;
using osu.Game.Rulesets.Tau.Mods;
using osu.Game.Rulesets.Tau.Replays;
using osu.Game.Rulesets.Tau.Scoring;
using osu.Game.Rulesets.Tau.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Tau
{
    public class TauRuleset : Ruleset
    {
        public override string Description => "tau";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) =>
            new DrawableTauRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new TauBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) =>
            new TauDifficultyCalculator(this, beatmap);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new TauModEasy(),
                        new TauModNoFail(),
                        new MultiMod(new TauModHalfTime(), new TauModDaycore()),
                        new TauModAutoHold(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new TauModHardRock(),
                        new TauModSuddenDeath(),
                        new MultiMod(new TauModDoubleTime(), new TauModNightcore()),
                        new TauModHidden(),
                        new MultiMod(new TauModFlashlight(), new TauModBlinds()),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new TauModAutoplay(), new ModCinema()),
                        new TauModRelax(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new TauModDifficultyAdjust(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                    };

                default:
                    return new Mod[] { null };
            }
        }

        public override string ShortName => "tau";

        public override RulesetSettingsSubsection CreateSettings() => new TauSettingsSubsection(this);

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new TauRulesetConfigManager(settings, RulesetInfo);

        public override ScoreProcessor CreateScoreProcessor() => new TauScoreProcessor();

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, TauAction.LeftButton),
            new KeyBinding(InputKey.X, TauAction.RightButton),
            new KeyBinding(InputKey.MouseLeft, TauAction.LeftButton),
            new KeyBinding(InputKey.MouseRight, TauAction.RightButton),
            new KeyBinding(InputKey.Space, TauAction.HardButton),
        };

        public override Drawable CreateIcon() => new Sprite
        {
            Texture = new TextureStore(new TextureLoaderStore(CreateResourceStore()), false).Get("Textures/tau"),
        };

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TauReplayFrame();
    }
}
