using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Undefined.DesignerCanvas.ObjectModel
{
    /// <summary>
    /// The direction of the first segment of elbow connection
    /// connected to the specific connector.
    /// </summary>
    public enum ConnectorDirection
    {
        Horizontal = 0,
        Vertical
    }

    public class Connector : INotifyPropertyChanged
    {
        private readonly IEntity _Owner;
        //private readonly ConnectionCollection _Connections;

        internal Connector(IEntity owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _Owner = owner;
            //_Connections = new ConnectionCollection(this);
        }

        //public ConnectionCollection Connections => _Connections ;

        // Right now it seems no use to keep track of connections.

        public IEntity Owner => _Owner;


        private Point _RelativePosition;

        /// <summary>
        /// Gets/Sets the position the connector relative to the Owner.
        /// </summary>
        public Point RelativePosition
        {
            get { return _RelativePosition; }
            set { SetProperty(ref _RelativePosition, value); }
        }

        /// <summary>
        /// Gets the position of Connector, relative to the DesignerCanvas.
        /// </summary>
        public Point AbsolutePosition
        {
            get
            {
                // Assume the rotation center is the center of bounds.
                var width = _Owner.Width;
                var height = _Owner.Height;
                var angle = _Owner.Angle/180.0*Math.PI;
                var x = (_RelativePosition.X - 0.5)*width;
                var y = (_RelativePosition.Y - 0.5)*height;
                var sa = Math.Abs(angle) < 0.01 ? angle : Math.Sin(angle);
                var ca = Math.Abs(angle) < 0.01 ? 1 - angle*angle/2 : Math.Cos(angle);
                return new Point(_Owner.Left + (x*ca - y*sa) + width/2,
                    _Owner.Top + (x*sa + y*ca) + height/2);
            }
        }


        private ConnectorDirection _Direction;

        /// <summary>
        /// The direction of the first segment of elbow connection
        /// connected to this connector.
        /// </summary>
        public ConnectorDirection Direction
        {
            get { return _Direction; }
            set { SetProperty(ref _Direction, value); }
        }

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
    }
}
