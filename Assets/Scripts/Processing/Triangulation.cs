using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ImagePipeline
{
    public class Triangulation
    {
		public Material defaultMaterial;
		public List<int> Triangulate(ref List<List<Color>> shapes, bool outputtextfile, ref List<List<Vector3>> vertices, ref List<List<Vector2>> vertices2D)
        {
			List<int> indices = new List<int>();

			//Loop through all the shapes
            for (int i = 0; i < shapes.Count; i++)
            {
                if (outputtextfile)
                    Utilities.WriteVertices2DToText(vertices2D);

                Vector2[] tvertices2D = vertices2D[i].ToArray();
				Vector3[] tvertices = vertices[i].ToArray();

//				List<Vector3> tvertices = new List<Vector3>();//vertices[i].ToArray();
//				List<Color> tshape = shapes[i];
//
//				for (int k=0; k<tshape.Count; k++)
//				{
//					Color tcolor = tshape[k];
//					tvertices.Add(
//				}

                if (tvertices2D.Length > 2)
                {
                    //Triangulate
 
                    //Poly2Tri
                    Poly2TriAlg.Triangulate Poly2TriProcess = new Poly2TriAlg.Triangulate();
                    indices = Poly2TriProcess.Process(tvertices2D);
                }
            }
			return indices;
		}
    }
}

namespace Poly2TriAlg
{
    public class Triangulate
    {
        public List<int> Process(Vector2[] vertices2D)
        {
            Poly2Tri.Polygon tpoly = LoadDat(vertices2D);
            Poly2Tri.P2T.Triangulate(tpoly);

            List<int> indices = new List<int>();
            for (int j = 0; j < tpoly.Triangles.Count; j++)
            {
                Poly2Tri.DelaunayTriangle ttri = tpoly.Triangles[j];
                indices.Add(ttri.indices[0]);
                indices.Add(ttri.indices[1]);
                indices.Add(ttri.indices[2]);
            }

            return indices;
        }

        public static Poly2Tri.Polygon LoadDat(Vector2[] ivectors)
        {
            var points = new List<Poly2Tri.PolygonPoint>();
            foreach (Vector2 vec in ivectors)
            {
                points.Add(new Poly2Tri.PolygonPoint(vec.x, vec.y));
            }
            return new Poly2Tri.Polygon(points);
        }
    }
}