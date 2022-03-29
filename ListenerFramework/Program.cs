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
            string ipPropia = "192.168.1.100";
            int socketHostPort = 6400;
            string puertoSerial = "COM5";
            int baudios = 9600;

            //ReaderModel antena1 = new ReaderModel(aipi, socket, puertoSerial, baudios);
            //ReaderModel antena2 = new ReaderModel(aipi,socket, socketHost, ipPropia, puertoSerial, baudios );
            ReaderModel antena1 = new ReaderModel(aipi, socket, socketHostPort, ipPropia);


            Thread hiloLeer = new Thread(() => antena1.readIDs_both());
            Thread hiloEscribir = new Thread(() => antena1.get_IDs());
            Thread hiloCOMLeer = new Thread(() => antena1.Read());

            antena1.invocarDelegado();

            hiloLeer.Start();
            hiloEscribir.Start();

            string a = Console.ReadLine();
            antena1.desconectar();

            Console.WriteLine("Termina la lectura de Tags");
            Console.WriteLine("Presione cualquier tecla");
            a = Console.ReadLine();

        }
        
    }
}
