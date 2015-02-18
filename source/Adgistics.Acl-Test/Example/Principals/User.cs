using System;
using System.Collections.Generic;

namespace Modules.Acl.Core
{
    public class User : IUser
    {
        private readonly List<string> _groupNames;

        public User(string name)
        {
            Identifier = Guid.NewGuid();
            _groupNames = new List<string>();
            Name = name;
        }

        public Guid Identifier { get; private set; }

        public string Name { get; private set; }

        public IEnumerable<string> Groups
        {
            get { return new List<string>(_groupNames); }
        }

        public void AssignToGroup(Group groupGuest)
        {
            if (false == _groupNames.Contains(groupGuest.Name))
            {
                _groupNames.Add(groupGuest.Name);
            }
        }
    }
}