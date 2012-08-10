Csharp-FSM-
===========

Abstract [Finite State Machine](http://en.wikipedia.org/wiki/Finite-state_machine) created via strings and delegates.

Files info
----------
<b>Main.cs</b>

Who-to-use example with console Main function. 

<b>FiniteStateMachine.cs</b>

FSM class file.

### Quick tutorial:

  1. Init class member with start condition name and function of this condition:

    public void StartStateFunc() 
    {
      // Do anything
    }
    ...
    // delegate type here is "void foo()"
    var fsm = new FiniteStateMachine("StartState", StartStateFunc);

  2. Add some states and functions:

    public void StateAFunc() 
    {
      // Do something else
    }

    public void StateBFunc() 
    {
      // Do something else
    }
    ...
    fsm.AddState("A", StateAFunc);
    fsm.AddState("B", StateBFunc);
    
  3. And if you need you can add some transitions. Transition look like state, which connect two exist state, 
     but they have only one type of events for finishing. You can use FSM without transitions, but it can help you
     support good logical structure. 

    public void ABConnect() 
    {
      // Do transition
    }
      
    fsm.AddTransition("A", "B", TransferWater);
    
  4. Try to run it!
