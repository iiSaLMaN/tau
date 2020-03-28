﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Tau.Configuration;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osu.Game.Rulesets.Tau.UI.Cursor;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.UI
{
    [Cached]
    public class TauPlayfield : Playfield
    {
        private Circle playfieldBackground;
        private TauCursor cursor;
        private JudgementContainer<DrawableTauJudgement> judgementLayer;
        private readonly Container<KiaiHitExplosion> kiaiExplosionContainer;

        public const float UNIVERSAL_SCALE = 0.6f;
        public static readonly Vector2 BASE_SIZE = new Vector2(768, 768);

        public TauPlayfield()
        {
            cursor = new TauCursor();

            AddRangeInternal(new Drawable[]
            {
                judgementLayer = new JudgementContainer<DrawableTauJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new VisualisationContainer(),
                playfieldBackground = new Circle
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(UNIVERSAL_SCALE),
                    FillAspectRatio = 1,
                    FillMode = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(UNIVERSAL_SCALE),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            FillMode = FillMode.Fit,
                            FillAspectRatio = 1, // 1:1 Aspect ratio to get a perfect circle
                            BorderThickness = 3,
                            BorderColour = Color4.White,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Alpha = 0,
                            }
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(UNIVERSAL_SCALE),
                    Children = new Drawable[]
                    {
                        HitObjectContainer,
                    }
                },
                cursor,
                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                {
                    Name = "Kiai hit explosions",
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Blending = BlendingParameters.Additive,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
            });
        }

        [Resolved]
        private TauRulesetConfigManager config { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PlayfieldDimLevel = config.GetBindable<float>(TauRulesetSettings.PlayfieldDim);
            PlayfieldDimLevel.ValueChanged += _ => updateVisuals();

            updateVisuals();
        }

        protected Bindable<float> PlayfieldDimLevel { get; private set; }

        private void updateVisuals()
        {
            playfieldBackground.FadeTo(PlayfieldDimLevel.Value, 100);
        }

        public bool CheckIfWeCanValidate(DrawableTauHitObject obj) => cursor.CheckForValidation(obj);

        public override void Add(DrawableHitObject h)
        {
            base.Add(h);

            var obj = (DrawableTauHitObject)h;
            obj.CheckValidation = CheckIfWeCanValidate;

            obj.OnNewResult += onNewResult;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            var tauObj = (DrawableTauHitObject)judgedObject;

            var b = tauObj.HitObject.PositionToEnd.GetDegreesFromPosition(tauObj.Box.AnchorPosition) * 4;
            var a = b *= (float)(Math.PI / 180);

            DrawableTauJudgement explosion = new DrawableTauJudgement(result, tauObj)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Position = new Vector2(-(285 * (float)Math.Cos(a)), -(285 * (float)Math.Sin(a))),
                Rotation = tauObj.Box.Rotation + 90,
            };

            judgementLayer.Add(explosion);

            if (judgedObject.HitObject.Kiai && result.Type != HitResult.Miss)
                kiaiExplosionContainer.Add(new KiaiHitExplosion(judgedObject)
                {
                    Position = new Vector2(-(215 * (float)Math.Cos(a)), -(215 * (float)Math.Sin(a))),
                    Rotation = tauObj.Box.Rotation,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });
        }

        private class VisualisationContainer : BeatSyncedContainer
        {
            private LogoVisualisation visualisation;
            private bool firstKiaiBeat = true;
            private int kiaiBeatIndex;
            protected Bindable<bool> ShowVisualisation;

            [BackgroundDependencyLoader(true)]
            private void load(TauRulesetConfigManager settings)
            {
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(UNIVERSAL_SCALE);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Child = visualisation = new LogoVisualisation
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Transparent
                };

                ShowVisualisation = settings.GetBindable<bool>(TauRulesetSettings.ShowVisualizer);

                ShowVisualisation.ValueChanged += value => { visualisation.FadeTo(value.NewValue ? 1 : 0, 500); };
                ShowVisualisation.TriggerChange();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                visualisation.AccentColour = Color4.White;
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode)
                {
                    kiaiBeatIndex += 1;

                    if (firstKiaiBeat)
                    {
                        visualisation.FlashColour(Color4.White, timingPoint.BeatLength * 4, Easing.In);
                        firstKiaiBeat = false;

                        return;
                    }

                    if (kiaiBeatIndex >= 5)
                        visualisation.FlashColour(Color4.White.Opacity(0.15f), timingPoint.BeatLength, Easing.In);
                }
                else
                {
                    firstKiaiBeat = true;
                    kiaiBeatIndex = 0;
                }
            }
        }
    }
}
