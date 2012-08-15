using UnityEngine;
using System;

public class TextRoration : MonoBehaviour {
  public  Transform CameraTransform, 
                    Cube;

  private Transform _mMyTransform;
	
  void Start () 
  {
    _mMyTransform = transform;
	  
    if (CameraTransform == null)
    {
      throw new ArgumentNullException("CameraTransform");
    }
    if (Cube == null)
    {
      throw new ArgumentNullException("Cube");
    }
  }
	
  void Update () 
  {
    _mMyTransform.position       = Cube.position + new Vector3(0.0f, 0.0f, 0.5f);
    _mMyTransform.localRotation  = CameraTransform.rotation;
  }
}