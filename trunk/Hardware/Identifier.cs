/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Text;

namespace OpenHardwareMonitor.Hardware {
  public class Identifier : IComparable<Identifier> {
    private string identifier;

    private static char SEPARATOR = '/';

    private static void CheckIdentifiers(string[] identifiers) {      
      foreach (string s in identifiers)
        if (s.Contains(" ") || s.Contains(SEPARATOR.ToString()))
          throw new ArgumentException("Invalid identifier");
    }

    public Identifier(params string[] identifiers) {
      CheckIdentifiers(identifiers);

      StringBuilder s = new StringBuilder();
      for (int i = 0; i < identifiers.Length; i++) {
        s.Append(SEPARATOR);
        s.Append(identifiers[i]);
      }
      this.identifier = s.ToString();
    }

    public Identifier(Identifier identifier, params string[] extensions) {
      CheckIdentifiers(extensions);

      StringBuilder s = new StringBuilder();
      s.Append(identifier.ToString());
      for (int i = 0; i < extensions.Length; i++) {
        s.Append(SEPARATOR);
        s.Append(extensions[i]);
      }
      this.identifier = s.ToString();
    }

    public override string ToString() {
      return identifier;
    }

    public override bool Equals(System.Object obj) {
      if (obj == null)
        return false;

      Identifier id = obj as Identifier;
      if (id == null)
        return false;

      return (identifier == id.identifier);
    }

    public override int GetHashCode() {
      return identifier.GetHashCode();
    }

    public int CompareTo(Identifier other) {
      if (other == null)
        return 1;
      else 
        return string.Compare(this.identifier, other.identifier, 
          StringComparison.Ordinal);
    }

    public static bool operator ==(Identifier id1, Identifier id2) {
      if (id1.Equals(null))
        return id2.Equals(null);
      else
        return id1.Equals(id2);
    }

    public static bool operator !=(Identifier id1, Identifier id2) {
      return !(id1 == id2);
    }

    public static bool operator <(Identifier id1, Identifier id2) {
      if (id1 == null)
        return id2 != null;
      else 
        return (id1.CompareTo(id2) < 0);
    }

    public static bool operator >(Identifier id1, Identifier id2) {
      if (id1 == null)
        return false;
      else 
        return (id1.CompareTo(id2) > 0);
    }  

  }
}
