using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/* <summary>
 * Файл FiniteStateMachine.cs
 * Автор: Исламов Денис, ОАО "ДЖЕТ"
 * Вид лицензии: GNU LGPL 
 *
 * TODO
 * 1). Добавить запрет на разбиения на несвязный граф
 * 2). Матрицы событий
 * 3). Внешний класс - отображение 
 *     (и возможность редактирования) в Юнити
 * 4). Экспорт / импорт (пока только сохранение в *.dot)
 */
namespace FiniteStateMachineProject
{
  /// <summary>
  /// Класс - Конечный детерминированный автомат
  /// </summary>
  public class FiniteStateMachine
  {
    // Переход между событиями записывается как "first_state->second_state"
    private readonly Dictionary<string, State> _states;      // имя состояния, функция 
    private readonly Dictionary<string, Transition> _transitions; // имя состояния -> cледующие состояния, функция перехода
    private readonly Dictionary<string, string> _events;      // имя события, имя состояния -> cледующие состояния

    private readonly string _startStateName;

    private bool _transitionState,
                 _stopped;

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
    /// Имя предыдущего события, только get
    /// </summary>
    public string PrevEventName { get; private set; }

    /// <summary>
    /// Имя текущего события, только get
    /// </summary>
    public string CurrenEventName { get; private set; }

    /// <summary>
    /// Константа - конец перехода
    /// </summary>
    public static string FinishTransition { get; set; }

    /// <summary>
    /// Возможно ли делать откат КА?
    /// </summary>
    public bool CanReset { get; set; }

    /// <summary>
    /// Остановка КА
    /// </summary>
    public void Stop()
    {
      _stopped = true;
    }

    /// <summary>
    /// Продолжение работы КА
    /// </summary>
    public void Continue()
    {
      _stopped = true;
    }

