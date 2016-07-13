using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Undefined.DesignerCanvas
{
    public class PolyLineCanvasItem : INotifyPropertyChanged, IPolyLineCanvasItem
    {

        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value) == false)
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void OnBoundsChanged()
        {
            BoundsChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(Bounds));
        }

        private Rect _PointBounds;

        /// <summary>
        /// Gets the bounding rectangle of the object.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                UpdateArrange();
                if (_PointBounds.IsEmpty) return new Rect(_Left, _Top, 0, 0);
                return Rect.Offset(_PointBounds, _Left, _Top);
            }
        }

        public event EventHandler BoundsChanged;

        /// <summary>
        /// Determines whether the object is in the specified region.
        /// </summary>
        public HitTestResult HitTest(Rect testRectangle)
        {
            var inside = false;
            var outside = false;
            foreach (var p in Points)
            {
                if (testRectangle.Contains(p))
                    inside = true;
                else
                    outside = true;
                if (inside && outside) break;
            }
            if (inside && outside) return HitTestResult.Intersects;
            if (!inside) return HitTestResult.None;
            return HitTestResult.Contains;
        }

        private double _Left;

        public double Left
        {
            get { return _Left; }
            set { if (SetProperty(ref _Left, value)) OnBoundsChanged(); }
        }

        private double _Top;

        public double Top
        {
            get { return _Top; }
            set { if (SetProperty(ref _Top, value)) OnBoundsChanged(); }
        }

        private bool arrangeInvalidated = false;

        private void UpdateArrange()
        {
            if (arrangeInvalidated)
            {
                arrangeInvalidated = false;
                _PointBounds = Points.Aggregate(Rect.Empty, Rect.Union);
                OnBoundsChanged();
            }
        }

        /// <summary>
        /// Make ajustments to point coordinations and Left, Top,
        /// making all X, Y of points > 0.
        /// </summary>
        public void NormalizePositions()
        {
            UpdateArrange();
            if (_PointBounds.IsEmpty) return;
            var deltaVector = new Vector(_PointBounds.X, _PointBounds.Y);
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = Points[i] - deltaVector;
            }
            this.Left += deltaVector.X;
            this.Top += deltaVector.Y;
        }

        /// <summary>
        /// Notifies the item when user dragging the item.
        /// </summary>
        public virtual void NotifyUserDragging(double deltaX, double deltaY)
        {

        }

        public ObservableCollection<Point> Points { get; } = new ObservableCollection<Point>();

        public PolyLineCanvasItem()
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            arrangeInvalidated = true;
            // TODO Delayed execution.
            UpdateArrange();
        }

        public PolyLineCanvasItem(IEnumerable<Point> points) : this()
        {
            foreach (var p in points)
            {
                Points.Add(p);
            }
        }
    }
}
