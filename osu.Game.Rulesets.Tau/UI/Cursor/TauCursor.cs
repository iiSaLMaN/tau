﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.UI.Cursor
{
    public class TauCursor : CompositeDrawable
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private DefaultCursor defaultCursor;

        public TauCursor()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            InternalChild = defaultCursor = new DefaultCursor(beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize);

            this.beatmap.BindTo(beatmap);
        }

        public bool CheckForValidation(DrawableTauHitObject h) => h.ScreenSpaceDrawQuad.AABBFloat.IntersectsWith(defaultCursor.HitReceptor.ScreenSpaceDrawQuad.AABBFloat);

        private class DefaultCursor : CompositeDrawable
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public readonly Box HitReceptor;

            public DefaultCursor(float cs = 5f)
            {
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;

                RelativeSizeAxes = Axes.Both;

                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1, // 1:1 Aspect Ratio.
                    Size = new Vector2(0.6f),
                    Children = new Drawable[]
                    {
                        HitReceptor = new Box
                        {
                            Height = 50,
                            Width = (float)convertValue(cs),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                });

                const double a = 1;
                const double b = 10;
                const double c = 230;
                const double d = 25;

                // Thank you AlFas for this code.
                double convertValue(double value) => c + (((d - c) * (value - a)) / (b - a));

                AddInternal(new GameplayCursor(cs));

                AddInternal(new AbsoluteCursor
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            private class AbsoluteCursor : CursorContainer
            {
                protected override Drawable CreateCursor() => new CircularContainer
                {
                    Size = new Vector2(15),
                    Origin = Anchor.Centre,
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 4,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    }
                };
            }

            private class GameplayCursor : CompositeDrawable
            {
                public GameplayCursor(float cs)
                {
                    RelativeSizeAxes = Axes.Both;

                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;

                    FillMode = FillMode.Fit;
                    FillAspectRatio = 1; // 1:1 Aspect Ratio.

                    InternalChildren = new Drawable[]
                    {
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(0.6f),
                            Children = new Drawable[]
                            {
                                new CircularProgress
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Current = new BindableDouble(convertValue(cs)),
                                    InnerRadius = 0.05f,
                                    Rotation = -360 * ((float)convertValue(cs) / 2)
                                },
                                new Box
                                {
                                    EdgeSmoothness = new Vector2(1f),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Y,
                                    Size = new Vector2(2.5f / 2, 0.5f),
                                }
                            }
                        }
                    };

                    const double a = 2;
                    const double b = 7;
                    const double c = 0.15;
                    const double d = 0.05;

                    // Thank you AlFas for this code.
                    double convertValue(double value) => c + (((d - c) * (value - a)) / (b - a));
                }
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var angle = e.MousePosition.GetDegreesFromPosition(AnchorPosition) - 25;

                Rotation = angle;

                return base.OnMouseMove(e);
            }
        }
    }
}
