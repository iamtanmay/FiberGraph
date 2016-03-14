using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImagePipeline;

public class Voxelizer : MonoBehaviour {

	public Transform voxelprefab;

	public bool debugmode = true;
	public string path = "\\Stack";
	public List<string> filenames;
	public Texture2D[] heightmaps;
	List<Color[]> imagepixels;
	public List<Vector3> voxels;
	public string filetypefilter = ".jpeg";

	//Voxel edge dimension in um
	public float voxeledgelength_um = 0.47f;

	//Image downscaled x resolution
	public int width = 544;

	//Max pixel intensity to expect
	public float maxintensity = 0.267f;

	
	//Min pixel intensity to expect
	public float minintensity = 0.1f;

	//Pixel intensity to use per slice
	public float intensityrangeperslice = 0.96f;

	public int voxelsgeneraterate = 500;
	public int maxvoxels = 10000;

	public int stage;
	public int currentgeneratedvoxel = 0;

	// Use this for initialization
	void Start () {
		voxels = new List<Vector3> ();

		//Load all images in Folder
		filenames = new List<string>();
		
		ImageUtilities.LoadImages(path, ref heightmaps, ref filenames, filetypefilter, debugmode);
		imagepixels = new List<Color[]>();

		Debug.Log ("Heightmap count " + heightmaps.Length);

//		for (int i=0; i<heightmaps.Length; i++) 
//		{
//			imagepixels.Add(heightmaps[i].GetPixels());
//		}
		StartCoroutine (Voxelize ());
	}

	public IEnumerator Voxelize()
	{
		int ttotalvoxels = 0;

		for (int k=0; k<heightmaps.Length; k++) 
		{
			List<Vector3> tvoxels = new List<Vector3>();

			for (int i=0; i<width; i++) 
			{
				for (int j=0; j<width; j++)
				{
					float tintensity = (heightmaps[k].GetPixel(i,j).r);
					
					if (tintensity > minintensity)
					{
						Vector3 tvoxel = new Vector3(i, j, (intensityrangeperslice * tintensity / maxintensity + k));
						tvoxel = tvoxel * voxeledgelength_um;
						tvoxels.Add(tvoxel);
					}
				}
			}

			voxels.AddRange(tvoxels);
			ttotalvoxels += tvoxels.Count;

			if (debugmode)
				Debug.Log ("Slice " + k + " Voxel count " + tvoxels.Count);

			yield return null;
		}

		stage = -1;

		if (debugmode)
			Debug.Log ("Total voxels count " + ttotalvoxels);

		yield return null;
	}

	public void GenerateVoxels(int istart, int iend)
	{
		for (int i=istart; i<iend; i++) {
			GameObject.Instantiate(voxelprefab,voxels[i], Quaternion.identity);
		}
		currentgeneratedvoxel = iend;
	}

	// Update is called once per frame
	void Update () {	
		switch (stage) {
		case -1: 
			if (currentgeneratedvoxel + voxelsgeneraterate < maxvoxels)
				GenerateVoxels(currentgeneratedvoxel, currentgeneratedvoxel + voxelsgeneraterate);
			else if (currentgeneratedvoxel < maxvoxels)
				GenerateVoxels(currentgeneratedvoxel, maxvoxels);
			break;
			case 0: break;
			case 1: break;
			default: break;
		}
	}
}
