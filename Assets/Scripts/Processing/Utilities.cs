using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ImagePipeline
{
    public static class Utilities
    {
        public static void WriteVertices2DToText(List<List<Vector2>> ivertices)
        {
			string tout = "";
            //Output the 2D vertices as a text file
			for (int i = 0; i < 1; i++)//ivertices.Count; i++)
            {
                for (int j = 0; j < ivertices[i].Count; j++)
                {
					tout += ivertices[i][j].x + " " + ivertices[i][j].y + "\r\n";//(char)10;
                }
            }
			File.WriteAllText(".\\file.txt", tout);
		}

        public static void WriteToText(List<string> itext)
        {
            string tout = "";

            for (int i = 0; i < itext.Count; i++)
            {
                tout += itext[i] + "\r\n";
            }
            File.WriteAllText(".\\file2.txt", tout);
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

        public static IEnumerator LoadImage(string url, Texture2D heightmap, bool image_loaded)
        {
            // Start a download of the given URL
            WWW www = new WWW(url);

            // Wait for download to complete
            yield return www;

            heightmap = www.texture;
            image_loaded = true;

            yield break;
        }
    }
}