﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="DockPanel"/>.
    /// </summary>
    public class DockPanelSplitter : Thumb
    {
        private Control _element;

        /// <summary>
        /// Defines the <see cref="Thickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<DockPanel, double>(nameof(Thickness), 4.0);

        /// <summary>
        /// Gets or sets the thickness (height or width, depending on orientation).
        /// </summary>
        /// <value>The thickness.</value>
        public double Thickness
        {
            get { return GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockPanelSplitter" /> class.
        /// </summary>
        public DockPanelSplitter()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this splitter is horizontal.
        /// </summary>
        public bool IsHorizontal
        {
            get
            {
                var dock = GetDock(this);
                return dock == Dock.Top || dock == Dock.Bottom;
            }
        }

        /// <inheritdoc/>
        protected override void OnDragDelta(VectorEventArgs e)
        {
            var dock = GetDock(this);
            if (IsHorizontal)
            {
                AdjustHeight(e.Vector.Y, dock);
            }
            else
            {
                AdjustWidth(e.Vector.X, dock);
            }
        }

        private DockPanel _panel;
        private Size _previousParentSize;

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _panel = Parent as DockPanel;

            _previousParentSize = _panel.Bounds.Size;

            _panel.LayoutUpdated += (sender, ee) =>
            {
                // if (!this.ProportionalResize)
                // {
                //     return;
                // }

                //var dp = this.Parent as DockPanel;
                //if (dp == null)
                //{
                //    return;
                //}

                if (_element.IsArrangeValid && _element.IsMeasureValid)
                {
                    var dSize = new Size(_panel.Bounds.Width / _previousParentSize.Width, _panel.Bounds.Height / _previousParentSize.Height);

                    if (!double.IsNaN(dSize.Width) && !double.IsInfinity(dSize.Width))
                    {
                        this.SetTargetWidth(_element.DesiredSize.Width - (_element.DesiredSize.Width * dSize.Width));
                    }

                    if (!double.IsInfinity(dSize.Height) && !double.IsNaN(dSize.Height))
                    {
                        this.SetTargetHeight(_element.DesiredSize.Height - (_element.DesiredSize.Height * dSize.Height));
                    }

                    _previousParentSize = _panel.Bounds.Size;
                }


            };

            UpdateHeightOrWidth();
            UpdateTargetElement();
        }

        private void AdjustHeight(double dy, Dock dock)
        {
            if (dock == Dock.Bottom)
            {
                dy = -dy;
            }
            SetTargetHeight(dy);
        }

        private void AdjustWidth(double dx, Dock dock)
        {
            if (dock == Dock.Right)
            {
                dx = -dx;
            }
            SetTargetWidth(dx);
        }

        private void SetTargetHeight(double dy)
        {
            double height = _element.DesiredSize.Height + dy;

            if (height < _element.MinHeight)
            {
                height = _element.MinHeight;
            }

            if (height > _element.MaxHeight)
            {
                height = _element.MaxHeight;
            }

            var panel = GetPanel();
            var dock = GetDock(this);
            if (dock == Dock.Top && height > panel.DesiredSize.Height - Thickness)
            {
                height = panel.DesiredSize.Height - Thickness;
            }

            _element.Height = height;
        }

        private void SetTargetWidth(double dx)
        {
            double width = _element.DesiredSize.Width + dx;

            if (width < _element.MinWidth)
            {
                width = _element.MinWidth;
            }

            if (width > _element.MaxWidth)
            {
                width = _element.MaxWidth;
            }

            var panel = GetPanel();
            var dock = GetDock(this);
            if (dock == Dock.Left && width > panel.DesiredSize.Width - Thickness)
            {
                width = panel.DesiredSize.Width - Thickness;
            }

            _element.Width = width;
        }

        private void UpdateHeightOrWidth()
        {
            if (IsHorizontal)
            {
                Height = Thickness;
                Width = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                PseudoClasses.Add(":horizontal");
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
                PseudoClasses.Add(":vertical");
            }
        }

        private Dock GetDock(Control control)
        {
            if (this.Parent is ContentPresenter presenter)
            {
                return DockPanel.GetDock(presenter);
            }
            return DockPanel.GetDock(control);
        }

        private Panel GetPanel()
        {
            if (this.Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is Panel panel)
                {
                    return panel;
                }
            }
            else
            {
                if (this.Parent is Panel panel)
                {
                    return panel;
                }
            }

            return _panel;
        }

        private void UpdateTargetElement()
        {
            if (this.Parent is ContentPresenter presenter)
            {
                if (!(presenter.GetVisualParent() is Panel panel))
                {
                    return;
                }

                int index = panel.Children.IndexOf(this.Parent);
                if (index > 0 && panel.Children.Count > 0)
                {
                    _element = (panel.Children[index - 1] as ContentPresenter).Child as Control;
                }
            }
            else
            {
                if (!(this.Parent is Panel panel))
                {
                    return;
                }

                int index = panel.Children.IndexOf(this);
                if (index > 0 && panel.Children.Count > 0)
                {
                    _element = panel.Children[index - 1] as Control;
                }
            }
        }
    }
}
