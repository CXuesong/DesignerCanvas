using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.ObjectModel
{
    /// <summary>
    /// Represents an entity (or an object, node, vertex) in the graph or diagram.
    /// There can be <see cref="Connection"/>s between entities.
    /// This class can be inherited by user to contain more information.
    /// </summary>
    public class Entity : INotifyPropertyChanged, IEntity
    {
        public event EventHandler BoundsChanged;

        private Point _Location;

        public Point Location
        {
            get { return _Location; }
            set
            {
                if (SetProperty(ref _Location, value))
                {
                    OnPropertyChanged(nameof(Left));
                    OnPropertyChanged(nameof(Top));
                    OnBoundsChanged();
                }
            }
        }

        private Size _Size;

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

        private ImageSource _Image;

        public ImageSource Image
        {
            get { return _Image; }
            set { SetProperty(ref _Image, value); }
        }

        public Rect Bounds => new Rect(_Location, _Size);

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
    }

    public interface IEntity : IGraphicalObject
    {
        double Left { get; set; }

        double Top { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        /// <summary>
        /// Gets the collection of the object's connectors.
        /// </summary>
        ConnectorCollection Connectors { get; }
    }
}
