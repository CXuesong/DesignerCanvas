using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Undefined.DesignerCanvas.ObjectModel
{
    public class Connector : INotifyPropertyChanged
    {
        private readonly Entity _Owner;
        //private readonly ConnectionCollection _Connections;

        internal Connector(Entity owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _Owner = owner;
            //_Connections = new ConnectionCollection(this);
        }

        //public ConnectionCollection Connections => _Connections ;

        // Right now it seems no use to keep track of connections.

        public Entity Owner => _Owner;


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
        public Point AbsolutePosition =>
            new Point(_Owner.Left + _Owner.Width*_RelativePosition.X,
                _Owner.Top + _Owner.Height*_RelativePosition.Y);

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
