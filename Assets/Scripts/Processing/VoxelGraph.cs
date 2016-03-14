using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voxel {
    public int x, y, z, width, length, height;

    public Voxel(int ix, int iy, int iz, int iwidth, int ilength, int iheight)
    {
        x = ix;
        y = iy;
        z = iz;
        width = iwidth;
        length = ilength;
        height = iheight;
    }
}

public class VoxelGraph
{
    public List<Voxel> voxels;
    public List<int> connections;
}
