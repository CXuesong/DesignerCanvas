using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Undefined.DesignerCanvas.ObjectModel;

namespace WpfTestApplication
{
    /// <summary>
    /// Custom <see cref="IEntity"/> implementation test.
    /// </summary>
    class MyEntity : IEntity, INotifyPropertyChanged
    {
        private Rect _Bounds;

        public MyEntity()
        {
            Connectors = new ConnectorCollection(this, 1);
            Connectors[0].RelativePosition = new Point(0.5, 0.5);
        }

        /// <summary>
        /// Gets the bounding rectangle of the object.
        /// </summary>
        public Rect Bounds
        {
            get { return _Bounds; }
            set
            {
                _Bounds = value;
                BoundsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler BoundsChanged;

        /// <summary>
        /// Determines whether the object is in the specified region.
        /// </summary>
        public HitTestResult HitTest(Rect testRectangle)
        {
            var b = Bounds;
            if (testRectangle.Contains(b)) return HitTestResult.Inside;
            if (b.Contains(testRectangle)) return HitTestResult.Contains;
            if (b.IntersectsWith(testRectangle)) return HitTestResult.Intersects;
            return HitTestResult.None;
        }

        public double Left
        {
            get { return _Bounds.X; }
            set
            {
                _Bounds.X = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Bounds));
            }
        }

        public double Top
        {
            get { return _Bounds.Y; }
            set
            {
                _Bounds.Y = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Bounds));
            }
        }

        public double Width
        {
            get { return _Bounds.Width; }
            set
            {
                _Bounds.Width = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(Bounds));
            }
        }

        public double Height
        {
            get { return _Bounds.Height; }
            set
            {
                _Bounds.Height = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(Bounds));
            }
        }

        public bool Resizeable => false;

        /// <summary>
        /// Gets the collection of the object's connectors.
        /// </summary>
        public ConnectorCollection Connectors { get; }

        private double _Angle;

        public double Angle
        {
            get { return _Angle; }
            set
            {
                _Angle = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
