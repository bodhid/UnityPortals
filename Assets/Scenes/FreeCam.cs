﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour {

	private Vector3 euler=Vector3.zero;
	void Update ()
	{
		euler = transform.localEulerAngles;
		if (euler.x > 89) euler.x -= 360;
		if (euler.z > 189) euler.z -= 360;
		euler.y += Input.GetAxis("Mouse X");
		euler.x -= Input.GetAxis("Mouse Y");
		euler.x = Mathf.Clamp(euler.x, -89, 89);
		euler.z = Mathf.Lerp(euler.z, 0, Time.deltaTime * 4f);
		transform.localEulerAngles = euler;
		Vector2 move = Vector2.zero;
		float speed = Input.GetKey(KeyCode.LeftShift) ? 6f : 2f;
		move.y -= Input.GetKey(KeyCode.S) ? 1 : 0;
		move.y += Input.GetKey(KeyCode.W) ? 1 : 0;
		move.x -= Input.GetKey(KeyCode.A) ? 1 : 0; 
		move.x += Input.GetKey(KeyCode.D) ? 1 : 0;
		transform.position += transform.forward * move.y*Time.deltaTime* speed;
		transform.position += transform.right * move.x*Time.deltaTime* speed;
	}
}
