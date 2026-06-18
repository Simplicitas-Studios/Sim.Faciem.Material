using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    internal sealed class MatRippleHost : VisualElement
    {
        public static readonly CustomStyleProperty<float> AlphaProperty = new("--mat-ripple-alpha");
        public static readonly CustomStyleProperty<float> DurationProperty = new("--mat-ripple-duration");

        private readonly VisualElement _eventHost;
        private readonly List<RippleState> _ripples = new();

        private bool _animationScheduled;
        private float _rippleAlpha = 0.18f;
        private float _rippleDuration = 0.45f;

        public MatRippleHost(VisualElement eventHost)
        {
            _eventHost = eventHost ?? throw new ArgumentNullException(nameof(eventHost));

            pickingMode = PickingMode.Ignore;

            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());

            _eventHost.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _eventHost.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            _eventHost.RegisterCallback<DetachFromPanelEvent>(_ => CancelAllRipples());
        }

        public Func<bool> DisableRippleEvaluator { get; set; }
        public Func<Color> TintColorProvider { get; set; }
        public Func<Rect, float> CornerRadiusProvider { get; set; }

        public float RippleAlpha => _rippleAlpha;
        public float RippleDuration => _rippleDuration;

        public override bool ContainsPoint(Vector2 localPoint)
        {
            var rect = contentRect;
            if (!rect.Contains(localPoint))
            {
                return false;
            }

            var radius = GetCornerRadius(rect);
            if (radius <= 0f)
            {
                return true;
            }

            return IsInsideRoundedRect(localPoint, rect, radius);
        }

        private bool DisableRipple => DisableRippleEvaluator?.Invoke() ?? false;

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            TryAssign(evt, AlphaProperty, ref _rippleAlpha);
            TryAssign(evt, DurationProperty, ref _rippleDuration);
            MarkDirtyRepaint();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (DisableRipple || evt.button != 0 || !_eventHost.enabledInHierarchy)
            {
                return;
            }

            var size = contentRect.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            var center = this.WorldToLocal(evt.position);
            var maxRadius = CalculateMaxRadius(center, size);
            if (maxRadius <= 0f)
            {
                return;
            }

            _ripples.Add(new RippleState(center, maxRadius, GetCurrentTimeSeconds()));
            MarkDirtyRepaint();
            EnsureAnimationLoop();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (DisableRipple || _ripples.Count == 0)
            {
                return;
            }

            var duration = Mathf.Max(0.0001f, RippleDuration);
            var now = GetCurrentTimeSeconds();
            var painter = context.painter2D;
            var tint = ResolveRippleColor();

            for (var i = 0; i < _ripples.Count; i++)
            {
                var ripple = _ripples[i];
                var progress = Mathf.Clamp01((float)((now - ripple.StartTimeSeconds) / duration));
                var easedProgress = EaseOutCubic(progress);
                var radius = Mathf.LerpUnclamped(0f, ripple.MaxRadius, easedProgress);
                var alpha = RippleAlpha * (1f - progress) * (1f - progress);
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
            _eventHost.schedule.Execute(() =>
            {
                _animationScheduled = false;
                TickRipples();

                if (_ripples.Count > 0 && _eventHost.panel != null && !DisableRipple)
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
            var duration = Mathf.Max(0.0001f, RippleDuration);
            for (var i = _ripples.Count - 1; i >= 0; i--)
            {
                if (now - _ripples[i].StartTimeSeconds >= duration)
                {
                    _ripples.RemoveAt(i);
                }
            }

            MarkDirtyRepaint();
        }

        private void CancelAllRipples()
        {
            if (_ripples.Count == 0)
            {
                return;
            }

            _ripples.Clear();
            MarkDirtyRepaint();
        }

        private Color ResolveRippleColor()
        {
            var tint = TintColorProvider?.Invoke() ?? _eventHost.resolvedStyle.color;
            if (tint.a <= 0f)
            {
                tint = Color.white;
            }

            tint.a = 1f;
            return tint;
        }

        private float GetCornerRadius(Rect rect)
        {
            var radius = CornerRadiusProvider?.Invoke(rect) ?? 0f;
            return Mathf.Min(radius, rect.width * 0.5f, rect.height * 0.5f);
        }

        private static void TryAssign(CustomStyleResolvedEvent evt, CustomStyleProperty<float> property, ref float field)
        {
            if (evt.customStyle.TryGetValue(property, out var value))
            {
                field = value;
            }
        }

        private static float CalculateMaxRadius(Vector2 center, Vector2 size)
        {
            var topLeft = center.magnitude;
            var topRight = Vector2.Distance(center, new Vector2(size.x, 0f));
            var bottomLeft = Vector2.Distance(center, new Vector2(0f, size.y));
            var bottomRight = Vector2.Distance(center, size);
            return Mathf.Max(topLeft, topRight, bottomLeft, bottomRight);
        }

        private static bool IsInsideRoundedRect(Vector2 point, Rect rect, float radius)
        {
            if (point.x >= rect.xMin + radius && point.x <= rect.xMax - radius)
            {
                return true;
            }

            if (point.y >= rect.yMin + radius && point.y <= rect.yMax - radius)
            {
                return true;
            }

            var topLeft = new Vector2(rect.xMin + radius, rect.yMin + radius);
            var topRight = new Vector2(rect.xMax - radius, rect.yMin + radius);
            var bottomLeft = new Vector2(rect.xMin + radius, rect.yMax - radius);
            var bottomRight = new Vector2(rect.xMax - radius, rect.yMax - radius);
            var radiusSquared = radius * radius;

            return (point - topLeft).sqrMagnitude <= radiusSquared
                || (point - topRight).sqrMagnitude <= radiusSquared
                || (point - bottomLeft).sqrMagnitude <= radiusSquared
                || (point - bottomRight).sqrMagnitude <= radiusSquared;
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
