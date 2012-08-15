using UnityEngine;
using FiniteStateMachineProject;

public class FSMCube : MonoBehaviour {
  private Transform _myTransform;
  private FiniteStateMachine _fsm;

  private const float MoveSpeed     = 0.5f,
                      RotationSpeed = 0.5f;

  private float _delay;

  private bool _sayHello = true;
  
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
    if (_sayHello) 
    {
      Debug.Log("SayHello()");
      Debug.Log("Hi! I like your strange red color!");
      
      Text.renderer.enabled = true;
      _sayHello              = false;
    }
    if (_delay >= 2)
    {
      Text.renderer.enabled = false;
      _fsm.EventHappen("say goodbye and go away");
    }

    _delay += Time.deltaTime;
  }

  public void MoveTo(Transform target) 
  { 
    Debug.Log("moveTo()");
    
    transform.position = 
      Vector3.Lerp(transform.position, target.position, Time.deltaTime * MoveSpeed);
    transform.rotation = 
      Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * RotationSpeed);

    if((_myTransform.position - target.position).magnitude <= 1.5f)
    {
      _fsm.EventHappen("finish transition");
      Debug.Log("finish transition");
    }
  }
  
  void Start() 
  {
    _myTransform = transform;
    
    Text.renderer.enabled = false;

    _fsm = new FiniteStateMachine("start state", StartState);
    _fsm.AddState("say hello", SayHello);

    _fsm.AddEvent("go and say hello", "start state", "say hello");
    _fsm.AddEvent("say goodbye and go away", "say hello", "start state"); 
    
    _fsm.AddTransition("start state", "say hello",  ()=> MoveTo(Target));
    _fsm.AddTransition("say hello",  "start state", ()=> MoveTo(End));
  }

  void Update() 
  {
    if (Input.GetKey(KeyCode.Return))
    {
      // m_FSM.EventHappen("go and say hello");
    }

    _fsm.Invoke();  // Каждый раз вызываем делегата текущего состояния или перехода
                    // Поэтому будьте внимательны, функция и так крутиться каждый цикл!
  }

  void OnGUI() 
  {
    if (GUI.Button (new Rect (10,10, 126, 30), "Run state machine!")) {
      _fsm.EventHappen("go and say hello");
      _sayHello = true;
      _delay    = 0.0f;
      print ("FSM running");
    }
  }
}
