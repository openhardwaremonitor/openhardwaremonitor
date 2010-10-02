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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace OpenHardwareMonitor.Utilities {
  public class IconFactory {

    private struct BITMAPINFOHEADER {
      public uint Size;
      public int Width;
      public int Height;
      public ushort Planes;
      public ushort BitCount;
      public uint Compression;
      public uint SizeImage;
      public int XPelsPerMeter;
      public int YPelsPerMeter;
      public uint ClrUsed;
      public uint ClrImportant;

      public BITMAPINFOHEADER(int width, int height, int bitCount) {
        this.Size = 40;
        this.Width = width;
        this.Height = height;
        this.Planes = 1;
        this.BitCount = (ushort)bitCount;
        this.Compression = 0;
        this.SizeImage = 0;
        this.XPelsPerMeter = 0;
        this.YPelsPerMeter = 0;
        this.ClrUsed = 0;
        this.ClrImportant = 0;
      }

      public void Write(BinaryWriter bw) {
        bw.Write(Size);
			  bw.Write(Width);
			  bw.Write(Height);
			  bw.Write(Planes);
			  bw.Write(BitCount);
			  bw.Write(Compression);
			  bw.Write(SizeImage);
			  bw.Write(XPelsPerMeter);
			  bw.Write(YPelsPerMeter);
			  bw.Write(ClrUsed);
			  bw.Write(ClrImportant);
      }
    }

    private struct ICONIMAGE {
      public BITMAPINFOHEADER Header;
      public byte[] Colors;
      public byte[] XOR;
      public byte[] AND;

      public ICONIMAGE(int width, int height, byte[] colors) {
        this.Header = new BITMAPINFOHEADER(width, height << 1, 
          (8 * colors.Length) / (width * height));
        this.Colors = colors;
        int maskSize = (width * height) >> 3;
        this.XOR = new byte[maskSize];
        this.AND = new byte[maskSize];
      }

      public void Write(BinaryWriter bw) {
        Header.Write(bw);
        int stride = Header.Width << 2;
        for (int i = (Header.Height >> 1) - 1; i >= 0; i--)
          bw.Write(Colors, i * stride, stride);
        bw.Write(XOR);        
        bw.Write(AND);
      }
    }

    private struct ICONDIRENTRY {
      public byte Width;
      public byte Height;
      public byte ColorCount;
      public byte Reserved;
      public ushort Planes;
      public ushort BitCount;
      public uint BytesInRes;
      public uint ImageOffset;

      public ICONDIRENTRY(ICONIMAGE image, int imageOffset) {
        this.Width = (byte)image.Header.Width;
        this.Height = (byte)(image.Header.Height >> 1);
        this.ColorCount = 0;
        this.Reserved = 0;
        this.Planes = image.Header.Planes;
        this.BitCount = image.Header.BitCount;
        this.BytesInRes = (uint)(image.Header.Size +
          image.Colors.Length + image.XOR.Length + image.AND.Length);
        this.ImageOffset = (uint)imageOffset;
      }

      public void Write(BinaryWriter bw) {
        bw.Write(Width);
        bw.Write(Height);
        bw.Write(ColorCount);
        bw.Write(Reserved);
        bw.Write(Planes);
        bw.Write(BitCount);
        bw.Write(BytesInRes);
        bw.Write(ImageOffset);
      }

      public uint Size {
        get { return 16; }
      }
    }

    private struct ICONDIR {
      public ushort Reserved;
      public ushort Type;
      public ushort Count;
      public ICONDIRENTRY[] Entries;

      public ICONDIR(ICONDIRENTRY[] entries) {
        this.Reserved = 0;
        this.Type = 1;
        this.Count = (ushort)entries.Length;
        this.Entries = entries;
      }

      public void Write(BinaryWriter bw) {
        bw.Write(Reserved);
        bw.Write(Type);
        bw.Write(Count);
        for (int i = 0; i < Entries.Length; i++)
          Entries[i].Write(bw);
      }

      public uint Size {
        get { return (uint)(6 + Entries.Length * 
          (Entries.Length > 0 ? Entries[0].Size : 0)); } 
      }
    }
	
    public static Icon Create(byte[] colors, int width, int height, 
      PixelFormat format) {
      if (format != PixelFormat.Format32bppArgb)
        throw new NotImplementedException();

      ICONIMAGE image = new ICONIMAGE(width, height, colors);
      ICONDIR dir = new ICONDIR(
        new ICONDIRENTRY[] { new ICONDIRENTRY(image, 0) } );
      dir.Entries[0].ImageOffset = dir.Size;

      Icon icon;
      using (BinaryWriter bw = new BinaryWriter(new MemoryStream())) {
				dir.Write(bw);
        image.Write(bw);

				bw.BaseStream.Position = 0;
        icon = new Icon(bw.BaseStream);
			}

      return icon;
    }

  }
}
