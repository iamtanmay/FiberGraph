using UnityEngine;
using ImagePipeline;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace StackPipeline
{
    public class StackPipeline : MonoBehaviour
    {
        public bool debugmode = false;
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

        public int blurwidth = 2;

        public int red_threshold = -1, green_threshold = -1, blue_threshold = -1; 

        public string filetypefilter = ".jpeg";

        public List<List<Vector3>> vertices;
        public List<List<Vector2>> vertices2D;

        //Shape center, bounds
        List<List<Vector2>> shapecenters;
        List<List<float>> shaperadii;

        //3D shapes
        List<List<Vector3>> shapes3D;

        List<List<Vector3>> shapes;
        List<Vector2> shape3Dcenters;
        List<int> shape3Dradii;
         

        public bool image_loaded = false, outputtextfile = false;

        public int counter, pixels_perframe;

        public int height, width;
        public string fullpath = "";
        public string path = "\\Stack";
        public List<string> filenames;
        public Texture2D[] heightmaps;
        public List<Color[]> imagepixels;
        public List<List<List<Vector3>>> slices;

        string[] files;
        string pathPreFix;

        public int slicecounter = 0;

        public int stage = -1;

        public void Start()
        {
            //Load all images in Folder
            slices = new List<List<List<Vector3>>>();
            shapecenters = new List<List<Vector2>>();
            shaperadii = new List<List<float>>();
            filenames = new List<string>();

			ImageUtilities.LoadImages(path, ref heightmaps, ref filenames, filetypefilter, debugmode);
            
			//Debug.Log(

			imagepixels = new List<Color[]>();
            stage = -1;
        }

        public void Update()
        {
            Debug.Log(stage);

            switch (stage)
            {
                case -1: StartCoroutine(Threshold()); break;
                case 0: StartCoroutine(GetShapes()); break;
                case 1: StartCoroutine(CalculateBounds()); break;
                //case -1: StartCoroutine(Threshold()); break;
                default: break;
            }
            //Add shapes into 3D shapes, while removing duplicate pixels, starting bottom up
            //if (stage == 2)
                //StartCoroutine(Create3DShapes());
    
            //if (stage == 1)
                //StartCoroutine(CalculateBounds());
            
            //Save Shape3Ds to disk
        }

        public IEnumerator Create3DShapes()
        {
            shapes3D = new List<List<Vector3>>();

            //Match consecutive slices
            for (int i = 0; i < slices.Count - 1; i++)
            {
                //Follow a shape through the stack
                for (int j = 0; j < slices[i].Count; i++)
                {
                    List<Vector3> tshape = slices[i][j];
                    List<Vector3> tshape3D = new List<Vector3>();
                    AddSliceToShape3D(ref tshape3D, ref tshape, 0);
                    
                    for (int k = i + 1; k < slices.Count; k++)
                    {
                        //Loop through the shapes
                        for (int l = 0; l < slices[k].Count; l++)
                        {
                            //If shapes overlap, then add to Shape3D
                            if (ShapeBoundOverlap(shapecenters[i][j], shapecenters[k][l], shaperadii[i][j], shaperadii[k][l], ref tshape, slices[k][l]))
                            {
                                List<Vector3> tshape2 = new List<Vector3>();
                                tshape2 = slices[k][l];
                                AddSliceToShape3D(ref tshape3D, ref tshape2, k);
                            }
                        }
                    }
                    yield return null;
                }
            }
            stage++;

            if (debugmode)
                Debug.Log(stage);
            yield return null;
        }

        public void AddSliceToShape3D(ref List<Vector3> ishape3D, ref List<Vector3> ishape, int ilevelofslice)
        {
            //Check all pixels of the slice to see if there is overlap
            for (int j = 0; j < ishape.Count; j++)
            {
                bool texists = false;
                int theight = (int) ishape[j].z + ilevelofslice * slice_height;

                for (int i = 0; i < ishape3D.Count; i++)
                {
                    int x = (int)ishape3D[i].x;
                    int y = (int)ishape3D[i].y;
                    int z = (int)ishape3D[i].z;
                    
                    //If pixel overlaps, check to see the intensity height + level height is same
                    //if yes, change texists flag
                    if (ishape[j].x == x && ishape[j].y == y)
                    {
                        if (theight == z)
                            texists = true;
                    }
                }

                if (!texists)
                    ishape3D.Add(new Vector3((int)ishape[j].x, (int)ishape[j].y, theight));
            }
        }

        //Radius is farthest pixel. If the radii don't overlap, shapes are definitely seperate. In case of radii overlap, 
        //every pixel has to be checked to see if there is any overlap
        public bool ShapeBoundOverlap(Vector3 icenter1, Vector3 icenter2, float iradius1, float iradius2, ref List<Vector3> ishape1, List<Vector3> ishape2)
        {
            bool treturn = false;
            treturn = (icenter1 - icenter2).magnitude < (iradius1 + iradius2);

            //Check to see if any pixel overlap
            if (treturn)
            {
                for (int i = 0; i < ishape1.Count; i++)
                {
                    for (int j = 0; j < ishape2.Count; j++)
                    {
                        if (ishape1[i].x == ishape2[j].x && ishape1[i].y == ishape2[j].y)
                            return true;
                    }
                }
            }

            return treturn;
        }

        //Calculate the centers and radii of the point clouds
        public IEnumerator CalculateBounds()
        {
            for (int i = 0; i < slices.Count; i++)
            {
                List<Vector2> tcenters = new List<Vector2>();
                List<float> tradii = new List<float>();
                for (int j = 0; j < slices[i].Count; j++)
                {
                    Vector2 tcenter= new Vector2();
                    float tradius = 0f;
                    CalculateBounds(slices[i][j], ref tcenter, ref tradius);

                    tcenters.Add(tcenter);
                    tradii.Add(tradius);
                }
                shapecenters.Add(tcenters);
                shaperadii.Add(tradii);
                yield return null;
            }
            stage++;
            yield return null;
        }

        //Calculates the center and radius to farthest point
        public void CalculateBounds(List<Vector3> icloud, ref Vector2 ocenter, ref float oradius)
        {
            ocenter = new Vector2(0, 0);
            oradius = 0f;

            if (icloud.Count > 0)
            {
                for (int i = 0; i < icloud.Count; i++)
                {
                    ocenter = ocenter + new Vector2(icloud[i].x, icloud[i].y);
                }
                ocenter = ocenter / icloud.Count;
                
                for (int i = 0; i < icloud.Count; i++)
                {
                    float tradius = (ocenter - new Vector2(icloud[i].x, icloud[i].y)).magnitude;
                    if (tradius > oradius)
                        oradius = tradius;
                }
            }
        }

        //Threshold images and create intensity maps
        public IEnumerator Threshold()
        {
            while (slicecounter < heightmaps.Length)
            {
                if (debugmode)
                    Debug.Log("Slice " + slicecounter + " thresholding");

                //Get all imagepixels
                Color[] imgpixels = heightmaps[slicecounter].GetPixels();

                //Blur pass
                imgpixels = ImageUtilities.Blur(imgpixels, heightmaps[slicecounter].width, heightmaps[slicecounter].height, blurwidth, filenames[slicecounter], debugmode);

                //Threshold based on total intensity or individual color intensity
                if (pixel_threshold != -1)
                {
                    imgpixels = ImageUtilities.Threshold(imgpixels, pixel_threshold, heightmaps[slicecounter].width, heightmaps[slicecounter].height, filenames[slicecounter], debugmode);
                }
                else if (red_threshold != -1 && green_threshold != -1 && blue_threshold != -1)
                {
                    imgpixels = ImageUtilities.Threshold(imgpixels, red_threshold, green_threshold, blue_threshold, heightmaps[slicecounter].width, heightmaps[slicecounter].height, filenames[slicecounter], debugmode);
                }
                else
                {
                    Debug.Log("No threshold intensity given !");
                }
                
                //Intensity map
                imgpixels = ImageUtilities.IntensityMap(imgpixels, heightmaps[slicecounter].width, heightmaps[slicecounter].height, filenames[slicecounter], debugmode);
                
                imagepixels.Add(imgpixels);

                slicecounter++;

                if (slicecounter == heightmaps.Length)
                    stage++;
                yield return null;
            }
            yield return null;
        }

        public IEnumerator GetShapes()
        {
            int tslicecounter = 0;
            while (tslicecounter < heightmaps.Length)
            {
                if (debugmode)
                    Debug.Log("Slice " + tslicecounter + " shapes");

				//Intensity map
				Color[] imgpixels = imagepixels[tslicecounter];

                //Process image into shapes              
                Shapes shapeprocessing = new Shapes(maxShapes);
                //List<string> zvalues = new List<string>();
                List<Vector3> tvertices = new List<Vector3>();
                List<List<bool>> checkedflags = new List<List<bool>>();
                List<List<bool>> pixelflags = new List<List<bool>>();
                List<List<Color>> shapes = new List<List<Color>>();

                height = heightmaps[tslicecounter].height;
                width = heightmaps[tslicecounter].width;

				for (int j = 0; j < height; j++)
				{
					checkedflags.Add(new List<bool>());
					pixelflags.Add(new List<bool>());
					for (int i = 0; i < width; i++)
					{
						checkedflags[j].Add(false);
						pixelflags[j].Add(false);
					}
				}

                //Check pixels and add to known fiber pixels
                int tpixelcount = 0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
						//Check if intensity value from intensity map is above threshold
						//R G B values are all same intenstity
						if (imgpixels[i + j * width].r * 255 > pixel_threshold)
						{
	                        pixelflags[j][i] = true;
    	                    tpixelcount++;
						}
                    }
                }

				Debug.Log ("tpixelcount " + tpixelcount);

                //shapeprocessing.pixelflags = pixelflags;
                //shapeprocessing.checkedflags = checkedflags;
                //shapeprocessing.minPixelInShapeThreshold = minPixelInShapeThreshold;
                //shapeprocessing.pixel_threshold = pixel_threshold;
                ExtractShapes(ref shapes, ref vertices, ref vertices2D, ref pixelflags, ref checkedflags, ref imgpixels);

                if (debugmode)
                {
                    Debug.Log("vertices" + vertices.Count);
                    //Print out shapes
                    ImageUtilities.DebugSaveShapesToDisk(ref vertices, filenames[tslicecounter]);
                }

                //slices.Add(vertices);

                counter = 0;
                tslicecounter++;

                if (tslicecounter == heightmaps.Length)
                    stage++;

                yield return null;
            }
            yield return null;
        }


        public void ExtractShapes(ref List<List<Color>> ishapes, ref List<List<Vector3>> ivertices,
            ref List<List<Vector2>> ivertices2D, ref List<List<bool>> ipixelflags, ref List<List<bool>> icheckedflags, ref Color[] iimgpixels)
        {
            ishapes = new List<List<Color>>();
            ivertices = new List<List<Vector3>>();
            ivertices2D = new List<List<Vector2>>();

            List<List<Vector2>> tpixels = new List<List<Vector2>>();

            int tcounter = 0;

            //Search neighbours to get list of shapes
            //Height
            for (int i = 0; i < ipixelflags.Count; i++)
            {
                //Width
                for (int j = 0; j < ipixelflags[i].Count; j++)
                {
                    if (ipixelflags[i][j])
                    {
                        if (!icheckedflags[i][j])
                        {
                            tcounter++;
                            List<Vector2> tpixelshape = new List<Vector2>();
							Debug.Log(new Vector2(i, j));
                            tpixelshape.AddRange(RecurseShapePixels(new Vector2(i, j), ref ipixelflags, ref icheckedflags));
                            Debug.Log(tpixelshape.Count);

                            if (tpixelshape.Count > 0)
                                tpixels.Add(tpixelshape);
                        }
                    }
                }
            }

            //Debug.Log(tpixels.Count);

            //Copy over the actual pixel colors
            //Looping over each shape
            for (int i = 0; i < tpixels.Count; i++)
            {
                List<Color> tshape = new List<Color>();
                List<Vector3> tvertices = new List<Vector3>();
                List<Vector2> tvertices2D = new List<Vector2>();


                if (tpixels[i].Count >= minPixelInShapeThreshold)
                {
                    for (int j = 0; j < tpixels[i].Count; j++)
                    {
                        Vector2 tcoord = tpixels[i][j];

                        Color tpixel = iimgpixels[(int)tcoord.x * height + (int)tcoord.y];

                        int theight = (int)(255f - 255f * tpixel.g);
                        //Debug.Log(tpixel.b);
                        //Debug.Log(theight);
                        //float rand1 = RandomDecimal(j%2);
                        //rand1 = rand1 * rand1 * rand1 * rand1 / 10000;
                        //float rand2 = rand1 + RandomDecimal((int)tcoord.x);
                        Vector3 tvector3 = new Vector3(tcoord.x, tcoord.y, pixel_height * theight);
                        //Debug.Log(tvector3);
                        tvertices.Add(tvector3);
                        //tvector3 = new Vector3(tcoord.x, tcoord.y, pixel_height * tpixel.b);
                        tvertices2D.Add(new Vector2(tvector3.x, tvector3.y));
                        tshape.Add(tpixel);
                    }
					if (debugmode)
	                    Debug.Log("Shape " + i + " has " + tvertices2D.Count);
                    ishapes.Add(tshape);
                    ivertices.Add(tvertices);
                    ivertices2D.Add(tvertices2D);
					if (debugmode)
						Debug.Log(ivertices2D[0].Count);
                }
            }
            //Debug.Log("Shape Count" + shapes.Count);
        }

        //ipixel is the x, y coordinate of the pixel. Compare pixelflags to know which pixels exist, 
        //and checked flags to know which have been already compared.
        public List<Vector2> RecurseShapePixels(Vector2 ipixel, ref List<List<bool>> ipixelflags, ref List<List<bool>> icheckedflags)
        {
            List<Vector2> treturn = new List<Vector2>();

            int x = (int)ipixel.x, y = (int)ipixel.y;

            icheckedflags[x][y] = true;
            treturn.Add(ipixel);

            //MoveRightAndDown
            for (int i = 0; i < searchwidth; i++)
            {
                //Check right neighbour
                //Recurse right neighbour
				if (x + i < height && y < width)
                {
                    //If right neighbour exists and is not checked yet
                    if (ipixelflags[x + i][y])
                    {
                        if (!icheckedflags[x + i][y])
                        {
                            treturn.AddRange(RecurseShapePixels(new Vector2(x + i, y), ref ipixelflags, ref icheckedflags));
                        }
                    }
                }
                //No right neighbour
                else
                {
                }

                //Check bottom neighbour
                //Recurse bottom neighbour
				if (x < height && y + i < width)
                {
                    //If bottom neighbour exists and is not checked yet
                    if (ipixelflags[x][y + i])
                    {
                        if (!icheckedflags[x][y + i])
                        {
                            treturn.AddRange(RecurseShapePixels(new Vector2(x, y + i), ref ipixelflags, ref icheckedflags));
                        }
                    }
                }
                //No bottom neighbour
                else
                {
                }
            }

            return treturn;
        }


        //Extract shapes

        //Output to text file
        //Utilities.WriteVertices2DToText(vertices2D);


        //linerenderer.Init();

        //int shapesrendered = maxShapes;

        //if (shapesrendered < 0 || shapesrendered > shapes.Count)
        //    shapesrendered = shapes.Count;

        //for (int i = 0; i < shapesrendered; i++)
        //{
        //    if (outputtextfile)
        //        Utilities.WriteVertices2DToText(vertices2D);

        //    Vector2[] tvertices2D = vertices2D[i].ToArray();
        //    Vector3[] tvertices = vertices[i].ToArray();

        //    if (tvertices2D.Length > 2)
        //    {
        //        linerenderer.AddPointCloud(vertices[i], shapes[i]);
        //    }
        //}        
        
        
    }
}