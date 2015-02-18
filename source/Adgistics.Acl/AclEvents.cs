namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Events;

    // TODO: Provide additional events that may be helpful to integrators.
    // for example: an OnDeletedResource public event firer.
    // And also look at the naming of the methods / properties.  Think the
    // "On" part of names shouldn't be on the methods.
    /// <summary>
    ///   Events manager for the ACL system.
    /// </summary>
    public sealed class AclEvents : IDisposable
    {
        #region Fields

        /// <summary>
        ///   Tracks all delegates assigned to the GroupDeleted event.
        /// </summary>
        private readonly List<EventHandler<GroupDeletedEventArgs>> _groupDeletedDelegates;

        /// <summary>
        ///   Tracks all delegates assigned to the GroupPropChanged event.
        /// </summary>
        private readonly List<EventHandler<GroupPropChangedEventArgs>> _groupPropChangedDelegates;

        /// <summary>
        ///   Tracks all delegates assigned to the ResourceUnregistered event.
        /// </summary>
        private readonly List<EventHandler<ResourceUnregisteredEventArgs>> _resourceUnregisteredDelegates;

        /// <summary>
        ///   Tracks if the object has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///   The "on group deleted" event handlers;
        /// </summary>
        private EventHandler<GroupDeletedEventArgs> _groupDeleted;

        /// <summary>
        ///   The "on group prop changed" event handlers;
        /// </summary>
        private EventHandler<GroupPropChangedEventArgs> _groupPropChanged;

        /// <summary>
        ///   The "on resource unregistered" event handlers;
        /// </summary>
        private EventHandler<ResourceUnregisteredEventArgs> _resourceUnregistered;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="AclEvents"/> class.
        /// </summary>
        internal AclEvents()
        {
            _groupDeletedDelegates =
                new List<EventHandler<GroupDeletedEventArgs>>();
            _groupPropChangedDelegates =
                new List<EventHandler<GroupPropChangedEventArgs>>();
            _resourceUnregisteredDelegates =
                new List<EventHandler<ResourceUnregisteredEventArgs>>();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        ///   Event binder for actions to be executed when a group is deleted.
        /// </summary>
        public event EventHandler<GroupDeletedEventArgs> GroupDeleted
        {
            add
            {
                _groupDeleted += value;
                _groupDeletedDelegates.Add(value);
            }
            remove
            {
                _groupDeleted -= value;
                _groupDeletedDelegates.Remove(value);
            }
        }

        /// <summary>
        ///   Event binder for actions to be executed when a group property
        ///   is changed.
        /// </summary>
        public event EventHandler<GroupPropChangedEventArgs> GroupPropChanged
        {
            add
            {
                _groupPropChanged += value;
                _groupPropChangedDelegates.Add(value);
            }
            remove
            {
                _groupPropChanged -= value;
                _groupPropChangedDelegates.Remove(value);
            }
        }

        /// <summary>
        ///   Event binder for actions to be executed when a resource is unregistered.
        /// </summary>
        public event EventHandler<ResourceUnregisteredEventArgs> ResourceUnregistered
        {
            add
            {
                _resourceUnregistered += value;
                _resourceUnregisteredDelegates.Add(value);
            }
            remove
            {
                _resourceUnregistered -= value;
                _resourceUnregisteredDelegates.Remove(value);
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        ///   Clears all the registered event handlers.
        /// </summary>
        /// 
        /// <exception cref="ObjectDisposedException">
        ///   If this instance has been disposed.
        /// </exception>
        /// 
        /// <remarks>
        ///   This is a utility method to aid testers.
        /// </remarks>
        public void ClearEventHandlers()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            //
            // Group deleted
            var groupDeletedHandlers = _groupDeletedDelegates.ToList();

            foreach (var eh in groupDeletedHandlers)
            {
                GroupDeleted -= eh;
            }

            _groupDeletedDelegates.Clear();

            //
            // Group prop changed
            var groupPropChangedHandlers = _groupPropChangedDelegates.ToList();

            foreach (var eh in groupPropChangedHandlers)
            {
                GroupPropChanged -= eh;
            }

            _groupPropChangedDelegates.Clear();

            //
            // Resource unregistered
            var resourceUnregisteredHandlers =
                _resourceUnregisteredDelegates.ToList();

            foreach (var eh in resourceUnregisteredHandlers)
            {
                ResourceUnregistered -= eh;
            }

            _resourceUnregisteredDelegates.Clear();
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, 
        ///   releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (false == _disposed)
            {
                ClearEventHandlers();
                _disposed = true;
            }
        }

        /// <summary>
        ///   Fires the <see cref="GroupDeleted"/> event handlers.
        /// </summary>
        internal void OnGroupDeleted(Group deleted)
        {
            if (_groupDeleted != null)
            {
                _groupDeleted(deleted, new GroupDeletedEventArgs(deleted));
            }
        }

        /// <summary>
        ///   Fires the <see cref="GroupPropChanged"/> event handlers.
        /// </summary>
        internal void OnGroupPropChanged(Group changed)
        {
            if (_groupPropChanged != null)
            {
                _groupPropChanged(changed, new GroupPropChangedEventArgs(changed));
            }
        }

        /// <summary>
        ///   Fires the <see cref="ResourceUnregistered"/> event handlers.
        /// </summary>
        internal void OnResourceUnregistered(ResourceId unregistered)
        {
            if (_resourceUnregistered != null)
            {
                _resourceUnregistered(unregistered,
                    new ResourceUnregisteredEventArgs(unregistered));
            }
        }

        #endregion Methods
    }
}