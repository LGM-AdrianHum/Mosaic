using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace People
{
    public class DragScrollViewer : ScrollViewer
    {
        public bool IsDragging { get { return _isDragging; } }
        public bool DragEverywhere { get; set; }

        public DragScrollViewer()
        {

        }

        private double _friction = DEFAULT_FRICTION;
        public double Friction
        {
            get
            {
                return _friction;
            }
            set
            {
                _friction = Math.Min(Math.Max(value, MINIMUM_FRICTION), MAXIMUM_FRICTION);
            }
        }

        private Point _previousPreviousPoint;
        private Point _previousPoint;
        private Point _currentPoint;


        private bool _mouseDown = false;
        private bool _isDragging = false;

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            //e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CancelDrag(PreviousVelocity);
        }

        private object mouseDownSource;
        private Point mouseDownCoords;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source.GetType() != typeof(DragScrollViewer) && !DragEverywhere)
            {
                return;
            }

            mouseDownSource = e.Source;
            mouseDownCoords = e.GetPosition(this);

            _currentPoint = _previousPoint = _previousPreviousPoint = e.GetPosition(this);
            Momentum = new Vector(0, 0);
            BeginDrag();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.Source.GetType() != typeof(DragScrollViewer) && !IsDragging && !DragEverywhere)
            {
                return;
            }

            _currentPoint = e.GetPosition(this);
            if (_mouseDown && !_isDragging)
            {
                _isDragging = true;
                DragScroll();
            }
            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.Source.GetType() != typeof(DragScrollViewer) && !DragEverywhere)
            {
                return;
            }

            CancelDrag(PreviousVelocity);
            if (mouseDownSource != e.Source || e.GetPosition(this) != mouseDownCoords)
                e.Handled = true;
        }

        private void BeginDrag()
        {
            _mouseDown = true;
        }

        private void CancelDrag(Vector velocityToUse)
        {
            if (_isDragging)
                Momentum = velocityToUse;
            _isDragging = false;
            _mouseDown = false;
            Cursor = Cursors.Arrow;
        }


        protected void DragScroll()
        {
            if (_dragScrollTimer == null)
            {
                _dragScrollTimer = new DispatcherTimer();
                _dragScrollTimer.Tick += TickDragScroll;
                _dragScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)DRAG_POLLING_INTERVAL);
                _dragScrollTimer.Start();
            }
        }

        private void TickDragScroll(object sender, EventArgs e)
        {
            if (_isDragging)
            {
                if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled)
                    Cursor = Cursors.SizeWE;
                if (HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled)
                    Cursor = Cursors.SizeNS;
                //HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                GeneralTransform generalTransform = this.TransformToVisual(this);
                Point childToParentCoordinates = generalTransform.Transform(new Point(0, 0));
                Rect bounds = new Rect(childToParentCoordinates, this.RenderSize);

                if (bounds.Contains(_currentPoint))
                {
                    PerformScroll(PreviousVelocity);
                }

                if (!_mouseDown)
                {
                    CancelDrag(Velocity);
                }
                _previousPreviousPoint = _previousPoint;
                _previousPoint = _currentPoint;
            }
            else if (Momentum.Length > 0)
            {
                Momentum *= (1.0 - _friction / 4.0);
                PerformScroll(Momentum);
                /*if (Momentum.Length < 0.1)
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;*/
            }
            else
            {
                if (_dragScrollTimer != null)
                {
                    _dragScrollTimer.Tick -= TickDragScroll;
                    _dragScrollTimer.Stop();
                    _dragScrollTimer = null;
                    //VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        private void CancelDrag()
        {
            _isDragging = false;
            Momentum = Velocity;
        }

        private void PerformScroll(Vector displacement)
        {
            var verticalOffset = Math.Max(0.0, VerticalOffset - displacement.Y);
            ScrollToVerticalOffset(verticalOffset);

            var horizontalOffset = Math.Max(0.0, HorizontalOffset - displacement.X);
            ScrollToHorizontalOffset(horizontalOffset);
        }

        private const double DRAG_POLLING_INTERVAL = 10; // milliseconds
        private DispatcherTimer _dragScrollTimer = null;
        private const double DEFAULT_FRICTION = 0.2;
        private const double MINIMUM_FRICTION = 0.0;
        private const double MAXIMUM_FRICTION = 1.0;


        private Vector Momentum { get; set; }
        private Vector Velocity
        {
            get
            {
                return new Vector(_currentPoint.X - _previousPoint.X, _currentPoint.Y - _previousPoint.Y);
            }
        }
        // Using PreviousVelocity gives a smoother, better feeling as it leaves out any last frame momentum changes
        private Vector PreviousVelocity
        {
            get
            {
                return new Vector(_previousPoint.X - _previousPreviousPoint.X, _previousPoint.Y - _previousPreviousPoint.Y);
            }
        }

        private class Vector
        {
            public double Length { get { return Math.Sqrt(X * X + Y * Y); } }
            public Vector(double x, double y)
            {
                X = x;
                Y = y;
            }

            public static Vector operator *(Vector vector, double scalar)
            {
                return new Vector(vector.X * scalar, vector.Y * scalar);
            }

            public double X { get; set; }
            public double Y { get; set; }

        }
    }
}
