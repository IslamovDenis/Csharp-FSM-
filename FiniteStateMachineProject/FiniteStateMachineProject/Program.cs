using System;

namespace FiniteStateMachineProject
{
  class Program
  {
    // Функции состояний и переходов
    public static void StartState()
    {
      Console.WriteLine("Чайник снят!");
    }

    public static void Water()
    {
      Console.WriteLine("Вода налита!");
    }

    public static void WaterOut()
    {
      Console.WriteLine("Вылить воду");
    }

    public static void Cooker()
    {
      Console.WriteLine("Плита включена и чайник скоро вскипит!");
    }

    public static void Solution()
    {
      Console.WriteLine("Cвести задачу к предыдущей");
    }

    public static void TransferWater()
    {
      Console.WriteLine("Наливаю воду...");
    }

    public static void TransferCooker()
    {
      Console.WriteLine("Включаю плиту...");
    }

    public static void TransferWaterOut()
    {
      Console.WriteLine("Выливаю воду...");
    }

    static void Main()
    {
      var fsm = new FiniteStateMachine("Снять чайник", StartState);
      // Добавляем состояния
      fsm.AddState("Налить воды", Water);
      fsm.AddState("Вылить воду", WaterOut);
      fsm.AddState("Включить плиту и вскипятить", Cooker);
      fsm.AddState("Cвести задачу к предыдущей", Solution);

      fsm.AddTransition("Снять чайник", "Налить воды", TransferWater);
      fsm.AddTransition("Вылить воду", "Снять чайник", TransferWaterOut);
      fsm.AddTransition("Налить воды", "Включить плиту и вскипятить", TransferCooker);
      fsm.AddTransition("Снять чайник", "Включить плиту и вскипятить", TransferCooker);

      fsm.AddEvent("Чайник пуст", "Снять чайник", "Налить воды");
      fsm.AddEvent("Чайник полон (мат)", "Снять чайник", "Cвести задачу к предыдущей");
      fsm.AddEvent("Чайник полон (физ)", "Снять чайник", "Включить плиту и вскипятить");
      fsm.AddEvent("Я это уже делал(а)!", "Cвести задачу к предыдущей", "Вылить воду");
      fsm.AddEvent("Вылить воду", "Вылить воду", "Снять чайник");
      fsm.AddEvent("Вскипятить", "Налить воды", "Включить плиту и вскипятить");

      /*
      fsm.EventHappen("Чайник пуст");
      fsm.Invoke();
      fsm.EventHappen("finish transition");
      fsm.Invoke();
      fsm.EventHappen("Вскипятить");
      fsm.Invoke();
      fsm.EventHappen("finish transition");
      fsm.Invoke();
      */

      Console.WriteLine("\nЯ - математик! T_T ");
      fsm.EventHappen("Чайник полон (мат)");
      fsm.Invoke();
      fsm.EventHappen("Я это уже делал(а)!");
      fsm.Invoke();
      fsm.EventHappen("Вылить воду");
      fsm.Invoke();
      // Вот она, рекурсия
      fsm.EventHappen("Чайник пуст");
      fsm.Invoke();
      fsm.EventHappen("finish transition");
      fsm.Invoke();
      fsm.EventHappen("Вскипятить");
      fsm.Invoke();
      fsm.EventHappen("finish transition");
      fsm.Invoke();

      // Разрешаем откатываться к начальному состоянию
      fsm.CanReset = true;
      fsm.Reset();

      Console.WriteLine("\nЯ - физик! O_o ");
      fsm.EventHappen("Чайник полон (физ)");
      fsm.Invoke();
      fsm.EventHappen("finish transition");
      fsm.Invoke();

      // Запрещаем откатываться к начальному состоянию
      fsm.CanReset = false;

      fsm.SaveToFile("dot"); // or use SaveToFile();
      Console.ReadLine();
    }
  }
}