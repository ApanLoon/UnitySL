
using System;

namespace Assets.Scripts.Appearance
{
    public class PolyFace
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }

        public int this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0:
                        return A;
                    case 1:
                        return B;
                    case 2:
                        return C;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (key)
                {
                    case 0:
                        A = value;
                        break;
                    case 1:
                        B = value;
                        break;
                    case 2:
                        C = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public PolyFace(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
