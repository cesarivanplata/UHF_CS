using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MR6100Api;

namespace ListenerFramework
{
    class Program
    {
        public bool access = true;

        static void Main(string[] args)
        {
            int socket = 100;
            string aipi = "192.168.1.200";
            ReaderModel antena1 = new ReaderModel(aipi, socket);
            ReaderModel antena2 = new ReaderModel("192.168.1.201", 100);
            Thread hiloLeer = new Thread(() => antena1.ReadIDsBoth());
            Thread hiloEscribir = new Thread(() => antena1.GetStreamIDs());
            Thread hiloLeer2 = new Thread(() => antena2.ReadIDsBoth());
            Thread hiloEscribir2 = new Thread(() => antena2.GetStreamIDs());
            Console.WriteLine(antena1.Conectar());
            Console.WriteLine(antena2.Conectar());
            hiloLeer.Start();
            hiloLeer2.Start();
            hiloEscribir.Start();
            hiloEscribir2.Start();
            string a = Console.ReadLine();
            antena1.Desconectar();
            antena2.Desconectar();
            Console.WriteLine("Termina la lectura de Tags");
            Console.WriteLine("Presione cualquier tecla");
            a = Console.ReadLine();
            
        }
        
    }
}
