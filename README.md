Csharp-FSM-
===========

Abstract [Finite State Machine](http://en.wikipedia.org/wiki/Finite-state_machine) created via strings and delegates.

Files info
----------
<b>Main.cs</b>

Who-to-use example with console Main function. 

<b>FiniteStateMachine.cs</b>

FSM class file.
License under the terms of the GNU Lesser General Public License (LGPL) version 2.1

### Quick tutorial:
Init class member with start condition name and function of this condition:

    public void StartStateFunc() 
    {
      // Do anything
    }
    ...
    // delegate type here is "void foo()"
    var fsm = new FiniteStateMachine("StartState", StartStateFunc);


Add some states and functions:

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
    

And if you need you can add some transitions. Transition look like state, which connect two exist state, 
but they have only one type of events for finishing. You can use FSM without transitions, but it can help you
support good logical structure:

    public void ABConnect() 
    {
      // Do transition
    }
      
    fsm.AddTransition("A", "B", TransferWater);
    

At the end  - add event for control our state conversion:

    AddEvent("EventAB", "A", "B");
    
    
Let's try our FSM:

    fsm.EventHappen("EventAB");            // a -> transition to b
    fsm.EventHappen("finish transition");  // transition -> b
    
If you want, you can back-up you FSM to first state, but use it accurate:

    fsm.CanReset = true;  // allow back-up to first step, false by default
    fsm.Reset();
    
###FSM Logical Presentation
And last one - you can save you FSM like graph with all states, transitions and events in file. At current version only 
[dot](http://en.wikipedia.org/wiki/DOT_language) format is supported. You can view this file in any type of text editor
or like vector image in special application like [graphviz](http://graphviz.org/)

    fsm.SaveToFile("dot");  // save *.dot file