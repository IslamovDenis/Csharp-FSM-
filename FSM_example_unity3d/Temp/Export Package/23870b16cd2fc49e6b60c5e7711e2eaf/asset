using UnityEngine;
using System.Collections;
using FiniteStateMachineProject;

public class FSMCube : MonoBehaviour {
  private Transform m_myTransform;
  private FiniteStateMachine m_FSM;
  
  private float moveSpeed     = 0.5f,
                rotationSpeed = 0.5f,
                delay         = 0.0f;
  
  private bool sayHello = false;
  
  public Transform Target, 
                   End;

  public GameObject Text;

  // Функции для состояний
  public void StartState() 
  {
    Debug.Log("StartState()");
  }

  public void SayHello() 
  { 
    if (sayHello == false) 
    {
      Debug.Log("SayHello()");
      Debug.Log("Hi! I like your strange red color!");
      
      Text.renderer.enabled = true;
      sayHello              = true;
    }
    if (delay >= 2)
    {
      Text.renderer.enabled = false;
      m_FSM.EventHappen("say goodbye and go away");
    }

    delay += Time.deltaTime;
  }

  public void moveTo(Transform target) 
  { 
    Debug.Log("moveTo()");
    
    transform.position = 
      Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
    transform.rotation = 
      Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * rotationSpeed);

    if((m_myTransform.position - target.position).magnitude <= 1.5f)
    {
      m_FSM.EventHappen("finish transition");
      Debug.Log("finish transition");
    }
  }
  
  void Start() 
  {
    m_myTransform = transform;
    
    Text.renderer.enabled = false;

    m_FSM = new FiniteStateMachine("start state", StartState);
    m_FSM.AddState("say hello", SayHello);

    m_FSM.AddEvent("go and say hello", "start state", "say hello");
    m_FSM.AddEvent("say goodbye and go away", "say hello", "start state"); 
    
    m_FSM.AddTransition("start state", "say hello",  ()=> { moveTo(Target); });
    m_FSM.AddTransition("say hello",  "start state", ()=> { moveTo(End); } );
  }

  void Update() 
  {
    if (Input.GetKey(KeyCode.Return))
    {
      // m_FSM.EventHappen("go and say hello");
    }

    m_FSM.Invoke();
  }

  void OnGUI() 
  {
    if (GUI.Button (new Rect (10,10, 126, 30), "Run state machine!")) {
      m_FSM.EventHappen("go and say hello");
      print ("FSM running");
    }
  }
}
