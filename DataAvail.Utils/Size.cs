using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.Utils
{
    public struct Size
    {
        public Size(int Widht, int Height)
        {
            width = Widht;

            height = Height;
        }

        public int width;

        public int height;
    }
}
