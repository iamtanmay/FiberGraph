using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ImagePipeline
{
    public class ImagePipeline : MonoBehaviour
    {
        public LinesGL linerenderer;

        public int zwritestart = 0, zwriteend = 100;

        //Height of pixel intensity in nanometers rounded to nearest int
        public float pixel_height = 1;

		public int maxShapes = -1;

        //Width to search for neighbour
        public int searchwidth = 1;

        //Minimum number of pixels to make a shape
        public int minPixelInShapeThreshold = 3;

        //Height of slice in nanometers
        public int slice_height = 5000;

        public int pixel_threshold = 200;

        public Vector2[] debugvectors;

        public string filetypefilter = "*.jpeg";

        public Mesh mesh;
        public Vector2[] uvs;
        public int[] triangles;

        public Texture2D heightmap, image;

        public Material material;

        public List<List<Vector3>> vertices;
        public List<List<Vector2>> vertices2D;

        public bool image_loaded = false, outputtextfile = false;

        public gg.Mesh.Form1 delaunay;

        public int verticespermesh, mesh_generatelimit;

        public Mesh[] meshes;

        public int counter, pixels_perframe;
        public List<Color> imagepixels;

        public int height, width;

        List<List<Color>> shapes;

        public void Init()
        {
            vertices = new List<List<Vector3>>();
            vertices2D = new List<List<Vector2>>();

            mesh = new Mesh();
            delaunay = new gg.Mesh.Form1();
        }

        public void Start()
        {
			Init ();

			//Extract shapes
			Shapes shapeprocessing = new Shapes (maxShapes);
            shapeprocessing.zwritestart = zwritestart;
            shapeprocessing.zwriteend = zwriteend;
			shapeprocessing.ProcessImage (searchwidth, minPixelInShapeThreshold, heightmap, image, ref shapes, ref vertices, ref vertices2D);

			//Output to text file
			Utilities.WriteVertices2DToText (vertices2D);


			//Triangulate
			Triangulation triangulationprocessing = new Triangulation ();
			//List<int> indices = triangulationprocessing.Triangulate(ref shapes, outputtextfile, ref vertices, ref vertices2D);
			Debug.Log ("Shapes count" + shapes.Count);

            linerenderer.Init();

            int shapesrendered = maxShapes;

            if (shapesrendered < 0 || shapesrendered > shapes.Count)
                shapesrendered = shapes.Count;

            for (int i = 0; i < shapesrendered; i++) 
			{
				if (outputtextfile)
					Utilities.WriteVertices2DToText (vertices2D);

				Vector2[] tvertices2D = vertices2D [i].ToArray ();
				Vector3[] tvertices = vertices [i].ToArray ();

				if (tvertices2D.Length > 2) 
				{
                    linerenderer.AddPointCloud(vertices[i], shapes[i]);
                    //List<int> indices = SimpleTriangulate(tvertices2D);
                    //// Create mesh
                    //Mesh msh = new Mesh ();
                    //msh.vertices = tvertices;
                    //msh.triangles = indices.ToArray ();
                    //msh.RecalculateNormals ();
                    //msh.RecalculateBounds ();

                    //// Set up game object with mesh;

                    //GameObject tobj = new GameObject ();
                    //MeshRenderer trender = (MeshRenderer)tobj.AddComponent (typeof(MeshRenderer));
                    //trender.material = new Material (Shader.Find ("Diffuse"));
                    //MeshFilter filter = tobj.AddComponent (typeof(MeshFilter)) as MeshFilter;
                    //filter.mesh = msh;
				}
			}
		}
		
		void Update()
		{
		}

		public List<int> SimpleTriangulate(Vector2[] ivertices2D)
		{
			List<int> indices = new List<int>();
			Vector2 oldvector;

			for (int i=0; i<ivertices2D.Length - 2; i++)
			{
				oldvector = ivertices2D[i];

				if ((oldvector - ivertices2D[i+1]).magnitude < 2)
				{
					if ((ivertices2D[i+1] - ivertices2D[i+2]).magnitude < 2)
					{
						indices.Add(i);
						indices.Add(i+1);
						indices.Add(i+2);
					}
				}
			}
			return indices;
		}
	}
}