
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Paquetes;
namespace Comunicador
{
    class Comunicador
    {
        string LocalIPAddress;
        private string IP_Emulador;
        private int Puerto_Emulador;
        bool EsperandoRespuesta;
        bool EsperandoConfirmacionDelEmulador = true;
        bool RespuestaSolicitud;

        string PackageEnEjecucion;
        Queue<Paquete> PaquetesEnEspera = new Queue<Paquete>();
        Socket sock;

        public event EventHandler<OnComunicationStartedEventArgs> OnComunicationStarted;
        public event EventHandler<OnPackageChangedEventArgs> OnPackageChange;
        public event EventHandler<EventArgs> OnComunicationFinished;

        public Comunicador()
        {
            IP_Emulador = "127.0.0.1";
            Puerto_Emulador = 4445;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Thread lectordepaquetes = new Thread(LectorDePaquetes);
            lectordepaquetes.Start();
            Thread coladepaquetes = new Thread(ColaDePaquetes);
            coladepaquetes.Start();
            BuscarEmulador();
        }
        public Comunicador(string IP_Emulador)
        {
            this.IP_Emulador = IP_Emulador;
            Puerto_Emulador = 4445;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Thread lectordepaquetes = new Thread(LectorDePaquetes);
            lectordepaquetes.Start();
            Thread coladepaquetes = new Thread(ColaDePaquetes);
            coladepaquetes.Start();
            BuscarEmulador();
        }
        public Comunicador(string IP_Emulador, int puerto)
        {
            this.IP_Emulador = IP_Emulador;
            Puerto_Emulador = puerto;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Thread lectordepaquetes = new Thread(LectorDePaquetes);
            lectordepaquetes.Start();
            Thread coladepaquetes = new Thread(ColaDePaquetes);
            coladepaquetes.Start();
            BuscarEmulador();
        }
        public void BuscarEmulador()
        {
            if (EsperandoConfirmacionDelEmulador == true)
            {
                while (EsperandoConfirmacionDelEmulador)
                {
                    if (EsperandoConfirmacionDelEmulador)
                    {
                        Console.WriteLine("Buscando emulador...");
                        foreach (string ip in ObtenerIPLocales())
                        {
                            EnviarPaquete(ip, Paquete.Paquetes.INICIO_COMUNICACION, true);
                        }
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(1500);
                }
            }
        }

        Paquete ultimoPaquete;

        public Paquete ObtenerUltimoPaquete()
        {
            return ultimoPaquete;
        }

        private void ColaDePaquetes()
        {

            while (true)
            {
                if (PaquetesEnEspera.Count > 0 && EsperandoConfirmacionDelEmulador == false)
                {
                    var paquete = PaquetesEnEspera.First();
                    if (paquete.ObtenerData() != null)
                    {
                        PaquetesEnEspera.First().CrearPaquete(LocalIPAddress, paquete.ObtenerTipoDePaquete(), paquete.ObtenerData());
                    }
                    else
                    {
                        PaquetesEnEspera.First().CrearPaquete(LocalIPAddress, paquete.ObtenerTipoDePaquete());
                    }
                    EnviarPaquete(PaquetesEnEspera.Dequeue());
                }
                Thread.Sleep(100);
            }
        }




        private void LectorDePaquetes()
        {
            UdpClient receivingUdpClient = new UdpClient(4445);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    ultimoPaquete = new Paquete(returnData);
                    Console.WriteLine(returnData);
                    switch (ultimoPaquete.LeerPaquete())
                    {
                        case Paquete.PaquetesRespuesta.INICIO_COMUNICACION:
                            {
                                PackageEnEjecucion = ultimoPaquete.ObtenerContenido().ElementAt(0);
                                OnComunicationStarted?.Invoke(this, new OnComunicationStartedEventArgs(PackageEnEjecucion));
                                break;
                            }
                        case Paquete.PaquetesRespuesta.CONFIRMAR_INICIO_COMUNICACION:
                            {
                                Console.WriteLine("Comunicacion confirmada a través de la IP:" + ultimoPaquete.GetIp());
                                LocalIPAddress = ultimoPaquete.GetIp();
                                EsperandoConfirmacionDelEmulador = false;
                                break;
                            }
                        case Paquete.PaquetesRespuesta.RTA_SOLICITUD_PACKAGE_EN_EJECUCION:
                            {
                                PackageEnEjecucion = ultimoPaquete.ObtenerContenido().ElementAt(0);
                                if (EsperandoRespuesta == true)
                                {
                                    EsperandoRespuesta = false;
                                }
                                break;
                            }
                        case Paquete.PaquetesRespuesta.EVENTO_CAMBIO_PACKAGE_EN_EJECUCION:
                            {
                                if (!ultimoPaquete.ObtenerContenido().Contains("null"))
                                {
                                    OnPackageChange?.Invoke(this, new OnPackageChangedEventArgs(ultimoPaquete.ObtenerContenido().ElementAt(0), ultimoPaquete.ObtenerContenido().ElementAt(1)));
                                    PackageEnEjecucion = ultimoPaquete.ObtenerContenido().ElementAt(1);
                                }
                                break;
                            }
                        case Paquete.PaquetesRespuesta.RTA_SOLICITUD_CONFIRMADA:
                            {
                                RespuestaSolicitud = true;
                                if (EsperandoRespuesta == true)
                                {
                                    EsperandoRespuesta = false;
                                }
                                break;
                            }
                        case Paquete.PaquetesRespuesta.RTA_SOLICITUD_RECHAZADA:
                            {
                                RespuestaSolicitud = false;
                                if (EsperandoRespuesta == true)
                                {
                                    EsperandoRespuesta = false;
                                }
                                break;
                            }
                        case Paquete.PaquetesRespuesta.FIN_COMUNICACION:
                            {
                                OnComunicationFinished?.Invoke(this, null);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(100);
            }
        }
        private List<string> ObtenerIPLocales(bool incluir_ipv6 = false)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST   
            string myIP = Dns.GetHostByName(hostName).AddressList[1].ToString();
            List<string> IPS = new List<string>();
            foreach (var ipdata in Dns.GetHostByName(hostName).AddressList)
            {
                switch (ipdata.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        IPS.Add(ipdata.ToString());
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        if(incluir_ipv6)
                        {
                            IPS.Add(ipdata.ToString());
                        }
                        break;
                    default:
                        break;
                }

            }
            return IPS;
        }
        public bool EstaLaAppInstalada(string App)
        {
            EnviarPaquete(Paquete.Paquetes.SOLICITAR_APP_INSTALADA, App);
            EsperarHastaObtenerRespuesta();
            return RespuestaSolicitud;
        }

        public string ObtenerPackageEnEjecucion()
        {
            EnviarPaquete(Paquete.Paquetes.SOLICITAR_PACKAGE_EN_EJECUCION);
            EsperarHastaObtenerRespuesta();
            return PackageEnEjecucion;
        }

        private void EsperarHastaObtenerRespuesta()
        {
            EsperandoRespuesta = true;
            while (EsperandoRespuesta)
            {
                Thread.Sleep(10);
            }
        }

        private void EnviarPaquete(Paquete p)
        {
            if (EsperandoConfirmacionDelEmulador == false && p.ObtenerPaquete() != null)
            {
                IPAddress serverAddr = IPAddress.Parse(IP_Emulador);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
        }



        public void EnviarPaquete(string Data)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(LocalIPAddress, Data);
            if (EsperandoConfirmacionDelEmulador == false)
            {
                IPAddress serverAddr = IPAddress.Parse(IP_Emulador);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }
        public void EnviarPaquete(Paquete.Paquetes paquete)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(LocalIPAddress, paquete);
            if (EsperandoConfirmacionDelEmulador == false)
            {
                IPAddress serverAddr = IPAddress.Parse(IP_Emulador);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }

        public void EnviarPaquete(Paquete.Paquetes paquete, string Data)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(LocalIPAddress, paquete, Data);
            if (EsperandoConfirmacionDelEmulador == false)
            {
                IPAddress serverAddr = IPAddress.Parse(IP_Emulador);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }


        public void EnviarPaquete(string server_ip_addr, string from_ip_addr, Paquete.Paquetes paquete)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(from_ip_addr, paquete);
            if (EsperandoConfirmacionDelEmulador == false)
            {
                IPAddress serverAddr = IPAddress.Parse(server_ip_addr);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }
        public void EnviarPaquete(string server_ip_addr, int port, string from_ip_addr, Paquete.Paquetes paquete)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(from_ip_addr, paquete);
            if (EsperandoConfirmacionDelEmulador == false)
            {
                IPAddress serverAddr = IPAddress.Parse(server_ip_addr);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, port);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }
        public void EnviarPaquete(string from_ip_addr, Paquete.Paquetes paquete, bool buscandoemulador = false)
        {
            Paquete p = new Paquete();
            p.CrearPaquete(from_ip_addr, paquete);
            if (EsperandoConfirmacionDelEmulador == false || buscandoemulador)
            {
                IPAddress serverAddr = IPAddress.Parse(IP_Emulador);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, Puerto_Emulador);
                sock.SendTo(Encoding.ASCII.GetBytes(p.ObtenerPaquete()), endPoint);
                Console.WriteLine("Paquete enviado ->" + p.ObtenerPaquete());
            }
            else PaquetesEnEspera.Enqueue(p);
        }
        public class OnPackageChangedEventArgs : EventArgs
        {
            private readonly string _FromPackage;
            private readonly string _ToPackage;
            public OnPackageChangedEventArgs(string FromPackage, string ToPackage)
            {
                this._FromPackage = FromPackage;
                this._ToPackage = ToPackage;
            }

            public string FromPackage
            {
                get { return this._FromPackage; }
            }
            public string ToPackage
            {
                get { return this._ToPackage; }
            }
        }
        public class OnComunicationStartedEventArgs : EventArgs
        {
            private readonly string _Package;
            public OnComunicationStartedEventArgs(string PaqueteInicial)
            {
                this._Package = PaqueteInicial;
            }

            public string PaqueteInicial
            {
                get { return this._Package; }
            }
        }
    }
}

