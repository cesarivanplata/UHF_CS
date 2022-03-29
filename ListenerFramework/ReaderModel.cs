using System;
using System.Collections.Generic;
using System.Threading;
using MR6100Api;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;


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

        /*varibles IP*/
        public string ip { get; set; }
        public IPAddress ipHost { get; set; }
        public int socketAntenna { get; set; }
        public int socketLocalHost { get; set; }

        /*Banderas*/
        public bool accessGranted { get; set; }
        public bool socketNullConectado { get; set; }
        public bool comNullConectado { get; set; }
        public int cantidadSalidas { get; set; }

        /*Buffers de control*/
        public volatile byte[,] TagBuff;
        public volatile byte ReaderPointer;
        public volatile byte InsertPointer;

        /* Variables de control a buffer*/
        public int getCount { get; set; }
        public int countTag { get; set; }
        public bool Access { get; set; } //no recurdo para que es esta

        /*Creacion de antenna*/
        public MR6100Api.MR6100Api reader { get; set; }

        /*varibles xocket*/
        public Socket sender;
        public IPEndPoint connect;

        /*Variables Com*/
        public int baudrate { get; set; }
        public SerialPort serialCOM;

        /***Delegados definicion y creacion***/
        /*Comunicacion de salida*/
        public delegate void Enviar(string mensaje);
        Enviar Envio;

        /*Conexionado*/
        public delegate string Conexiones();
        Conexiones Connectar;

        /*Desconectar*/




        public ReaderModel()
        {
            ip = "192.168.1.200";
            socketAntenna = 100;

            TagBuff = new byte[256, 12];
            InsertPointer = 0;
            ReaderPointer = 0;

            accessGranted = false;
            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();

            Envio = null;
            Connectar = null;
            cantidadSalidas = 0;
            configurarDelegados();

        }

        public ReaderModel(string internetProtocol, int puerto, string puertoCOM, int baudiosCOM)
        {
            ip = internetProtocol;
            socketAntenna = puerto;
            serialCOM = new SerialPort(puertoCOM, baudiosCOM);
            serialCOM.ReadTimeout = 500;
            serialCOM.WriteTimeout = 500;

            accessGranted = false;

            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;

            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();

            Envio = null;
            Connectar = null;
            cantidadSalidas = 1;
            configurarDelegados();

        }

        public ReaderModel(string internetProtocol, int puerto, int socketHost, string ipPropia)
        {
            ip = internetProtocol;
            socketAntenna = puerto;
            socketLocalHost = socketHost;
            ipHost = IPAddress.Parse(ipPropia);


            accessGranted = false;

            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;

            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();

            Envio = null;
            Connectar = null;
            cantidadSalidas = 2;
            configurarDelegados();

        }

        public ReaderModel(string internetProtocol, int puerto, int socketHost, string ipPropia, string puertoCOM, int baudiosCOM)
        {
            ip = internetProtocol;
            socketAntenna = puerto;

            serialCOM = new SerialPort(puertoCOM, baudiosCOM);
            serialCOM.ReadTimeout = 500;
            serialCOM.WriteTimeout = 500;

            socketLocalHost = socketHost;
            ipHost = IPAddress.Parse(ipPropia);

            accessGranted = false;

            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;

            countTag = new int();
            getCount = new byte();
            reader = new MR6100Api.MR6100Api();

            Envio = null;
            Connectar = null;
            cantidadSalidas = 3;
            configurarDelegados();

        }


        public string conectarAntenna()
        {

            if (reader.TcpConnectReader(ip, socketAntenna) == 2001)
            {
                accessGranted = true;

                return "Conexion socket Exitosa";
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

        public void inicializar()
        {
            accessGranted = false;
            TagBuff = new byte[256, 12];
            ReaderPointer = 0;
            InsertPointer = 0;

        }

        public void configurarDelegados()
        {
            Envio = null;
            Connectar = null;
            switch (cantidadSalidas)
            {
                case 1:
                    Envio += EnviarCom;
                    Envio += EnviarDefault;
                    Connectar += ConectarSerial;
                    Connectar += conectarAntenna;
                    break;
                case 2:
                    Envio += EnviarSocket;
                    Envio += EnviarDefault;
                    Connectar += ConectarSocket;
                    Connectar += conectarAntenna;
                    break;
                case 3:
                    Envio += EnviarCom;
                    Envio += EnviarSocket;
                    Envio += EnviarDefault;
                    Connectar += ConectarSocket;
                    Connectar += ConectarSerial;
                    Connectar += conectarAntenna;
                    break;
                default:
                    Connectar += conectarAntenna;
                    Envio += EnviarDefault;
                    break;
            }

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
                isoquery = reader.IsoMultiTagIdentify(Int32.Parse(ip.Substring(ip.Length - 3)), ref tagID_iso, ref tagContador_iso, ref getcount_iso);
                epcquery = reader.EpcMultiTagIdentify(Int32.Parse(ip.Substring(ip.Length - 3)), ref tagID_epc, ref tagContador_epc, ref getcount_epc);

                if (epcquery == 2001 && tagContador_epc > 0)
                {

                    for (i = 0; i < tagContador_epc; i++)
                    {
                        for (j = 0; j < 12; j++)
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
                        for (j = 0; j < 12; j++)
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
            string tagUnicoString;
            while (accessGranted)
            {

                if (ReaderPointer == InsertPointer)
                {
                    Thread.Sleep(1);
                    //Envio.Invoke("Equal");
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
                    if (counter != 12)
                    {
                        tagUnicoString = BitConverter.ToString(tagUnico);
                        Envio.Invoke(tagUnicoString);
                    }

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
            string tagUnicoString;

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
                    tagUnicoString = BitConverter.ToString(tagUnico);
                    Console.WriteLine(tagUnicoString);
                    serialCOM.WriteLine(tagUnicoString);

                    ReaderPointer++;
                }

            }

        }

        public string ConectarSerial()
        {
            try
            {
                serialCOM.Open();
                comNullConectado = true;
                return "Conexion Com Exitosa";
            }
            catch (Exception ex)
            {
                return "Conexion Com Fallida" + ex.ToString();
            }
        }

        public void EnviarCom(string mensaje)
        {
            if (comNullConectado)
            {
                serialCOM.WriteLine(mensaje);
            }
        }

        public void Read()
        {
            string message;
            while (accessGranted)
            {
                try
                {
                    message = serialCOM.ReadLine();
                    if (message == "BorraLista")
                    {

                    }
                }
                catch (TimeoutException)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public string ConectarSocket()
        {
            try
            {
                connect = new IPEndPoint(ipHost, socketLocalHost);
                sender = new Socket(ipHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(connect);
                socketNullConectado = true;
                return "Conexion Exitosa al socket";
            }
            catch (Exception ex)
            {
                return "Fallo Conexion al host:" + ex.ToString();
            }
        }

        public void DesconectarSocket()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void EnviarSocket(string mensaje)
        {
            if (socketNullConectado)
            {
                byte[] mensajeBytes = Encoding.ASCII.GetBytes(mensaje);
                int bytesEnviados = sender.Send(mensajeBytes);
            }
        }


        public void EnviarDefault(string mensaje)
        {
            Console.WriteLine(mensaje);
        }

        public void invocarDelegado()
        {
            Connectar.Invoke();
        }

    }
}
