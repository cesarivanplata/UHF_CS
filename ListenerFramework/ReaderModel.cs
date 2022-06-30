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

        public volatile byte[,] TagBuff;
        public volatile byte ReaderPointer;
        public volatile byte InsertPointer;

        public int getcount { get; set; }
        public int count_tag { get; set; }
        public bool access { get; set; }
        public MR6100Api.MR6100Api reader { get; set; }





        public ReaderModel()
        {
            ip = "192.168.1.200";
            socket = 100;

            TagBuff = new byte[256, 12];
            InsertPointer = 0;
            ReaderPointer = 0;

            accessGranted = false;
            count_tag = new int();
            getcount = new byte();
            reader = new MR6100Api.MR6100Api();
        }

        public ReaderModel(string internetProtocol, int puerto)
        {
            ip = internetProtocol;
            socket = puerto;
            accessGranted = false;

            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;

            count_tag = new int();
            getcount = new byte();
            reader = new MR6100Api.MR6100Api();
        }

        public string conectar()
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

        public string desconectar()
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

        public void inicializarVariables()
        {
            accessGranted = false;
            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;
        }

        public void readIDs_both()
        {
            /*
             * Esta funcion debe llamarse en un hilo, esto para que a la vez que se esta leyendo ambos tipos se pueda enviar
             * todo lo que se este leyendo, y despues se debe matar el hilo cerrando la conexion con la funcion
             * --------------desconectar()-------------------
             */
            byte[,] tagID_iso = new byte[1024, 12];
            int tagContador_iso = 0;
            int getcount_iso = 0;
            int isoquery;

            byte[,] tagID_epc = new byte[1024, 12];
            int tagContador_epc = 0;
            byte getcount_epc = 0;
            int epcquery;

            int i;
            int j;

            while (accessGranted)
            {
                isoquery = reader.IsoMultiTagRead(Int32.Parse(ip.Substring(ip.Length - 3)),9, ref tagID_iso, ref tagContador_iso, ref getcount_iso);
                epcquery = reader.EpcMultiTagIdentify(Int32.Parse(ip.Substring(ip.Length - 3)), ref tagID_epc, ref tagContador_epc, ref getcount_epc);
				//epcquery = reader.EpcRead(Int32.Parse(ip.Substring(ip.Length - 3)), 3,0x09,0x04,1);


                if (epcquery == 2001 && tagContador_epc > 0)
                {
                    
                    for (i = 0; i<tagContador_epc; i++)
                    {
                        for ( j = 0; j < 12; j++)
                        {
                            TagBuff[InsertPointer, j] = tagID_epc[i, j];
                        }
                        InsertPointer++;
                    }
                    
                }
                if (isoquery == 2001 && tagContador_iso > 0)
                {
                    for (i = 0; i < tagContador_iso; i++)
                    {
                        for ( j = 0; j < 12; j++)
                        {
                            TagBuff[InsertPointer, j] = tagID_iso[i, j];
                        }
                        InsertPointer++;
                    }
                }
            }

            
        }
        public void get_IDs()
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
                
                if (ReaderPointer == InsertPointer)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    counter = 0;
                    for (int w = 0; w < 12; w++)
                    {
                        lastTag[w] = tagUnico[w];
                        tagUnico[w] = TagBuff[ReaderPointer, w];
                        if (lastTag[w] == tagUnico[w]) { counter++; };
                    }
                    if (counter != 12) { Console.WriteLine(BitConverter.ToString(tagUnico)); }
                    
                    ReaderPointer++;
                }

            }

        }

        public void getstream_IDs()
        {
            /*
             * Esta funcion pretende mostrar en consola todos los tags incluso repetidos es la misma base que el get_IDs sin el seguro de
             * los repetidos
             */

            byte[] tagUnico = new byte[12];
            
            
            while (accessGranted)
            {

                if (ReaderPointer == InsertPointer)
                {
                    Thread.Sleep(1);

                }
                else
                {
                    
                    for (int w = 0; w < 12; w++)
                    {
                        
                        tagUnico[w] = TagBuff[ReaderPointer, w];
                        
                    }
                    Console.WriteLine(BitConverter.ToString(tagUnico));

                    ReaderPointer++;
                }

            }

        }

    }
}
