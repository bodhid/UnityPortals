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
	public MeshRenderer inside;
	private Material insideMaterial;
	private BoxCollider myCollider;
	[Range(1, 16)]
	public int portalRenderDepth = 8;
	[Range(0.5f, 1f)]
	public float downscaleFactor = 1;

	private int renderDepth = 8;
	private int portalDepth = 0;
	void Start()
	{
		insideMaterial = inside.material;
		myMaterial = GetComponent<MeshRenderer>().material;
		myCollider = GetComponent<BoxCollider>();

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
		buffer = new RenderTexture[portalRenderDepth];
		renderDepth = portalRenderDepth;
	}

	private void Update()
	{
		if (renderDepth != portalRenderDepth) InitializeBuffers();
		portalDepth = -1;
		myMaterial.mainTexture = buffer[0];
		insideMaterial.mainTexture = buffer[0];
	}
	private void OnWillRenderObject()
	{
		if (portalDepth < renderDepth - 1)
		{
			portalDepth++;
			SetPortalBuffer(portalDepth);
			SetPortalCameraSettings(portalDepth);
			SetPortalCameraTransform(portalDepth);
			RenderPortal(portalDepth);
		}
	}
	private void SetPortalBuffer(int portalDepth)
	{
		RenderTexture.ReleaseTemporary(buffer[portalDepth]);
		buffer[portalDepth] = RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
	}

	private void SetPortalCameraSettings(int portalDepth)
	{
		cam[portalDepth].CopyFrom(Camera.current);
		int excludeLayer = (gameObject.layer == 11) ? 12 : 11;
		cam[portalDepth].cullingMask = ~(1 << excludeLayer);
	}

	private void SetPortalCameraTransform(int portalDepth)
	{
		camParent[portalDepth].position = transform.position;
		camParent[portalDepth].rotation = transform.rotation;
		cam[portalDepth].transform.position = Camera.current.transform.position;
		cam[portalDepth].transform.rotation = Camera.current.transform.rotation;
		camParent[portalDepth].position = connection.transform.position;
		camParent[portalDepth].eulerAngles = connection.transform.eulerAngles + new Vector3(0, 180, 0);
	}

	private void RenderPortal(int portalDepth)
	{
		cam[portalDepth].targetTexture = buffer[portalDepth];
		Texture temp = myMaterial.mainTexture;
		myMaterial.mainTexture = (portalDepth == (renderDepth - 1)) ? (Texture2D.blackTexture) : ((Texture)buffer[portalDepth + 1]);
		cam[portalDepth].Render();
		myMaterial.mainTexture = temp;
	}

	private void OnTriggerStay(Collider other)
	{
		//Teleport object that enters
		if (myCollider.bounds.Contains(other.transform.position))
		{
			Transform t = other.transform;
			Transform parent = other.transform.parent;
			t.SetParent(transform);
			Vector3 localPosition = t.localPosition;
			Quaternion localRotation = t.localRotation;
			t.SetParent(connection.transform);
			t.localPosition = localPosition;
			t.localRotation = localRotation;
			t.SetParent(parent);
		}
	}
}
