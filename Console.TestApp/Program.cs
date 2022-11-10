using System;
using System.Linq;
using System.Reflection;

namespace Console.TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region Index
            var appVersion = Assembly.GetExecutingAssembly().CustomAttributes
                .Where(w => w.AttributeType.Name.Contains("TargetFramework")).FirstOrDefault()
                .ConstructorArguments.FirstOrDefault().Value;
            System.Console.WriteLine($@"
           ____________________________________                
           |                                  |
           |    EasyMapper Test Programına    |
           |           Hoşgeldiniz            |                    
           |__________________________________|
           Framework : {appVersion}

Çalıştırmak istediğiniz Test Fonksiyon Numarasını yazınız.
");
            #endregion
            var testClass = new Test();
            var methods = ((TypeInfo)testClass.GetType()).DeclaredMethods
                /*.Where(w => !w.Name.Contains("<") && !w.Name.Contains(">"))*/.ToList();
            while (true)
            {
                for (int i = 0; i < methods.Count; i++)
                {
                    System.Console.WriteLine($"{i + 1}. {methods[i].Name}");
                }
                System.Console.Write($"Seçiminiz : ");
            Again:
                var select = System.Console.ReadKey();
                if (select.Key == ConsoleKey.Escape)
                    Environment.Exit(0);
                System.Console.WriteLine("");
                var status = int.TryParse(select.KeyChar.ToString(), out int index);
                if (!status)
                {
                    System.Console.WriteLine($"Nümerik bir seçim yapın!");
                    goto Again;
                }
                if (methods.Count < index || index == 0)
                {
                    System.Console.WriteLine($"Böyle bir method yok. Yeniden bir seçim yapın!");
                    goto Again;
                }
                var method = methods[index - 1];
                method.Invoke(testClass, new object[] { });
            }
        }
    }
}
