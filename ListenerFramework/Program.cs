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
            if (args == null)
            {
                Console.WriteLine("Faltan argumentos");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    switch (argument)
                    {
                        case "-ip":
                            aipi = args[i + 1];
                            i++;
                            break;
                        case "-s":
                            socket = Convert.ToInt32(args[i + 1]);
                            i++;
                            break;
                        default:
                            Console.WriteLine("argumentos invalidos");
                            break;
                    }
                        
                }
            }
            Console.WriteLine("Conectandose a IP: {0}, por el puerto: {1}", aipi, socket);
            
            ReaderModel antena1 = new ReaderModel(aipi, socket, 4600);
            //ReaderModel antena2 = new ReaderModel("192.168.1.201", 100,4600);
            Thread hiloLeer = new Thread(() => antena1.ReadIDsBoth());
            Thread hiloEscribir = new Thread(() => antena1.GetStreamIDs());
            //Thread hiloLeer2 = new Thread(() => antena2.ReadIDsBoth());
            //Thread hiloEscribir2 = new Thread(() => antena2.GetStreamIDs());
            Console.WriteLine(antena1.Conectar());
            //Console.WriteLine(antena2.Conectar());
            hiloLeer.Start();
            //hiloLeer2.Start();
            hiloEscribir.Start();
            //hiloEscribir2.Start();
            string a = Console.ReadLine();
            antena1.Desconectar();
            //antena2.Desconectar();
            Console.WriteLine("Termina la lectura de Tags");
            Console.WriteLine("Presione cualquier tecla");
            a = Console.ReadLine();
            
        }
        
    }
}
