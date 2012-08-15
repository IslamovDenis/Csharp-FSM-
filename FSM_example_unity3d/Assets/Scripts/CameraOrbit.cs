using UnityEngine;
using System;
using System.Collections;

public class CameraOrbit : MonoBehaviour {
  public Transform m_myCamera;
  
  private float speed = 2.0f;

	void Start () {
	  if (m_myCamera == null) 
    {
      throw new ArgumentNullException("m_myCamera");
    }  
	}
	
	void Update () {
    float trueSpeed = speed * Time.deltaTime;

    m_myCamera.Rotate(Input.GetAxis("Vertical")   * trueSpeed,
                      Input.GetAxis("Horizontal") * trueSpeed, 
                      0.0f);
	}
}
