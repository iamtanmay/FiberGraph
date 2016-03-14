using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GraphDefinition{

    public class IntVector2
    {
        public int x, y;
        public IntVector2()
        {
        }
        public IntVector2(int ix, int iy)
        {
            x = ix;
            y = iy;
        }
    }

    public class IntVector3
    {
        public int x, y, z;
        public IntVector3()
        {
        }
        public IntVector3(int ix, int iy, int iz)
        {
            x = ix;
            y = iy;
            z = iz;
        }
    }

    public class Fiber
    {
        public Vector3 start, end;
        public List<IntVector3> pixels;
        public float coeff1, coeff2, coeff3, coeff4, coeff5;
    }

    public class PixelGraph
    {
        public Fiber[] fibers;
    }

    public class StainFibers
    {
        public List<List<IntVector3>> pixels;
    }	
}
