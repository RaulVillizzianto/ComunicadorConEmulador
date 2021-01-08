using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Comunicador
{
    class Program
    {
        static Comunicador comunicador;
        static void Main(string[] args)
        {
            comunicador = new Comunicador();
            comunicador.OnComunicationStarted += Comunicador_OnComunicationStarted;
            comunicador.OnPackageChange += Comunicador_OnPackageChanged;
            comunicador.OnComunicationFinished += Comunicador_OnComunicationFinished;
        }
        
        private static void Comunicador_OnComunicationFinished(object sender, EventArgs e)
        {
            comunicador.BuscarEmulador();
        }

        private static void Comunicador_OnComunicationStarted(object sender, Comunicador.OnComunicationStartedEventArgs e)
        {
            Console.WriteLine("Comunicacion inicializada -> " + e.PaqueteInicial);
        }

        private static void Comunicador_OnPackageChanged(object sender, Comunicador.OnPackageChangedEventArgs e)
        {
            Console.WriteLine("Paquete cambiado -> " + e.FromPackage + " a: " + e.ToPackage);
        }
    }
}

