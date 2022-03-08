using System;
using System.Collections.Generic;
using System.Threading;

using MR6100Api;

namespace ListenerFramework
{
    public class ReaderModel
    {
        /*
         * El proceso de uso es el siguiente:
         * se crea el objeto ---------- var lectora = ReaderModel("100.0.0.1",100);
         * nos conectamos a la antena-- lectora.conectar()
         * inicializamos variables----- lectora.inicializarVariables()
         * inciamos hilo obtencion----- Thread h = new Thread(new ThreadStart());
         */

        public string ip { get; set; }
        public int socket { get; set; }
        public bool accessGranted { get; set; }

        public volatile byte[,] tagBuff;
        public volatile byte readerPointer;
        public volatile byte insertPointer;

        public int getCount { get; set; }
        public int countTag { get; set; }
        public bool access { get; set; }
        public MR6100Api.MR6100Api reader { get; set; }





        public ReaderModel()
        {
            ip = "192.168.1.200";
            socket = 100;

            tagBuff = new byte[256, 12];
            insertPointer = 0;
            readerPointer = 0;

            accessGranted = false;
            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();
        }

        public ReaderModel(string internetProtocol, int puerto)
        {
            ip = internetProtocol;
            socket = puerto;
            accessGranted = false;

            tagBuff = new byte[256, 12];
            readerPointer = 0;
            insertPointer = 0;

            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();
        }

        public string Conectar()
        {
            if (reader.TcpConnectReader(ip,socket) == 2001)
            {
                accessGranted = true;
                return "Conexion Exitosa";
            }
            else
            {
                accessGranted = false;
                return "Fallo la conexion";
            }
        }

        public string Desconectar()
        {
            if (reader.TcpCloseConnect() == 2001)
            {
                accessGranted = false;
                return "Exitoso";
            }
            else
            {
                return "Fallido";
            }
        }

        public void InicializarVariables()
        {
            accessGranted = false;
            tagBuff = new byte[256, 12];
            readerPointer = 0;
            insertPointer = 0;
        }

        

        public void ReadIDsBoth()
        {
            /*
             * Esta funcion debe llamarse en un hilo, esto para que a la vez que se esta leyendo ambos tipos se pueda enviar
             * todo lo que se este leyendo, y despues se debe matar el hilo cerrando la conexion con la funcion
             * --------------desconectar()-------------------
             */
            byte[,] tagIDIso = new byte[1024, 12];
            int tagContadorIso = 0;
            int getcountIso = 0;
            int isoQuery;

            byte[,] tagIDEpc = new byte[1024, 12];
            int tagContadorEpc = 0;
            byte getcountEpc = 0;
            int epcQuery;

            int i;
            int j;

            while (accessGranted)
            {
                isoQuery = reader.IsoMultiTagIdentify(Int32.Parse(ip.Substring(ip.Length - 3)), ref tagIDIso, ref tagContadorIso, ref getcountIso);
                epcQuery = reader.EpcMultiTagIdentify(Int32.Parse(ip.Substring(ip.Length - 3)), ref tagIDEpc, ref tagContadorEpc, ref getcountEpc);

                if (epcQuery == 2001 && tagContadorEpc > 0)
                {
                    
                    for (i = 0; i<tagContadorEpc; i++)
                    {
                        for ( j = 0; j < 12; j++)
                        {
                            tagBuff[insertPointer, j] = tagIDEpc[i, j];
                        }
                        insertPointer++;
                    }
                    
                }
                if (isoQuery == 2001 && tagContadorIso > 0)
                {
                    for (i = 0; i < tagContadorIso; i++)
                    {
                        for ( j = 0; j < 12; j++)
                        {
                            tagBuff[insertPointer, j] = tagIDIso[i, j];
                        }
                        insertPointer++;
                    }
                }
            }

            
        }

        public void GetIDs()
        {
            /*
             * Esta funcion pretende mostrar en consola  los tags leidos no repetidos mientras aun se leen datos, esta originalmente
             * estaba en el main pero la idea es mostrar y reacciona lo mas rapido posible a las lecturas, esto solo puede 
             * lograrse mediante el uso de hilos, un hilo inserta los tags leidos y el otro los lee.
             */

            byte[] tagUnico = new byte[12];
            byte[] lastTag = new byte[12];
            int counter = 0;
            while (accessGranted)
            {
                
                if (readerPointer == insertPointer)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    counter = 0;
                    for (int w = 0; w < 12; w++)
                    {
                        lastTag[w] = tagUnico[w];
                        tagUnico[w] = tagBuff[readerPointer, w];
                        if (lastTag[w] == tagUnico[w]) { counter++; };
                    }
                    if (counter != 12) { Console.WriteLine(BitConverter.ToString(tagUnico)); }
                    
                    readerPointer++;
                }

            }

        }

        public void GetStreamIDs()
        {
            /*
             * Esta funcion pretende mostrar en consola todos los tags incluso repetidos es la misma base que el get_IDs sin el seguro de
             * los repetidos
             */

            byte[] tagUnico = new byte[12];
            
            while (accessGranted)
            {
                if (readerPointer == insertPointer)
                {
                    Thread.Sleep(1);

                }
                else
                {
                    for (int w = 0; w < 12; w++)
                    {
                        
                        tagUnico[w] = tagBuff[readerPointer, w];
                        
                    }
                    Console.WriteLine(BitConverter.ToString(tagUnico));
                    readerPointer++;
                }

            }

        }

    }
}