    /// <summary>
    /// Конструктор - инициализация словарей
    /// </summary>
    /// <param name="startStateName">Начальное состояние оператора</param>
    /// <param name="stateFunc">Функция отвечающие за начальное состояние</param>
    /// <exception cref="ArgumentNullException">Аргумент currentStateName = null</exception>
    /// <exception cref="ArgumentNullException">Аргумент stateFunc = null</exception>
    public FiniteStateMachine(string startStateName, State stateFunc)
    {
      if (startStateName != null && stateFunc != null)
      {
        CurrenStateName = startStateName;
        PrevStateName = startStateName;
        _startStateName = startStateName;

        PrevEventName = "";
        CurrenEventName = "";

        FinishTransition = "finish transition";

        _transitionState = false;

        _states = new Dictionary<string, State>();
        _transitions = new Dictionary<string, Transition>();
        _events = new Dictionary<string, string>();

        var execFunc = new State(stateFunc);
        _states.Add(startStateName, execFunc);
        execFunc.Invoke();
      }
      else if (startStateName == null)
      {
        throw new ArgumentNullException("startStateName");
      }
      else // if (stateFunc == null)
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
    /// <exception cref="ArgumentNullException">Аргумент eventName = null</exception>
    /// <exception cref="ApplicationException">Состояние - не переходное</exception>
    public void EventHappen(string eventName)
    {
      if (eventName != null)
      {
        // Если происходит одно и тоже событие - делаем одно и тоже
        // Добавлено, для событий крутящихся в цикле
        if (eventName != FinishTransition)
        {
          if (_events.ContainsKey(eventName) &&
              LeftPart(_events[eventName]) == CurrenStateName)
          {
            // Если происходит одно и тоже событие - делаем одно и тоже!
            // Добавлено, для событий крутящихся в цикле
            if (PrevEventName != eventName)
            {
              PrevStateName = CurrenStateName;               // a = b
              CurrenStateName = RightPart(_events[eventName]); // b = следующие ребро
            }

            // выполняем переход, если таковой есть
            if (_transitions.ContainsKey(PrevStateName + "->" + CurrenStateName))
            {
              _transitionState = true;
              // _transitions[PrevStateName + "->" + CurrenStateName].Invoke();
            }
            else // В противном случае - переключаемся на состояние
            {
              _transitionState = false;
              // _states[CurrenStateName].Invoke();
            }
          }
          else if (!_events.ContainsKey(eventName))
          {
            throw new KeyNotFoundException("Can't find \"" + eventName +
                                           "\" in events collection");
          }
          else if (RightPart(_events[eventName]) != CurrenStateName)
          {
            throw new ApplicationException("Event \"" + eventName +
                                           "\"(" + _events[eventName] + ")" +
                                           "\" haven't connection with current state " +
                                           CurrenStateName);
          }
        }
        else if (PrevEventName != FinishTransition)
        {
          if (_transitionState)
          {
            _transitionState = false;
            // _states[CurrenStateName].Invoke();
          }
          else
          {
            throw new ApplicationException("Current state \"" + CurrenStateName +
                                           "\" - not transition");
          }
        }
        else // PrevEventName == FINISH_TRANSITION && eventName == FINISH_TRANSITION
        {
          throw new ApplicationException("Two transitions without state");
        }
      }
      else // eventName == null
      {
        throw new ArgumentNullException("eventName");
      }
    }

    /// <summary>
    /// Вызов функции делегата текущего состояния / перехода
    /// </summary>
    public void Invoke()
    {
      if (_stopped == false)
      {
        if (_transitionState)
        {
          _transitions[PrevStateName + "->" + CurrenStateName].Invoke();
        }
        else
        {
          _states[CurrenStateName].Invoke();
        }
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
      if (stateName != null && stateFunc != null)
      {
        if (!_states.ContainsKey(stateName))
        {
          var execFunc = new State(stateFunc);
          _states.Add(stateName, execFunc);
        }
        else  // (_states.ContainsKey(stateName))
        {
          throw new ArgumentException("State \"" + CurrenStateName +
                                      "\" already present in states collection");
        }
      }
      else if (stateName == null)
      {
        throw new ArgumentNullException("stateName");
      }
      else // if (stateFunc == null)
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
        throw new KeyNotFoundException("Can't find \"" + stateName +
                                       "\" in states collection");
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
          throw new ArgumentException("Transition \"" + startState +
                                      "\" already present in transitions collection");
        }
      }

      else if (!_states.ContainsKey(startState))
      {
        throw new KeyNotFoundException("Can't find state \"" + startState +
                                       "\" in states collection");
      }
      else if (!_states.ContainsKey(endState))
      {
        throw new KeyNotFoundException("Can't find state \"" + endState +
                                       "\" in states collection");
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
      else // if (endState == null)
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
      if (transitionName != null)
      {
        if (_transitions.ContainsKey(transitionName))
        {
          _transitions.Remove(transitionName);
        }
        else // !_transitions.ContainsKey(transitionName)
        {
          throw new KeyNotFoundException("Can't find \"" + transitionName +
                                         "\" in transitions collection");
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
          throw new ArgumentException("Event \"" + eventName +
                                      "\" already present in transitions collection");
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
    public static string LeftPart(string edge)
    {
      var indexOfQuote = edge.IndexOf('>');
      if (indexOfQuote != -1)
      {
        var tmpForCopy = new char[indexOfQuote - 1]; // -2, т.к. есть еще '-'
        edge.CopyTo(0, tmpForCopy, 0, indexOfQuote - 1);

        return new string(tmpForCopy);
      }
      // else if (indexOfQuote == -1)
      throw new ApplicationException("Can't find '->' symbols in string " +
                                       edge);
    }

    /// <summary>
    /// Вспомогательная функция
    /// </summary>
    /// <param name="edge">Названия ребра, связывающего состояния (a->b)</param>
    /// <returns>Правая вершина (конец ребра) или null, если таковой нет</returns>
    public static string RightPart(string edge)
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
    /// <param name="fileName">Имя файла (указывайте расширение соответственно формату)</param>
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
              var graphFile = new FileInfo(fileName);
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
                var stateKey = new StringBuilder(state.Key);
                stateKey.Replace('\\', '_');
                stateKey.Replace(' ', '_');

                // ReSharper disable RedundantStringFormatCall
                graphWriter.WriteLine(string.Format("\t{0}[label=\"{1}\" color=lightblue2]",
                                                    stateKey, state.Key));
                // ReSharper restore RedundantStringFormatCall
              }

              graphWriter.WriteLine();
              foreach (var events in _events)
              {
                var eventsValue = new StringBuilder(events.Value);
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
      else if (fileFormat == null)
      {
        throw new ArgumentNullException("fileFormat");
      }
      else // if (fileName == null)
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
      var fileNameAdd = new StringBuilder(DateTime.Now.ToString());
      fileNameAdd.Replace('/', '_');
      fileNameAdd.Replace(' ', '_');
      fileNameAdd.Replace(':', '_');

      SaveToFile(fileFormat, string.Format("state_graph_{0}.{1}",
                                                fileNameAdd, fileFormat));
    }

    /// <summary>
    /// Записать структуру КА в файл в формате dot, с именем по умолчанию
    /// </summary>
    public void SaveToFile()
    {
      SaveToFile("dot");
    }
  }
}
