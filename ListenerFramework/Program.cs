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
            //ReaderModel antena2 = new ReaderModel("192.168.1.200", 100);
            Thread hiloLeer = new Thread(() => antena1.readIDs_both());
            Thread hiloEscribir = new Thread(() => antena1.getstream_IDs());
            //Thread hiloLeer2 = new Thread(() => antena2.readIDs_both());
            //Thread hiloEscribir2 = new Thread(() => antena2.getstream_IDs());
            Console.WriteLine(antena1.conectar());
            //Console.WriteLine(antena2.conectar());
            hiloLeer.Start();
            //hiloLeer2.Start();
            hiloEscribir.Start();
            //hiloEscribir2.Start();
            string a = Console.ReadLine();
            antena1.desconectar();
            //antena2.desconectar();
            Console.WriteLine("Termina la lectura de Tags");
            Console.WriteLine("Presione cualquier tecla");
            a = Console.ReadLine();
            
        }
        
    }
}
