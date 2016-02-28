/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Hardware
{
    public class Identifier : IComparable<Identifier>
    {
        private const char Separator = '/';
        private readonly string identifier;

        public Identifier(params string[] identifiers)
        {
            CheckIdentifiers(identifiers);

            var s = new StringBuilder();
            for (var i = 0; i < identifiers.Length; i++)
            {
                s.Append(Separator);
                s.Append(identifiers[i]);
            }
            identifier = s.ToString();
        }

        public Identifier(Identifier identifier, params string[] extensions)
        {
            CheckIdentifiers(extensions);

            var s = new StringBuilder();
            s.Append(identifier);
            for (var i = 0; i < extensions.Length; i++)
            {
                s.Append(Separator);
                s.Append(extensions[i]);
            }
            this.identifier = s.ToString();
        }

        public int CompareTo(Identifier other)
        {
            if (other == null)
                return 1;
            return string.Compare(identifier, other.identifier,
                StringComparison.Ordinal);
        }

        private static void CheckIdentifiers(IEnumerable<string> identifiers)
        {
            foreach (var s in identifiers)
                if (s.Contains(" ") || s.Contains(Separator.ToString()))
                    throw new ArgumentException("Invalid identifier");
        }

        public override string ToString()
        {
            return identifier;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var id = obj as Identifier;
            if (id == null)
                return false;

            return (identifier == id.identifier);
        }

        public override int GetHashCode()
        {
            return identifier.GetHashCode();
        }

        public static bool operator ==(Identifier id1, Identifier id2)
        {
            if (id1.Equals(null))
                return id2.Equals(null);
            return id1.Equals(id2);
        }

        public static bool operator !=(Identifier id1, Identifier id2)
        {
            return !(id1 == id2);
        }

        public static bool operator <(Identifier id1, Identifier id2)
        {
            if (id1 == null)
                return id2 != null;
            return (id1.CompareTo(id2) < 0);
        }

        public static bool operator >(Identifier id1, Identifier id2)
        {
            if (id1 == null)
                return false;
            return (id1.CompareTo(id2) > 0);
        }
    }
}