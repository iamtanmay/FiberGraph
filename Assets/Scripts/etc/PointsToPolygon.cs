using System.Collections.Generic;
using System.Linq;
using Poly2Tri;
using UnityEngine;

class PointsToPolygon 
{
    public static Polygon CreatePolygon( List<Vector3> ipoints ) 
    {
		List<PolygonPoint> points = new List<PolygonPoint>();
		for ( int i=0; i < ipoints.Count; i++)
        {
			points.Add( new PolygonPoint(ipoints[i].x, ipoints[i].y));//(xflip?-1:+1) * double.Parse(xy[0],CultureInfo.InvariantCulture), (yflip?-1:+1) * double.Parse(xy[1],CultureInfo.InvariantCulture) ) );
		}

		return new Polygon(points);
    }

    //public static List<Mesh> CreateMeshes(List<List<Vector3>> points) 
    //{
    //    List<Mesh> meshes = new List<Mesh>();
    //    List<Polygon> Polygons;
    //    Polygons = new List<Polygon>();

    //    for (int i=0; i<points.Count; i++)
    //        Polygons.Add(CreatePolygon(points[i]));

    //    //foreach ( var poly in Polygons ) try {
    //    //    P2T.Triangulate(poly);
            
    //    //    List<Vector
    //    //    foreach (var tri in poly.Triangles){
    //    //        meshes
    //    //    }
    //    //}
    //    //catch {
    //    //    Debug.Log("Poly2Tri ERROR !");
    //    //}		
    //}
}