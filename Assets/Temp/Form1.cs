using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace gg.Mesh
{
    public class Form1
    {
        List<Vertex> Set = new List<Vertex>();
        public int recursion;

        private void Run()
        {
            //Set = MakeTestSet(640, 480, (int)numericUpDown2.Value, (int)numericUpDown1.Value);
            Mesh m = new Mesh();
            m.Recursion = recursion;
            //System.DateTime start = System.DateTime.Now;
            m.Compute(Set, new Rect(0, 0, 640, 480));
        }

        public int[] GetVertexIndicies(List<GraphDefinition.IntVector3> ilist, int height, int width)
        {
            Mesh m = new Mesh();
            m.Recursion = recursion;
            Set = IntToVector(ilist);
            Debug.Log("Compute Starting");
            m.Compute(Set, new Rect(0, 0, height, width));
            Debug.Log("Compute Ended");
            return m.GetVertexIndicies();
        }

        public List<Vertex> IntToVector(List<GraphDefinition.IntVector3> ilist)
        {
            List<Vertex> result = new List<Vertex>();

            for (int i = 0; i < ilist.Count; i++)
            {
                result.Add(new Vertex(ilist[i].x, ilist[i].y, ilist[i].z));
            }

            return result;
        }

        public List<Vertex> MakeTestSet(int width, int height, int seed, int count)
        {
            List<Vertex> set = new List<Vertex>();
            Random.Random r = new Random.Random(seed);
            for (int i = 0; i < count; i++)
            {
                set.Add(new Vertex(r.NextFlat_Int(0, width), r.NextFlat_Int(0, height), 0));
            }
            return set;
        }
    }
}
