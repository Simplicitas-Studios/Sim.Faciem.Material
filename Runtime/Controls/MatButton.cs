using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sim.Faciem.Controls;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// A button control that mirrors Angular Material's button system.
    /// Inherits all binding and command capabilities from <see cref="BindableButton"/>.
    /// Variant and colour are applied exclusively through USS classes; no stylesheet
    /// is loaded programmatically — add a MatButton theme TSS to your PanelSettings.
    /// </summary>
    [UxmlElement]
    public partial class MatButton : BindableButton
    {
        // ── Shared base ────────────────────────────────────────────────────────
        public const string BaseClassName = "mat-button-base";

        // ── Variant classes ────────────────────────────────────────────────────
        public const string BasicClassName   = "mat-button";
        public const string RaisedClassName  = "mat-raised-button";
        public const string StrokedClassName = "mat-stroked-button";
        public const string FlatClassName    = "mat-flat-button";
        public const string IconClassName    = "mat-icon-button";
        public const string FabClassName     = "mat-fab";
        public const string MiniFabClassName = "mat-mini-fab";

        // ── Colour classes ─────────────────────────────────────────────────────
        public const string PrimaryClassName = "mat-primary";
        public const string AccentClassName  = "mat-accent";
        public const string WarnClassName    = "mat-warn";

        // ── Internal lookup arrays ─────────────────────────────────────────────
        private static readonly string[] AllVariantClasses =
        {
            BasicClassName, RaisedClassName, StrokedClassName, FlatClassName,
            IconClassName, FabClassName, MiniFabClassName,
        };

        private static readonly string[] AllColorClasses =
        {
            PrimaryClassName, AccentClassName, WarnClassName,
        };

        // ── Backing fields ─────────────────────────────────────────────────────
        private readonly MatRippleController _rippleController;

        private MatButtonVariant _variant = MatButtonVariant.Basic;
        private MatButtonColor _themeColor = MatButtonColor.Default;
        private bool _disableRipple;

        // ── UXML attributes ────────────────────────────────────────────────────

        /// <summary>Controls which Angular Material button appearance is applied.</summary>
        [UxmlAttribute]
        [CreateProperty]
        public MatButtonVariant Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                foreach (var cls in AllVariantClasses)
                    RemoveFromClassList(cls);
                AddToClassList(GetVariantClassName(value));
                MarkDirtyRepaint();
            }
        }

        /// <summary>Applies a theme palette colour class to the button.</summary>
        [UxmlAttribute]
        [CreateProperty]
        public MatButtonColor ThemeColor
        {
            get => _themeColor;
            set
            {
                _themeColor = value;
                foreach (var cls in AllColorClasses)
                    RemoveFromClassList(cls);
                if (value != MatButtonColor.Default)
                    AddToClassList(GetColorClassName(value));
            }
        }

        /// <summary>When true the animated ripple enhancement is suppressed.</summary>
        [UxmlAttribute]
        [CreateProperty]
        public bool DisableRipple
        {
            get => _disableRipple;
            set
            {
                _disableRipple = value;
                _rippleController.DisableRipple = value;
            }
        }

        // ── Constructor ────────────────────────────────────────────────────────

        public MatButton()
        {
            _rippleController = new MatRippleController(this);

            AddToClassList(BaseClassName);
            Variant = MatButtonVariant.Basic;
            ThemeColor = MatButtonColor.Default;
            DisableRipple = false;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string GetVariantClassName(MatButtonVariant variant) => variant switch
        {
            MatButtonVariant.Basic   => BasicClassName,
            MatButtonVariant.Raised  => RaisedClassName,
            MatButtonVariant.Stroked => StrokedClassName,
            MatButtonVariant.Flat    => FlatClassName,
            MatButtonVariant.Icon    => IconClassName,
            MatButtonVariant.Fab     => FabClassName,
            MatButtonVariant.MiniFab => MiniFabClassName,
            _                        => BasicClassName,
        };

        private static string GetColorClassName(MatButtonColor color) => color switch
        {
            MatButtonColor.Primary => PrimaryClassName,
            MatButtonColor.Accent  => AccentClassName,
            MatButtonColor.Warn    => WarnClassName,
            _                      => string.Empty,
        };
    }

    /// <summary>
    /// Lightweight ripple renderer attached directly to a host element.
    /// Uses UI Toolkit custom painting on all supported Unity versions.
    /// </summary>
    internal sealed class MatRippleController
    {
        private readonly VisualElement _host;

        private const float DurationSeconds = 0.45f;
        private const float BaseAlpha = 0.18f;
        private const float ShadowCornerRadius = 4f;

        private readonly List<RippleState> _ripples = new();
        private bool _animationScheduled;

        public bool DisableRipple { get; set; }

        public MatRippleController(VisualElement host)
        {
            _host = host;

            _host.generateVisualContent += OnGenerateVisualContent;
            _host.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.RegisterCallback<DetachFromPanelEvent>(_ => CancelAllRipples());
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (DisableRipple || evt.button != 0 || !_host.enabledInHierarchy)
            {
                return;
            }

            var size = _host.contentRect.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            var center = _host.WorldToLocal(evt.position);
            var maxRadius = CalculateMaxRadius(center, size);
            if (maxRadius <= 0f)
            {
                return;
            }

            _ripples.Add(new RippleState(center, maxRadius, GetCurrentTimeSeconds()));
            _host.MarkDirtyRepaint();
            EnsureAnimationLoop();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            DrawRaisedButtonShadow(painter);

            if (DisableRipple || _ripples.Count == 0)
            {
                return;
            }

            var now = GetCurrentTimeSeconds();
            var tint = ResolveRippleColor();

            for (var i = 0; i < _ripples.Count; i++)
            {
                var ripple = _ripples[i];
                var progress = Mathf.Clamp01((float)((now - ripple.StartTimeSeconds) / DurationSeconds));
                var easedProgress = EaseOutCubic(progress);
                var radius = Mathf.LerpUnclamped(0f, ripple.MaxRadius, easedProgress);
                var alpha = BaseAlpha * (1f - progress) * (1f - progress);
                if (alpha <= 0f || radius <= 0f)
                {
                    continue;
                }

                painter.fillColor = new Color(tint.r, tint.g, tint.b, tint.a * alpha);
                painter.BeginPath();
                painter.Arc(ripple.Center, radius, new Angle(0f), new Angle(360f), ArcDirection.Clockwise);
                painter.Fill(FillRule.NonZero);
            }
        }

        private void EnsureAnimationLoop()
        {
            if (_animationScheduled)
            {
                return;
            }

            _animationScheduled = true;
            ScheduleNextAnimationFrame();
        }

        private void ScheduleNextAnimationFrame()
        {
            _host.schedule.Execute(() =>
            {
                _animationScheduled = false;
                TickRipples();

                if (_ripples.Count > 0 && _host.panel != null && !DisableRipple)
                {
                    EnsureAnimationLoop();
                }
            });
        }

        private void TickRipples()
        {
            if (_ripples.Count == 0)
            {
                return;
            }

            var now = GetCurrentTimeSeconds();
            for (var i = _ripples.Count - 1; i >= 0; i--)
            {
                var age = now - _ripples[i].StartTimeSeconds;
                if (age >= DurationSeconds)
                {
                    _ripples.RemoveAt(i);
                }
            }

            _host.MarkDirtyRepaint();
        }

        private void CancelAllRipples()
        {
            if (_ripples.Count == 0)
            {
                return;
            }

            _ripples.Clear();
            _host.MarkDirtyRepaint();
        }

        private void DrawRaisedButtonShadow(Painter2D painter)
        {
            if (_host is not MatButton button || button.Variant != MatButtonVariant.Raised)
            {
                return;
            }

            var rect = _host.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            DrawRoundedRect(painter, new Rect(rect.x + 2f, rect.y + 4f, rect.width - 4f, rect.height - 5f), 6f, new Color(0f, 0f, 0f, 0.12f));
            DrawRoundedRect(painter, new Rect(rect.x + 1f, rect.y + 2f, rect.width - 2f, rect.height - 3f), 5f, new Color(0f, 0f, 0f, 0.18f));
            DrawRoundedRect(painter, new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 1f), ShadowCornerRadius, new Color(0f, 0f, 0f, 0.06f));
        }

        private float CalculateMaxRadius(Vector2 center, Vector2 size)
        {
            var topLeft = center.magnitude;
            var topRight = Vector2.Distance(center, new Vector2(size.x, 0f));
            var bottomLeft = Vector2.Distance(center, new Vector2(0f, size.y));
            var bottomRight = Vector2.Distance(center, size);
            return Mathf.Max(topLeft, topRight, bottomLeft, bottomRight);
        }

        private Color ResolveRippleColor()
        {
            var tint = _host.resolvedStyle.color;
            if (tint.a <= 0f)
            {
                tint = Color.white;
            }

            tint.a = 1f;
            return tint;
        }

        private static void DrawRoundedRect(Painter2D painter, Rect rect, float radius, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            radius = Mathf.Min(radius, rect.width * 0.5f, rect.height * 0.5f);
            var left = rect.xMin;
            var right = rect.xMax;
            var top = rect.yMin;
            var bottom = rect.yMax;

            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(left + radius, top));
            painter.LineTo(new Vector2(right - radius, top));
            painter.Arc(new Vector2(right - radius, top + radius), radius, new Angle(270f), new Angle(360f), ArcDirection.Clockwise);
            painter.LineTo(new Vector2(right, bottom - radius));
            painter.Arc(new Vector2(right - radius, bottom - radius), radius, new Angle(0f), new Angle(90f), ArcDirection.Clockwise);
            painter.LineTo(new Vector2(left + radius, bottom));
            painter.Arc(new Vector2(left + radius, bottom - radius), radius, new Angle(90f), new Angle(180f), ArcDirection.Clockwise);
            painter.LineTo(new Vector2(left, top + radius));
            painter.Arc(new Vector2(left + radius, top + radius), radius, new Angle(180f), new Angle(270f), ArcDirection.Clockwise);
            painter.ClosePath();
            painter.Fill(FillRule.NonZero);
        }

        private static float EaseOutCubic(float value)
        {
            var inv = 1f - value;
            return 1f - inv * inv * inv;
        }

        private static double GetCurrentTimeSeconds()
        {
            return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        }

        private readonly struct RippleState
        {
            public RippleState(Vector2 center, float maxRadius, double startTimeSeconds)
            {
                Center = center;
                MaxRadius = maxRadius;
                StartTimeSeconds = startTimeSeconds;
            }

            public Vector2 Center { get; }
            public float MaxRadius { get; }
            public double StartTimeSeconds { get; }
        }
    }

}
