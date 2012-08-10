using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
// using System.Threading.Tasks;  For .NET 4.0

/// <summary>
/// Файл FiniteStateMachine.cs
/// Автор: Исламов Денис, ОАО "ДЖЕТ"
/// Вид лицензии: GNU LGPL 
/// </summary>  

namespace FiniteStateMachineProject
{ 
  /// <summary>
  /// Класс - Конечный детерминированный автомат
  /// </summary>
  public class FiniteStateMachine
  {
    // Переход между событиями записывается как "first_state->second_state"
    private readonly Dictionary<string, State>      _states;      // Имя состояния, функция 
    private readonly Dictionary<string, Transition> _transitions; // состояние -> cледующие событие, функция перехода
    private readonly Dictionary<string, string>     _events;      // событие, состояние -> cледующие состояние

    private string _startStateName;

    private bool _transitionState;

    public delegate void State();       // Функция состояния
    public delegate void Transition();  // Переход между событиями, если таковой есть

    /// <summary>
    /// Имя предыдущего состояния, только get
    /// </summary>
    public string PrevStateName { get; private set; }

    /// <summary>
    /// Имя текущего состояния, только get
    /// </summary>
    public string CurrenStateName { get; private set; }
    
    /// <summary>
    /// Константа - конец перехода
    /// </summary>
    public static string FINISH_TRANSITION { get; set; }
    
    /// <summary>
    /// Возможно ли делать откат КА?
    /// </summary>
    public bool CanReset { get; set; }

    /// <summary>
    /// Конструктор - инициализация словарей
    /// </summary>
    /// <param name="startStateName">Начальное состояние оператора</param>
    /// <param name="stateFunc">Функция отвечающие за начальное состояние</param>
    /// <exception cref="ArgumentNullException">Аргумент currentStateName = null</exception>
    /// <exception cref="ArgumentNullException">Аргумент stateFunc = null</exception>
    public FiniteStateMachine(string startStateName, State stateFunc)
    {
      if (startStateName != null && stateFunc != null) {
        CurrenStateName   = startStateName;
        PrevStateName     = startStateName;
        _startStateName   = startStateName;

        FINISH_TRANSITION = "finish transition";
        
        _transitionState = false;

        _states      = new Dictionary<string, State>();
        _transitions = new Dictionary<string, Transition>();
        _events      = new Dictionary<string, string>();

        var execFunc = new State(stateFunc);
        _states.Add(startStateName, execFunc);
        execFunc.DynamicInvoke();
      }
      else if (startStateName == null)
      {
        throw new ArgumentNullException("startStateName");
      }
      else if (stateFunc == null)
      {
        throw new ArgumentNullException("stateFunc");
      }
    }

    /// <summary>
    /// Событие произошло
    /// </summary>
    /// <param name="eventName">Имя события, по завершению перехода, всегда передается "finish transition"</param>
    /// <exception cref="ApplicationException">Нет события связанного с текущим состоянием</exception>
    /// <exception cref="KeyNotFoundException">Нет ключа в словаре событий</exception>
    /// <exception cref="ArgumentNullException">аргумент eventName = null</exception>
    /// <exception cref="ApplicationException">Состояние - не переходное</exception>
    public void EventHappen(string eventName)
    {
      if (eventName != null) 
      {
        if (eventName != FINISH_TRANSITION)
        {
          if (_events.ContainsKey(eventName) && 
              LeftPart(_events[eventName]) == CurrenStateName) {
            PrevStateName   = CurrenStateName;               // a = b
            CurrenStateName = RightPart(_events[eventName]); // b = следующие ребро

            // выполняем переход, если таковой есть
            if (_transitions.ContainsKey(PrevStateName + "->" + CurrenStateName))
            {
              _transitionState = true;
              _transitions[PrevStateName + "->" + CurrenStateName].DynamicInvoke();
            }
            else // В проитвном случае сразу переключаемся на состояние
            {
              _transitionState = false;
              _states[CurrenStateName].DynamicInvoke();
            }
          }
          else if (!_events.ContainsKey(eventName))  
          {
            throw new KeyNotFoundException("Can't find " + eventName + 
                                           " in events collection");
          }
          else if (!_events[eventName].StartsWith(CurrenStateName))
          {
            throw new ApplicationException("Event " + eventName + 
                                           " haven't connection with current state");
          }
        }
        else
        {
          if (_transitionState)
          {
            _transitionState = false;
            _states[CurrenStateName].DynamicInvoke();
          }
          else
          {
            throw new ApplicationException("Current state - not transition");
          }
        }
      }
      else // eventName == null)
      {
        throw new ArgumentNullException("eventName");
      }
      
    }

