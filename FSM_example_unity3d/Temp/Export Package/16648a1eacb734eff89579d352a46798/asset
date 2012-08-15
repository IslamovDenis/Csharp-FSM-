using UnityEngine;
using System;
using System.Collections;

public class TextRoration : MonoBehaviour {
  public  Transform cameraTransform, 
                    cube;

  private Transform m_myTransform;
	
  void Start () 
  {
	  m_myTransform = transform;
	  
    if (cameraTransform == null)
    {
      throw new ArgumentNullException("cameraTransform");
    }
    if (cube == null)
    {
      throw new ArgumentNullException("cube");
    }
  }
	
	void Update () 
  {
    m_myTransform.position       = cube.position + new Vector3(0.0f, 0.0f, 0.5f);
    m_myTransform.localRotation  = cameraTransform.rotation;
  }
}
