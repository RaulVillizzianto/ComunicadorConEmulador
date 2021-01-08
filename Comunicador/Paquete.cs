using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paquetes
{
    public class Paquete
    {
        public enum Paquetes
        {
            INICIO_COMUNICACION,
            FIN_COMUNICACION,
            PAQUETE_DESCONOCIDO,
            SOLICITAR_PACKAGE_EN_EJECUCION,
            SOLICITAR_APP_INSTALADA,
            CONFIRMAR_FIN_COMUNICACION
        }

        public enum PaquetesRespuesta
        {
            CONFIRMAR_INICIO_COMUNICACION,
            INICIO_COMUNICACION,
            CONFIRMAR_FIN_COMUNICACION,
            RTA_SOLICITUD_PACKAGE_EN_EJECUCION,
            RTA_SOLICITUD_CONFIRMADA,
            RTA_SOLICITUD_RECHAZADA,
            EVENTO_CAMBIO_PACKAGE_EN_EJECUCION,
            FIN_COMUNICACION,
            PAQUETE_DESCONOCIDO
        }

        public Paquete()
        {

        }

        private string Data;


        string UltimoPaquete;
        List<string> contenido;
        Paquetes Tipo;
        PaquetesRespuesta TipoRespuesta;
        public Paquetes ObtenerTipoDePaquete()
        {
            return Tipo;
        }

        public Paquete(string data)
        {
            this.Data = data;
        }
        private const string InicioComunicacion = "IniciandoComunicacion";
        private const string FinComunicacion = "FinalizandoComunicacion";
        public string CrearPaquete(string ip, Paquetes p, string Data)
        {
            this.Tipo = p;
            this.Data = Data;
            return UltimoPaquete = ip + ">" + p + ">" + Data;
        }


        public string CrearPaquete(string ip, Paquetes p)
        {
            this.Tipo = p;
            return UltimoPaquete = ip + ">" + p;
        }

        public string CrearPaquete(string ip, string Data)
        {
            this.Data = Data;

            return UltimoPaquete = ip + ">" + Data;
        }


        public string ObtenerPaquete()
        {
            return UltimoPaquete == null ? null : UltimoPaquete;
        }

        public string GetIp()
        {
            return Data.Split(">").First();
        }

        public List<string> ObtenerContenido()
        {
            return contenido;
        }

        public string ObtenerData()
        {
            return Data;
        }


        public PaquetesRespuesta LeerPaquete()
        {
            string[] info = Data.Split(">");
            contenido = new List<string>();
            if (info.Count() >= 2)
            {
                for (int i = 2; i < info.Length; i++)
                {
                    contenido.Add(info[i]);
                }
                if (info.ElementAt(1).CompareTo(PaquetesRespuesta.CONFIRMAR_INICIO_COMUNICACION.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.CONFIRMAR_INICIO_COMUNICACION;
                    return PaquetesRespuesta.CONFIRMAR_INICIO_COMUNICACION;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.FIN_COMUNICACION.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.FIN_COMUNICACION;
                    return PaquetesRespuesta.FIN_COMUNICACION;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.RTA_SOLICITUD_PACKAGE_EN_EJECUCION.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.RTA_SOLICITUD_PACKAGE_EN_EJECUCION;
                    return PaquetesRespuesta.RTA_SOLICITUD_PACKAGE_EN_EJECUCION;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.EVENTO_CAMBIO_PACKAGE_EN_EJECUCION.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.EVENTO_CAMBIO_PACKAGE_EN_EJECUCION;
                    return PaquetesRespuesta.EVENTO_CAMBIO_PACKAGE_EN_EJECUCION;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.INICIO_COMUNICACION.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.INICIO_COMUNICACION;
                    return PaquetesRespuesta.INICIO_COMUNICACION;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.RTA_SOLICITUD_CONFIRMADA.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.RTA_SOLICITUD_CONFIRMADA;
                    return PaquetesRespuesta.RTA_SOLICITUD_CONFIRMADA;
                }
                else if (info.ElementAt(1).CompareTo(PaquetesRespuesta.RTA_SOLICITUD_RECHAZADA.ToString()) == 0)
                {
                    TipoRespuesta = PaquetesRespuesta.RTA_SOLICITUD_RECHAZADA;
                    return PaquetesRespuesta.RTA_SOLICITUD_RECHAZADA;
                }
            }
            return PaquetesRespuesta.PAQUETE_DESCONOCIDO;
        }
        public void LeerPaquete(out List<string> contenido)
        {
            string[] info = Data.Split(">");
            contenido = new List<string>();
            if (info.Count() >= 2)
            {
                for (int i = 1; i < info.Length; i++)
                {
                    contenido.Add(info[i]);
                }
                return;
            }
            else return;
        }
        public string ObtenerInformacionDelPaquete(string paquete)
        {
            string[] info = paquete.Split(">");
            string contenido = null;
            if (info.Count() >= 2)
            {
                for (int i = 1; i < info.Length; i++)
                {
                    contenido.Concat(">" + info[i]);
                }
                return contenido;
            }
            else return null;
        }
    }
}