    /// <summary>
    /// Добавить состояние
    /// </summary>
    /// <param name="stateName">Имя состояния</param>
    /// <param name="stateFunc">Функция выполняемая при переходе в состояние</param>
    /// <exception cref="ArgumentNullException">Аргумент stateName = null</exception>
    /// <exception cref="ArgumentNullException">Аргумент stateFunc = null</exception>
    /// <exception cref="ArgumentException">Уже есть такое состояние в коллекции состояний</exception>
    public void AddState(string stateName, State stateFunc)
    {
      if (stateName != null && stateFunc != null) {
        if (!_states.ContainsKey(stateName)) 
        {
          var execFunc = new State(stateFunc);
          _states.Add(stateName, execFunc);
        }
        else  // (_states.ContainsKey(stateName))
        {
        throw new ArgumentException("State " + stateName + 
                                    "already present in  states collection");
        }
      }
      else if (stateName == null)
      {
        throw new ArgumentNullException("stateName");
      }
      else if (stateFunc == null)
      {
        throw new ArgumentNullException("stateFunc");
      }
      
    }

    /// <summary>
    /// Удалить состояние
    /// </summary>
    /// <param name="stateName">Имя состояния</param>
    /// <exception cref="KeyNotFoundException">Нет такого элемента в коллекции</exception>
    public void RemoveState(string stateName)
    {
      if (!_states.ContainsKey(stateName))
      {
        throw new KeyNotFoundException("Can't find " + stateName + " in states collection");
      }

      _states.Remove(stateName);
      RemoveConnectTransitions(stateName);
      RemoveConnectEvents(stateName);
    }

    /// <summary>
    /// Добавить переход
    /// </summary>
    /// <param name="startState">Начало перехода</param>
    /// <param name="endState">Конец перехода</param>
    /// <param name="transitionFunc">Функция перехода</param>
    /// <exception cref="KeyNotFoundException">Нет начального состояния в коллекции</exception>
    /// <exception cref="KeyNotFoundException">Нет конечного состояния в коллекции</exception>
    /// <exception cref="KeyNotFoundException">Нет перехода в коллекции</exception>
    public void AddTransition(string startState, string endState,
                              Transition transitionFunc)
    {
      if (_states.ContainsKey(startState) && 
          _states.ContainsKey(endState))
      { 
        var transitionName = startState + "->" + endState;
        
        if (!_transitions.ContainsKey(transitionName))
        {
          var execFunc = new Transition(transitionFunc);
          _transitions.Add(startState + "->" + endState, execFunc);
        }
        else  // _transitions.ContainsKey(transitionName))
        {
        throw new ArgumentException("Transition " + startState +
                                    " already present in transitions collection");
        }
      }

      else if (!_states.ContainsKey(startState))
      {
        throw new KeyNotFoundException("Can't find state " + startState +
                                       " in states collection");
      }
      else if (!_states.ContainsKey(endState))
      {
        throw new KeyNotFoundException("Can't find state " + endState +
                                       " in states collection");
      }
    }

    /// <summary>
    /// Удалить переход
    /// </summary>
    /// <param name="startState">Начало перехода</param>
    /// <param name="endState">Конец перехода</param>
    /// <exception cref="ArgumentNullException">Аргумент startState = null</exception>
    /// <exception cref="ArgumentNullException">Аргумент endState = null</exception>
    public void RemoveTransition(string startState, string endState)
    {
      if (startState != null && endState != null)
      {
        RemoveTransition(startState + "->" + endState);
      }
      else if (startState == null)
      {
        throw new ArgumentNullException("startState");
      }
      else if (endState == null)
      {
        throw new ArgumentNullException("endState");
      }
    }

