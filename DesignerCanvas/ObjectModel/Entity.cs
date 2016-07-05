using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.ObjectModel
{
    /// <summary>
    /// Represents an entity (or an object, node, vertex) in the graph or diagram.
    /// There can be <see cref="Connection"/>s between entities.
    /// This class can be inherited by user to contain more information.
    /// </summary>
    public class Entity : INotifyPropertyChanged, IEntity, ISizeConstraint
    {
        public event EventHandler BoundsChanged;
        private ImageSource _Image;
        private bool _Resizeable = true;
        private Point _Location;
        private Size _Size;

        public Point Location
        {
            get { return _Location; }
            set
            {
                if (SetProperty(ref _Location, value))
                {
                    OnPropertyChanged(nameof(Left));
                    OnPropertyChanged(nameof(Top));
                    OnPropertyChanged(nameof(Bounds));
                    OnBoundsChanged();
                }
            }
        }


        public Size Size
        {
            get { return _Size; }
            set
            {
                if (SetProperty(ref _Size, value))
                {
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Bounds));
                    OnBoundsChanged();
                }
            }
        }

        public double Left
        {
            get { return _Location.X; }
            set
            {
                _Location.X = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Bounds));
                OnBoundsChanged();
            }
        }

        public double Top
        {
            get { return _Location.Y; }
            set
            {
                _Location.Y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Bounds));
                OnBoundsChanged();
            }
        }

        public double Width
        {
            get { return _Size.Width; }
            set
            {
                _Size.Width = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(Bounds));
                OnBoundsChanged();
            }
        }

        public double Height
        {
            get { return _Size.Height; }
            set
            {
                _Size.Height = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(Bounds));
                OnBoundsChanged();
            }
        }


        private double _Angle;

        public double Angle
        {
            get { return _Angle; }
            set
            {
                SetProperty(ref _Angle, value);
                OnPropertyChanged(nameof(Bounds));
                OnBoundsChanged();
            }
        }

        public ImageSource Image
        {
            get { return _Image; }
            set { SetProperty(ref _Image, value); }
        }

        public Rect Bounds
        {
            get
            {
                var angle = _Angle*Math.PI/180.0;
                var sa = Math.Abs(Math.Abs(angle) < 0.01 ? angle : Math.Sin(angle));
                var ca = Math.Abs(Math.Abs(angle) < 0.01 ? 1 - angle * angle / 2 : Math.Cos(angle));
                var centerX = _Location.X + _Size.Width/2;
                var centerY = _Location.Y + _Size.Height/2;
                // bounding rectangle
                var width = _Size.Width*ca + _Size.Height*sa;
                var height = _Size.Width*sa + _Size.Height*ca;
                return new Rect(centerX - width/2, centerY - height/2, width, height);
            }
        }

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

        public virtual bool Resizeable => _Resizeable;

        /// <summary>
        /// Gets the collection of the object's connectors.
        /// </summary>
        public ConnectorCollection Connectors { get; }

        #region PropertyNotifications

        public event PropertyChangedEventHandler PropertyChanged;

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

        #endregion

        public Entity()
        {
            Connectors = new ConnectorCollection(this, 4);
            Connectors[0].RelativePosition = new Point(0.5, 0);
            Connectors[1].RelativePosition = new Point(1, 0.5);
            Connectors[2].RelativePosition = new Point(0.5, 1);
            Connectors[3].RelativePosition = new Point(0, 0.5);
        }

        public Entity(float left, float top, float width, float height, ImageSource image)
            : this()
        {
            _Location = new Point(left, top);
            _Size = new Size(width, height);
            _Image = image;
        }

        protected virtual void OnBoundsChanged()
        {
            BoundsChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual double MinWidth => 10;

        public virtual double MinHeight => 10;
    }

    public interface IEntity : IGraphicalObject
    {
        double Left { get; set; }

        double Top { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        /// <summary>
        /// Angle of rotation, in degrees.
        /// </summary>
        double Angle { get; set; }

        bool Resizeable { get; }

        /// <summary>
        /// Gets the collection of the object's connectors.
        /// </summary>
        ConnectorCollection Connectors { get; }
    }

    /// <summary>
    /// Enables size constraint for a specific type of <see cref="IEntity"/> 。
    /// </summary>
    public interface ISizeConstraint
    {
        double MinWidth { get; }

        double MinHeight { get; }
    }
}
