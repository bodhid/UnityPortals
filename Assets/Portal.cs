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
		float resolution = 1;
		for (int i = 0; i < portalDepth; ++i) resolution *= downscaleFactor;
		buffer[portalDepth] = RenderTexture.GetTemporary((int)( Screen.width* resolution), (int)(Screen.height* resolution), 32, RenderTextureFormat.ARGB32);
	}

	private void SetPortalCameraSettings(int portalDepth)
	{
		cam[portalDepth].CopyFrom(Camera.current);

		//Never render the other portal, this creates a messy loop
		int excludeLayer = (gameObject.layer == 11) ? 12 : 11;
		cam[portalDepth].cullingMask = ~(1 << excludeLayer);

		//Set znear for portalCamera

		Transform currentCameraTransform = Camera.current.transform;
		Vector3 closestPoint =  myCollider.bounds.ClosestPoint(Camera.current.transform.position);
		Vector3 closestPointOnDepthBuffer = Vector3.Project(closestPoint - currentCameraTransform.position, currentCameraTransform.forward);
		cam[portalDepth].nearClipPlane = closestPointOnDepthBuffer.magnitude;
	}

	private void SetPortalCameraTransform(int portalDepth)
	{
		Transform currentCameraTransform = Camera.current.transform;
		camParent[portalDepth].position = transform.position;
		camParent[portalDepth].rotation = transform.rotation;
		cam[portalDepth].transform.position = currentCameraTransform.position;
		cam[portalDepth].transform.rotation = currentCameraTransform.rotation;
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
			Transform otherTransform = other.transform;
			Transform otherParent = other.transform.parent;
			Transform myParent = transform.parent;
			Vector3 myPosition = transform.localPosition;
			Quaternion myRotation = transform.localRotation;
			otherTransform.SetParent(transform);
			transform.SetParent(connection.transform);
			transform.localEulerAngles = new Vector3(0, 180, 0);
			transform.localPosition = Vector3.zero;
			otherTransform.SetParent(otherParent);
			transform.SetParent(myParent);
			transform.localPosition = myPosition;
			transform.localRotation = myRotation;
		}
	}
}
