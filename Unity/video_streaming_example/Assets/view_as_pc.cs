using UnityEngine;
using System;

/// <summary>
/// A Unity3D Script to dipsplay Mjpeg streams. Apply this script to the mesh that you want to use to view the Mjpeg stream. 
/// </summary>
public class view_as_pc : MonoBehaviour
{
	/// <param name="streamAddress">
	/// Set this to be the network address of the mjpg stream. 
	/// Example: "http://extcam-16.se.axis.com/mjpg/video.mjpg"
	/// </param>
	public string ip_address;
	public string rgb_topic;
	public string depth_topic;

	Texture2D tex;
	byte[] pixelMap;
	const int numOfCols = 16;
	const int numOfRows = numOfCols / 2;
	const int numOfPixels = numOfCols * numOfRows;
	// Flag showing when to update the frame
	bool updateFrame = false;

	MjpegProcessor rgb_mjpeg;
	MjpegProcessor depth_mjpeg;
	//System.Diagnostics.Stopwatch watch;
	int frameCount = 0;
	bool newRGB = false;
	bool newDepth = false;

	ParticleSystem.Particle[] particles;

	public void Start()
	{
		string rgb_url = "http://" + ip_address + ":8080/stream?topic=" + rgb_topic + "&quality=30";
		string depth_url = "http://" + ip_address + ":8080/stream?topic=" + depth_topic + "&quality=30";

		// Stream RGB
		rgb_mjpeg = new MjpegProcessor();
		rgb_mjpeg.FrameReady += rgb_FrameReady;
		rgb_mjpeg.Error += mjpeg_Error;
		Uri rgb_jpeg_address = new Uri(rgb_url);
		rgb_mjpeg.ParseStream(rgb_jpeg_address);

		// Stream Depth
		depth_mjpeg = new MjpegProcessor();
		depth_mjpeg.FrameReady += depth_FrameReady;
		depth_mjpeg.Error += mjpeg_Error;
		Uri depth_jpeg_address = new Uri(depth_url);
		depth_mjpeg.ParseStream(depth_jpeg_address);

		// Create a 16x16 texture with PVRTC RGBA4 format
		// and will it with raw PVRTC bytes.
		tex = new Texture2D(800, 600, TextureFormat.PVRTC_RGBA4, false);
	}
	private void rgb_FrameReady(object sender, FrameReadyEventArgs e)
	{
		newRGB = true;
	}
	private void depth_FrameReady(object sender, FrameReadyEventArgs e)
	{
		newDepth = true;
	}
	void mjpeg_Error(object sender, ErrorEventArgs e)
	{
		Debug.Log("Error received while reading the MJPEG.");
	}

	// Update is called once per frame
	void Update()
	{
		if (newRGB & newDepth)
		{
			Debug.Log("giving out new pc");
			SetParticles ();
//			tex.LoadImage(rgb_mjpeg.CurrentFrame);
//			tex.Apply();
			// Assign texture to renderer's material.
			//GetComponent<Renderer>().material.mainTexture = tex;
			newDepth = false;
			newRGB = false;
		}
	}

	void OnDestroy()
	{
		rgb_mjpeg.StopStream();
		depth_mjpeg.StopStream ();
	}

	void SetParticles()
	{
		Debug.Log ("SetParticles()");
		tex.LoadImage(rgb_mjpeg.CurrentFrame);
		Color[] pixels = tex.GetPixels ();
		tex.LoadImage (depth_mjpeg.CurrentFrame);
		Color[] depth = tex.GetPixels ();
		int size = tex.height * tex.width;

		Debug.Log ("Size is " + size);
		particles = new ParticleSystem.Particle[size];

		Debug.Log("pixels length =" + pixels.Length);
		Debug.Log("depth length =" + depth.Length);

		int width = tex.width;
		int height = tex.height;
		int focal = 1;
		float invFocal = 1.0f / focal;

		for (int v = 0; v < height; v++)
		{
			for (int u = 0; u < width; u++)
			{
				int c = u + v * width;
				float grayscale = depth [c].r;
				if (grayscale == 0)
				{
					Debug.Log ("g=0");
					//particles [u + v * width].position = new Vector3 (float.NaN, float.NaN, float.NaN);
					//particles [u + v * width].startColor = new Color (0, 0, 0);
				}
				else
				{
					particles [c].position = new Vector3 (u * grayscale * invFocal, v * grayscale * invFocal, grayscale * invFocal);
					particles [c].startColor = new Color (pixels[c].r, pixels[c].g, pixels[c].b);
				}
			}
		}

		ParticleSystem system = (ParticleSystem)GetComponent("ParticleSystem");
		system.Clear ();
		system.SetParticles (particles, particles.Length);
	}
}
