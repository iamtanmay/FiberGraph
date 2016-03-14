using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace ImagePipeline
{
    public class Shapes
    {
        public List<string> zvalues;

        public int zwritestart = 0, zwriteend = 100;

		public int maxShapes = -1;

        //Height of pixel intensity in nanometers rounded to nearest int
        public float pixel_height = 20f; //5000/255 

        //Width to search for neighbour
        public int searchwidth = 1;

        //Minimum number of pixels to make a shape
        public int minPixelInShapeThreshold = 3;
        public int pixel_threshold = 200;

        public List<List<bool>> pixelflags, checkedflags;
        public List<Color> imagepixels;
        public Color[] imgpixels, colorpixels;

        public int height, width;
        public int counter;

        public bool image_loaded = false;

		public Shapes()
		{
		}

		public Shapes(int imax)
		{
			maxShapes = imax;
		}

		public float RandomDecimal(int i)
		{
			System.Random r = new System.Random();
			return (r.Next (i, 100+i));
		}

        public void ExtractShapes(ref List<List<Color>> shapes, ref List<List<Vector3>> vertices,
            ref List<List<Vector2>> vertices2D)
        {
            shapes = new List<List<Color>>();
            vertices = new List<List<Vector3>>();
            vertices2D = new List<List<Vector2>>();

            List<List<Vector2>> tpixels = new List<List<Vector2>>();

            int tcounter = 0;

            //Search neighbours to get list of shapes
            //Height
            for (int i = 0; i < pixelflags.Count; i++)
            {
                //Width
                for (int j = 0; j < pixelflags[i].Count; j++)
                {
                    if (pixelflags[i][j])
                    {
                        if (!checkedflags[i][j])
                        {
                            tcounter++;
                            List<Vector2> tpixelshape = new List<Vector2>();
                            tpixelshape.AddRange(RecurseShapePixels(new Vector2(i, j)));
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

                        Color tpixel = imgpixels[(int)tcoord.x * height + (int)tcoord.y];

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
                    Debug.Log("Shape " + i + " has " + tvertices2D.Count);
                    shapes.Add(tshape);
                    vertices.Add(tvertices);
                    vertices2D.Add(tvertices2D);
					Debug.Log (vertices2D[0].Count);
                }
            }
            //Debug.Log("Shape Count" + shapes.Count);
        }

        //ipixel is the x, y coordinate of the pixel. Compare pixelflags to know which pixels exist, 
        //and checked flags to know which have been already compared.
        public List<Vector2> RecurseShapePixels(Vector2 ipixel)
        {
            List<Vector2> treturn = new List<Vector2>();

            int x = (int)ipixel.x, y = (int)ipixel.y;

            checkedflags[x][y] = true;
            treturn.Add(ipixel);

            //MoveRightAndDown
            for (int i = 0; i < searchwidth; i++)
            {
                //Check right neighbour
                //Recurse right neighbour
                if (x + i < width && y < height)
                {
                    //If right neighbour exists and is not checked yet
                    if (pixelflags[x + i][y])
                    {
                        if (!checkedflags[x + i][y])
                        {
                            treturn.AddRange(RecurseShapePixels(new Vector2(x + i, y)));
                        }
                    }
                }
                //No right neighbour
                else
                {
                }

                //Check bottom neighbour
                //Recurse bottom neighbour
                if (x < width && y + i < height)
                {
                    //If bottom neighbour exists and is not checked yet
                    if (pixelflags[x][y + i])
                    {
                        if (!checkedflags[x][y + i])
                        {
                            treturn.AddRange(RecurseShapePixels(new Vector2(x, y + i)));
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

        public void ProcessImage(int isearchwidth, int iminPixelInShapeThreshold, Texture2D heightmap, Texture2D image, ref List<List<Color>> shapes, ref List<List<Vector3>> vertices,
            ref List<List<Vector2>> vertices2D)
        {
            zvalues = new List<string>();
            searchwidth = isearchwidth;
            minPixelInShapeThreshold = iminPixelInShapeThreshold;

            imgpixels = heightmap.GetPixels();
            colorpixels = image.GetPixels();

            List<Vector3> tvertices = new List<Vector3>();
            checkedflags = new List<List<bool>>();
            pixelflags = new List<List<bool>>();
            height = heightmap.height;
            width = heightmap.width;
            imagepixels = new List<Color>(); 

            shapes = new List<List<Color>>();

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

            //Check pixels, if above theshold, assign a height according to intensity and add to known fiber pixels
            int tpixelcount = 0;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Color tpixel = imgpixels[i + j * width];
                    int z = (int)(255f * tpixel.b);

                    zvalues.Add("" + z);
                    
                    //Debug.Log(z);

                    //Filter threshold
                    if (z < pixel_threshold)
                    {
                        pixelflags[i][j] = true;
                        tpixelcount++;
                    }
                }
            }

            //Utilities.WriteToText(zvalues.GetRange(zwritestart, zwriteend));

            //Debug.Log("tpixelcount " + tpixelcount);
            ExtractShapes(ref shapes, ref vertices, ref vertices2D);

            counter = 0;
        }
    }
}