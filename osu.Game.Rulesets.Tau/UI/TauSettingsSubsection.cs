﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Tau.Configuration;

namespace osu.Game.Rulesets.Tau.UI
{
    public class TauSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "tau";

        public TauSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (TauRulesetConfigManager)Config;

            // for an odd reason, Config seems to be passed as null when creating it. doesnt even get called...
            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show Visualizer",
                    Bindable = config.GetBindable<bool>(TauRulesetSettings.ShowVisualizer)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Playfield dim",
                    Bindable = config.GetBindable<float>(TauRulesetSettings.PlayfieldDim),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }
            };
        }
    }
}
