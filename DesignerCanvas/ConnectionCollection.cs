using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Undefined.DesignerCanvas
{
    public class ConnectionCollection : ObservableCollection<Connection>
    {
        private readonly Connector _Owner;

        public ConnectionCollection(Connector owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _Owner = owner;
        }

        internal Connector Owner => _Owner;

        protected override void InsertItem(int index, Connection item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            // Check if the connection has something to do with the owner connector.
            if (item.Source != _Owner && item.Sink != _Owner)
                throw new InvalidOperationException("Attempt to add an unaffiliated connection.");
            base.InsertItem(index, item);
        }
    }
}
