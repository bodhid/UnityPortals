using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Portal : MonoBehaviour
{
	public Portal connection;
	private Camera[] cam;
	private Transform[] camParent;
	private RenderTexture[] buffer;
	private Material myMaterial;
	[Range(1,16)]
	public int portalRenderDepth=8;
	private int renderDepth = 8;
	private int portalDepth=0;
	void Start ()
	{
		InitializeBuffers();
	}

	private void InitializeBuffers()
	{
		System.GC.Collect();
		Resources.UnloadUnusedAssets();
		camParent = new Transform[portalRenderDepth];
		cam = new Camera[portalRenderDepth];
		for (int i = 0; i < cam.Length; ++i)
		{
			camParent[i] = new GameObject("CamParent").transform;
			camParent[i].SetParent(transform);
			camParent[i].localPosition = Vector3.zero;
			GameObject portalCam = new GameObject("PortalCam");
			cam[i] = portalCam.AddComponent<Camera>();
			portalCam.transform.SetParent(camParent[i]);
			portalCam.SetActive(false);
		}
		myMaterial = GetComponent<MeshRenderer>().material;
		buffer = new RenderTexture[portalRenderDepth];
		renderDepth = portalRenderDepth;
	}

	private void Update()
	{
		if (renderDepth != portalRenderDepth) InitializeBuffers();
		portalDepth = -1;
		myMaterial.mainTexture = buffer[0];
	}
	private void OnWillRenderObject ()
	{
		if (portalDepth<portalRenderDepth-1)
		{
			portalDepth++;
			RenderTexture.ReleaseTemporary(buffer[portalDepth]);
			buffer[portalDepth] = RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
			cam[portalDepth].CopyFrom(Camera.current);

			int excludeLayer = (gameObject.layer == 11) ? 12 : 11;
			cam[portalDepth].cullingMask = ~(1 << excludeLayer);
			camParent[portalDepth].position = transform.position;
			camParent[portalDepth].rotation = transform.rotation;
			cam[portalDepth].transform.position = Camera.current.transform.position;
			cam[portalDepth].transform.rotation = Camera.current.transform.rotation;
			camParent[portalDepth].position = connection.transform.position;
			camParent[portalDepth].eulerAngles = connection.transform.eulerAngles + new Vector3(0, 180, 0);
			cam[portalDepth].targetTexture = buffer[portalDepth];

			Texture temp = myMaterial.mainTexture;
			myMaterial.mainTexture = buffer[(portalDepth+1)%portalRenderDepth];
			cam[portalDepth].Render();
			myMaterial.mainTexture = temp;
		}
	}
}
