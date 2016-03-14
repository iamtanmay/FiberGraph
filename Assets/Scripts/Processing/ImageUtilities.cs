// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ImagePipeline
{
    public class Blur
    {
        private float avgR = 0;
        private float avgG = 0;
        private float avgB = 0;
        private float avgA = 0;
        private float blurPixelCount = 0;

        public int radius = 2;
        public int iterations = 2;

        private Texture2D tex;

        public Texture2D FastBlur(Texture2D image, int radius, int iterations)
        {
            Texture2D tex = image;

            for (var i = 0; i < iterations; i++)
            {

                tex = BlurImage(tex, radius, true);
                tex = BlurImage(tex, radius, false);

            }

            return tex;
        }

        Texture2D BlurImage(Texture2D image, int blurSize, bool horizontal)
        {

            Texture2D blurred = new Texture2D(image.width, image.height);
            int _W = image.width;
            int _H = image.height;
            int xx, yy, x, y;

            if (horizontal)
            {
                for (yy = 0; yy < _H; yy++)
                {
                    for (xx = 0; xx < _W; xx++)
                    {
                        ResetPixel();

                        //Right side of pixel

                        for (x = xx; (x < xx + blurSize && x < _W); x++)
                        {
                            AddPixel(image.GetPixel(x, yy));
                        }

                        //Left side of pixel

                        for (x = xx; (x > xx - blurSize && x > 0); x--)
                        {
                            AddPixel(image.GetPixel(x, yy));

                        }


                        CalcPixel();

                        for (x = xx; x < xx + blurSize && x < _W; x++)
                        {
                            blurred.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));

                        }
                    }
                }
            }

            else
            {
                for (xx = 0; xx < _W; xx++)
                {
                    for (yy = 0; yy < _H; yy++)
                    {
                        ResetPixel();

                        //Over pixel

                        for (y = yy; (y < yy + blurSize && y < _H); y++)
                        {
                            AddPixel(image.GetPixel(xx, y));
                        }
                        //Under pixel

                        for (y = yy; (y > yy - blurSize && y > 0); y--)
                        {
                            AddPixel(image.GetPixel(xx, y));
                        }
                        CalcPixel();
                        for (y = yy; y < yy + blurSize && y < _H; y++)
                        {
                            blurred.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));

                        }
                    }
                }
            }

            blurred.Apply();
            return blurred;
        }
        void AddPixel(Color pixel)
        {
            avgR += pixel.r;
            avgG += pixel.g;
            avgB += pixel.b;
            blurPixelCount++;
        }

        void ResetPixel()
        {
            avgR = 0.0f;
            avgG = 0.0f;
            avgB = 0.0f;
            blurPixelCount = 0;
        }

        void CalcPixel()
        {
            avgR = avgR / blurPixelCount;
            avgG = avgG / blurPixelCount;
            avgB = avgB / blurPixelCount;
        }
    }

    public static class ImageUtilities
    {
        public static bool saveThreshold = true;
        public static string imageThresholdSavepath = "\\Output\\Thresholds\\";
        public static string imageIntensitySavepath = "\\Output\\Intensities\\";
        public static string imageBlurSavepath = "\\Output\\Blurs\\";
        public static string imageShapeSavepath = "\\Output\\Shapes\\";

        public static Color[] Blur(Color[] iimage, int iwidth, int iheight, int iblurwidth, string iname, bool idebug)
        {
            Color[] blurred = new Color[iimage.Length];
            Texture2D ttex = new Texture2D(iwidth, iheight);
            ttex.SetPixels(iimage);
            Blur tblur = new Blur();
            ttex = tblur.FastBlur(ttex, iblurwidth, 1);

            blurred = ttex.GetPixels();
 
            //// look at every pixel in the blur rectangle
            //for (int xx = 0; xx < iwidth; xx++)
            //{
            //    for (int yy = 0; yy < iheight; yy++)
            //    {
            //        float avgR = 0f, avgG = 0f, avgB = 0f;
            //        int blurPixelCount = 0;
 
            //        // average the color of the red, green and blue for each pixel in the
            //        // blur size while making sure you don't go outside the image bounds
            //        for (int x = xx; (x < xx + iblurwidth && x < iwidth); x++)
            //        {
            //            for (int y = yy; (y < yy + iblurwidth && y < iheight); y++)
            //            {
            //                Color pixel = new Color();
            //                pixel = blurred[y * iwidth + x];

            //                avgR += pixel.r * 255;
            //                avgG += pixel.g * 255;
            //                avgB += pixel.b * 255;
 
            //                blurPixelCount++;
            //            }
            //        }
 
            //        avgR = avgR / blurPixelCount;
            //        avgG = avgG / blurPixelCount;
            //        avgB = avgB / blurPixelCount;
 
            //        // now that we know the average for the blur size, set each pixel to that color
            //        for (int x = xx; x < xx + iblurwidth && x < iwidth; x++)
            //            for (int y = yy; y < yy + iblurwidth && y < iheight; y++)
            //                blurred[y * iwidth + x] = new Color(avgR / 255f, avgG / 255f, avgB / 255f, 1f);
            //    }
            //}

            if (idebug)
            {
                SaveImage(blurred, imageBlurSavepath + iname + ".png", iwidth, iheight);
            }

            return blurred;
        }

        public static Color[] IntensityMap(Color[] iimage, int iwidth, int iheight, string iname, bool idebug)
        {
            Color[] treturn = new Color[iimage.Length];
            treturn = iimage;

            for (int i = 0; i < iimage.Length; i++)
            {
                float tintesity = (((iimage[i].r + iimage[i].g + iimage[i].b) / 3f));
                treturn[i] = new Color(tintesity, tintesity, tintesity, 1f);
            }

            if (idebug)
            {
                SaveImage(treturn, imageIntensitySavepath + iname + ".png", iwidth, iheight);
            }

            return treturn;
        }

        public static Color[] Threshold(Color[] iimage, int iintesity, int iwidth, int iheight, string iname, bool idebug)
        {
            Color[] treturn = new Color[iimage.Length];
            int tcounter = 0;
            treturn = iimage;

            for (int i = 0; i < iimage.Length; i++)
            {
                int tintesity = (int)(((iimage[i].r + iimage[i].g + iimage[i].b) /3f) * 255);

                if (tintesity < iintesity)
                    treturn[i] = new Color(0, 0, 0, 1f);
                else
                    tcounter++;
            }

            if (idebug)
            {
                Debug.Log("Thresholded pixels " + tcounter);
                SaveImage(treturn, imageThresholdSavepath + iname + ".png", iwidth, iheight);
            }

            return treturn;
        }

        public static Color[] Threshold(Color[] iimage, int irintesity, int igintesity, int ibintesity, int iwidth, int iheight, string iname, bool idebug)
        {
            Color[] treturn = new Color[iimage.Length];
            int tcounter = 0;
            treturn = iimage;

            for (int i = 0; i < iimage.Length; i++)
            {
                if ((int)(iimage[i].r * 255) < irintesity & (int)(iimage[i].g * 255) < igintesity & (int)(iimage[i].b * 255) < ibintesity)
                    treturn[i] = new Color(0, 0, 0, 0);
                else
                    tcounter++;
            }

            if (idebug)
            {
                Debug.Log("Thresholded pixels " + tcounter);
                SaveImage(treturn, imageThresholdSavepath + iname + ".png", iwidth, iheight);
            }

            return treturn;
        }

        public static void DebugSaveShapesToDisk(ref List<List<Vector3>> ishapes, string iname)
        {
            for (int i = 0; i < ishapes.Count; i++)
            {
                DebugSaveShapeToDisk(ishapes[i], iname + "_" + i);
            }
        }

        public static void DebugSaveShapeToDisk(List<Vector3> ishape, string iname)
        {
            int tminx = 0, tmaxx = 0, tminy = 0, tmaxy = 0;

            FindShapeBounds(ref ishape, ref tminx, ref tmaxx, ref tminy, ref tmaxy);

            int twidth = tmaxx - tminx;
            int theight = tmaxy - tminy;
            Texture2D ttex = new Texture2D(twidth, theight);
            Color[] tcolors = ttex.GetPixels();

			for (int i = 0; i < tcolors.Length; i++) 
			{
				tcolors[i] = new Color(0, 0, 0, 1);
			}

            for (int i = 0; i < ishape.Count; i++)
            {
                int tx = (int)ishape[i].x, ty = (int)ishape[i].y, tz = (int)ishape[i].z;
				tcolors[(tx - tminx) + (ty - tminy) * twidth] = new Color(1, 0, 0, 1);
            }
			SaveImage(tcolors, imageShapeSavepath + iname + ".png", twidth, theight);
        }

        public static void FindShapeBounds(ref List<Vector3> ishape, ref int iminx, ref int imaxx, ref int iminy, ref int imaxy)
        {
            for (int i = 0; i < ishape.Count; i++)
            {
                int x = (int)ishape[i].x;
                int y = (int)ishape[i].y;

                if (x > imaxx)
                    imaxx = x;

                if (x < iminx)
                    imaxx = x;

                if (y > imaxy)
                    imaxy = y;

                if (y < iminy)
                    iminy = y;
            }
        }

        public static void SaveImage(Color[] iimage, string tpath, int iwidth, int iheight)
        {
            Texture2D ttex = new Texture2D(iwidth, iheight);
            ttex.SetPixels(iimage);
            byte[] tbytes = ttex.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + tpath, tbytes);
        }

		public static void LoadImages(string ipath, ref Texture2D[] oheightmaps, ref List<string> ofilenames, string ifilter, bool debugmode)
		{
			string pathPreFix = @"file://";
			string[] files = System.IO.Directory.GetFiles(Application.dataPath + ipath, "*" + ifilter);
			string fullpath = Application.dataPath + ipath;
			oheightmaps = new Texture2D[files.Length];
			
			int tcounter = 0;
			foreach (string tstring in files)
			{
				string pathTemp = pathPreFix + tstring;
				char[] tsplit = {'/'};
				string[] tarr = tstring.Split(tsplit);
				
				ofilenames.Add(tarr[tarr.Length-1].Replace(ifilter, ""));


	
				WWW www = new WWW(pathTemp);
				Texture2D texTmp = new Texture2D(512, 512, TextureFormat.DXT1, false);

				if (debugmode)
					Debug.Log(ofilenames[ofilenames.Count - 1] + " Res: " + texTmp.height);

				www.LoadImageIntoTexture(texTmp);
				
				oheightmaps[tcounter] = texTmp;
				tcounter++;
			}
		}
    }

	public class TextureScale
	{
		public class ThreadData
		{
			public int start;
			public int end;
			public ThreadData (int s, int e) {
				start = s;
				end = e;
			}
		}
		
		private static Color[] texColors;
		private static Color[] newColors;
		private static int w;
		private static float ratioX;
		private static float ratioY;
		private static int w2;
		private static int finishCount;
		private static Mutex mutex;
		
		public static void Point (Texture2D tex, int newWidth, int newHeight)
		{
			ThreadedScale (tex, newWidth, newHeight, false);
		}
		
		public static void Bilinear (Texture2D tex, int newWidth, int newHeight)
		{
			ThreadedScale (tex, newWidth, newHeight, true);
		}
		
		private static void ThreadedScale (Texture2D tex, int newWidth, int newHeight, bool useBilinear)
		{
			texColors = tex.GetPixels();
			newColors = new Color[newWidth * newHeight];
			if (useBilinear)
			{
				ratioX = 1.0f / ((float)newWidth / (tex.width-1));
				ratioY = 1.0f / ((float)newHeight / (tex.height-1));
			}
			else {
				ratioX = ((float)tex.width) / newWidth;
				ratioY = ((float)tex.height) / newHeight;
			}
			w = tex.width;
			w2 = newWidth;
			var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
			var slice = newHeight/cores;
			
			finishCount = 0;
			if (mutex == null) {
				mutex = new Mutex(false);
			}
			if (cores > 1)
			{
				int i = 0;
				ThreadData threadData;
				for (i = 0; i < cores-1; i++) {
					threadData = new ThreadData(slice * i, slice * (i + 1));
					ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
					Thread thread = new Thread(ts);
					thread.Start(threadData);
				}
				threadData = new ThreadData(slice*i, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
				while (finishCount < cores)
				{
					Thread.Sleep(1);
				}
			}
			else
			{
				ThreadData threadData = new ThreadData(0, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
			}
			
			tex.Resize(newWidth, newHeight);
			tex.SetPixels(newColors);
			tex.Apply();
		}
		
		public static void BilinearScale (System.Object obj)
		{
			ThreadData threadData = (ThreadData) obj;
			for (var y = threadData.start; y < threadData.end; y++)
			{
				int yFloor = (int)Mathf.Floor(y * ratioY);
				var y1 = yFloor * w;
				var y2 = (yFloor+1) * w;
				var yw = y * w2;
				
				for (var x = 0; x < w2; x++) {
					int xFloor = (int)Mathf.Floor(x * ratioX);
					var xLerp = x * ratioX-xFloor;
					newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
					                                       ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
					                                       y*ratioY-yFloor);
				}
			}
			
			mutex.WaitOne();
			finishCount++;
			mutex.ReleaseMutex();
		}
		
		public static void PointScale (System.Object obj)
		{
			ThreadData threadData = (ThreadData) obj;
			for (var y = threadData.start; y < threadData.end; y++)
			{
				var thisY = (int)(ratioY * y) * w;
				var yw = y * w2;
				for (var x = 0; x < w2; x++) {
					newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
				}
			}
			
			mutex.WaitOne();
			finishCount++;
			mutex.ReleaseMutex();
		}
		
		private static Color ColorLerpUnclamped (Color c1, Color c2, float value)
		{
			return new Color (c1.r + (c2.r - c1.r)*value, 
			                  c1.g + (c2.g - c1.g)*value, 
			                  c1.b + (c2.b - c1.b)*value, 
			                  c1.a + (c2.a - c1.a)*value);
		}
	}
}