    /// <summary>
    /// Удалить переход
    /// </summary>
    /// <param name="transitionName">Имя перехода (a->b)</param>
    /// <exception cref="ArgumentNullException">Аргумент transitionName = null</exception>
    /// <exception cref="KeyNotFoundException">Нет такого перехода в коллекции</exception>
    public void RemoveTransition(string transitionName)
    {
      if (transitionName != null) {
        if (_transitions.ContainsKey(transitionName))
        {
          _transitions.Remove(transitionName);
        }
        else // !_transitions.ContainsKey(transitionName)
        {
          throw new KeyNotFoundException("Can't find " + transitionName +
                                         "int transitions collection");
        }
      }
      else  // transitionName == null
      {
        throw new ArgumentNullException("transitionName");
      }
    }

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="eventName">Название события</param>
    /// <param name="startState">Начальное состояние</param>
    /// <param name="endState">Конечное состояние</param>
    public void AddEvent(string eventName, string startState,
                         string endState)
    {
      if (_states.ContainsKey(startState) &&
          _states.ContainsKey(endState))
      {
        var eventValue = startState + "->" + endState;
        
        if (!_events.ContainsKey(eventName))
        {
          _events.Add(eventName, eventValue);
        }
        else // _events.ContainsKey(eventName))
        {
          throw new ArgumentException("Event" + eventName +
                                      "already present in transitions collection");
        }
      }
      else if (!_states.ContainsKey(startState))
      {
        throw new KeyNotFoundException("prevState");
      }
      else if (!_states.ContainsKey(endState))
      {
        throw new KeyNotFoundException("nextState");
      }
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="eventName"></param>
    public void RemoveEvent(string eventName)
    {
      if (_events.ContainsKey(eventName))
      {
        _events.Remove(eventName);
      }
      else // !_events.ContainsKey(eventName)
      {
        throw new KeyNotFoundException("nextState");
      }
    }

    /// <summary>
    /// Удалить все смежные события инцидентные данному состоянию 
    /// </summary>
    /// <param name="stateName">Имя состояния</param>
    /// <exception cref="ArgumentNullException">Аргумент stateName = null</exception>
    private void RemoveConnectEvents(string stateName)
    {
      if (stateName != null)
      {
        foreach (var _event in
          _events.Where(_event => LeftPart(_event.Value) == stateName ||
                                  RightPart(_event.Value) == stateName))
        {
          RemoveEvent(_event.Key);
        }
      } 
      else  // stateName == null
      {
        throw new ArgumentNullException("stateName");
      }
    }

    /// <summary>
    /// Удалить все смежные переходы инцидентные данному состоянию
    /// </summary>
    /// <param name="stateName">Имя состояния</param>
    /// <exception cref="ArgumentNullException">Аргумент stateName = null</exception>
    private void RemoveConnectTransitions(string stateName)
    {
      if (stateName != null)
      {
        foreach (var transition in
          _transitions.Where(transition => LeftPart(transition.Key) == stateName ||
                                           RightPart(transition.Key) == stateName))
        {
          RemoveTransition(transition.Key);
        }
      }
      else  // (stateName == null)
      {
        throw new ArgumentNullException("stateName");
      }
    }

    ///<summary>
    /// Вспомогательная функция
    /// </summary>
    /// <param name="edge">Названия ребра, связывающего состояния (a->b)</param>
    /// <exception cref="ApplicationException">Проверяет, есть ли в строке '->'</exception>
    /// <returns>Левая вершина (начало ребра), если таковой нет</returns>
    private static string LeftPart(string edge)
    {
      var indexOfQuote = edge.IndexOf('>');
      if (indexOfQuote != -1)
      {
        var tmpForCopy = new char[indexOfQuote - 1]; // -2, т.к. есть еще '-'
        edge.CopyTo(0, tmpForCopy, 0, indexOfQuote - 1);

        return new string(tmpForCopy);
      }
      else  // indexOfQuote == -1
      {
        throw new ApplicationException("Can't find '->' symbols in string " + edge);
      }
    }

    /// <summary>
    /// Вспомогательная функция
    /// </summary>
    /// <param name="edge">Названия ребра, связывающего состояния (a->b)</param>
    /// <returns>Правая вершина (конец ребра) или null, если таковой нет</returns>
    private static string RightPart(string edge)
    {
      var indexOfQuote = edge.IndexOf('>');
      if (indexOfQuote != -1)
      {
        var tmpForCopy = new char[edge.Length - indexOfQuote - 1];  // -2, т.к. есть еще '-'
        edge.CopyTo(indexOfQuote + 1, tmpForCopy, 0,
                    edge.Length - indexOfQuote - 1);
        
        return new string(tmpForCopy);
      }
      if (indexOfQuote == -1)
      {
        throw new ApplicationException("Can't find '->' symbols in string " +
                                       edge);
      }
        return null;
    }

    public void Reset()
    {
      if (CanReset) 
      {
        CurrenStateName = _startStateName;
      }
      else 
      {
        throw new ApplicationException("This FSM can't be reset");
      }
    }

    /// <summary>
    /// Записать структуру КА в файл
    /// </summary>
    /// <param name="fileFormat">Формат файла (пока только dot)</param>
    /// <param name="fileFormat">Имя файла (указывайте расширение соответственно формату)</param>
    /// <exception cref="ArgumentNullException">Аргумент fileFormat = null</exception>
    /// <exception cref="ArgumentNullException">Аргумент fileName = null</exception>
    /// <exception cref="ApplicationException">Не верный формат файла</exception>
    public void SaveToFile(string fileFormat, string fileName)
    { 
      if (fileFormat != null && fileName != null)
      {
        switch (fileFormat)
        { 
          // http://en.wikipedia.org/wiki/DOT_language (DOT format description)
          case "dot": 
          {
            //var graphFile   = 
            //new FileInfo(string.Format("state_graph{0}.dot", DateTime.Now));
            var graphFile   = new FileInfo(fileName);
            var graphWriter = graphFile.CreateText();
      
            graphWriter.WriteLine("/* This file generate automaticaly by FSM class function\n" +
                                " * Programmed by Islamov Denis (GET, MIPT)\n" +
                                " */\n\n" +
                                "digraph state {\n" +
                                "\tgraph [ratio=fill]\n" +
                                "\tsize=\"2\"\n" +
                                "\tnode [shape=circle style=filled]\n");
          
            foreach (var state in _states)
            { 
              StringBuilder stateKey = new StringBuilder(state.Key);
              stateKey.Replace('\\', '_'); 
              stateKey.Replace(' ', '_');
              
              graphWriter.WriteLine(string.Format("\t{0}[label=\"{1}\" color=lightblue2]",
                                                  stateKey, state.Key));
            }
            
            graphWriter.WriteLine();
            foreach (var events in _events)
            { 
              StringBuilder eventsValue = new StringBuilder(events.Value);
              eventsValue.Replace('\\', '_'); 
              eventsValue.Replace(' ', '_');
              
              graphWriter.WriteLine(_transitions.ContainsKey(events.Value)
                                     ? string.Format("\t{0}[label=\"{1}\" color=lightblue2]",
                                                     eventsValue, events.Key)
                                     : string.Format("\t{0}[label=\"{1}\" style=\"dotted\" color=lightblue2]",
                                                     eventsValue, events.Key));
            }
            graphWriter.WriteLine("}");
            graphWriter.Close();
            break;
          }
          default:
          {
            throw new ApplicationException("Format " + fileFormat +
                                           " doesn't support");
          }
        }
      }
      else if(fileFormat == null) 
      {
        throw new ArgumentNullException("fileFormat");
      }
      else if(fileName == null) 
      {
        throw new ArgumentNullException("fileName");
      }
    }

    /// <summary>
    /// Записать структуру КА в файл, с именем по умолчанию
    /// </summary>
    /// <param name="fileFormat">Формат файла (пока только dot)</param>
    public void SaveToFile(string fileFormat) 
    {
      StringBuilder fileNameAdd = new StringBuilder(DateTime.Now.ToString());
      fileNameAdd.Replace('/', '_');
      fileNameAdd.Replace(' ', '_');
      fileNameAdd.Replace(':', '_');
      
      this.SaveToFile(fileFormat, string.Format("state_graph_{0}.{1}", 
                                                fileNameAdd, fileFormat));
    }

    /// <summary>
    /// Записать структуру КА в файл в формате dot, с именем по умолчанию
    /// </summary>
    public void SaveToFile() 
    {
      this.SaveToFile("dot");
    } 
  }
}