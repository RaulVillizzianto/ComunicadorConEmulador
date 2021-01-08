package com.example.comunicador2;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.Service;
import android.app.usage.UsageStats;
import android.app.usage.UsageStatsManager;
import android.content.ActivityNotFoundException;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Build;
import android.os.IBinder;
import android.util.Log;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.RequiresApi;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.util.Iterator;
import java.util.List;
import java.util.SortedMap;
import java.util.TreeMap;
import java.util.Vector;

public class Comunicador extends Service {

    static final int UdpServerPORT = 4445;
    UdpServerThread udpServerThread;
    UdpEventThread udpEventThread;
    Context context;
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        udpServerThread = new UdpServerThread(UdpServerPORT);
        udpServerThread.start();
        udpEventThread = new UdpEventThread(UdpServerPORT);
        udpEventThread.start();
        return Service.START_STICKY;
    }

    @Override
    public void onDestroy()
    {
        EnviarPaquete(Paquete.FIN_COMUNICACION);
    }
    private final static String TAG = MainActivity.class.getSimpleName();

    @Override
    public IBinder onBind(Intent intent) {
        //TODO for communication return IBinder implementation
        return null;
    }

    public static String IP_Aplicacion;
    public static final int Puerto = 4445;
    String UltimoPackageInformado;


    public class UdpServerThread extends Thread {
        DatagramSocket socket;
        boolean running;
        public boolean Terminated = false;

        public UdpServerThread(int serverPort) {
            super();
        }

        public void setRunning(boolean running) {
            this.running = running;
        }
        @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
        @Override
        public void run() {
            running = true;
            try {
                socket = new DatagramSocket(Puerto);
                Log.e(TAG, "UDP Server is running");

                while (running) {
                    byte[] buf = new byte[256];
                    DatagramPacket packet = new DatagramPacket(buf, buf.length);
                    socket.receive(packet);
                    Paquete paquete = new Paquete(packet);
                    switch (paquete.ObtenerTipo()) {
                        case Paquete.INICIO_COMUNICACION: {
                            IP_Aplicacion = paquete.ObtenerIP();
                            EnviarPaquete(Paquete.CONFIRMAR_INICIO_COMUNICACION);
                            //inicializacion de las variables en la aplicacion.
                            EnviarPaquete(Paquete.INICIO_COMUNICACION, ObtenerPackageEnEjecucion());
                            UltimoPackageInformado = ObtenerPackageEnEjecucion();
                            break;
                        }
                        case Paquete.FIN_COMUNICACION: {
                            EnviarPaquete(Paquete.CONFIRMAR_FIN_COMUNICACION);
                            running = false;
                            break;
                        }
                        case Paquete.SOLICITAR_PACKAGE_EN_EJECUCION: {
                            EnviarPaquete(Paquete.RTA_SOLICITUD_PACKAGE_EN_EJECUCION, ObtenerPackageEnEjecucion());
                            break;
                        }
                        case Paquete.SOLICITAR_APP_INSTALADA:
                        {
                            if(IsAppInstalled(paquete.ObtenerInfo().get(0)))
                            {
                                EnviarPaquete(Paquete.RTA_SOLICITUD_CONFIRMADA);
                            } else  EnviarPaquete(Paquete.RTA_SOLICITUD_RECHAZADA);
                            break;
                        }
                        default: {
                            EnviarPaquete(Paquete.PAQUETE_DESCONOCIDO);
                            break;
                        }
                    }
                    Thread.sleep(10);
                }

                Log.e(TAG, "UDP Server ended");

            } catch (SocketException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            } catch (InterruptedException e) {
                e.printStackTrace();
            } finally {
                if (socket != null) {
                    socket.close();
                    Log.e(TAG, "socket.close()");
                }
            }
        }
    }

    public class UdpEventThread extends Thread {
        DatagramSocket socket;
        boolean running;
        public UdpEventThread(int serverPort) {
            super();
        }

        public void setRunning(boolean running) {
            this.running = running;
        }

        @Override
        public void run() {
            running = true;
            try {
                Log.e(TAG, "UDP Server is running");

                while (running) {
                    if (IP_Aplicacion != null)
                    {
                        if(UltimoPackageInformado != null)
                        {
                            String paqueteenejecucion = ObtenerPackageEnEjecucion();
                            if (UltimoPackageInformado.compareTo(paqueteenejecucion) != 0) {
                                EnviarPaquete(Paquete.EVENTO_CAMBIO_PACKAGE_EN_EJECUCION, UltimoPackageInformado + ">" + paqueteenejecucion);
                                UltimoPackageInformado = paqueteenejecucion;
                            }
                        }
                    }
                    Thread.sleep(10);
                }

                Log.e(TAG, "UDP EventThread ended");
            }
            catch (InterruptedException e) {
                e.printStackTrace();
            } finally {
                if (socket != null) {
                    socket.close();
                    Log.e(TAG, "socket.close()");
                }
            }
        }
    }
    private void EnviarPaquete(final String tipo, final String data) {
        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    DatagramSocket socket = new DatagramSocket();
                    String message = IP_Aplicacion + ">" + tipo + ">" + data;
                    InetAddress address = InetAddress.getByName(IP_Aplicacion);
                    DatagramPacket packet = new DatagramPacket(message.getBytes(), message.getBytes().length, address, Puerto);
                    socket.send(packet);
                    socket.close();
                } catch (Exception e) {
                    System.out.println(e.getMessage());
                }
                return null;
            }
        }.execute();
    }

    public void EnviarPaquete(final String tipo) {
        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    DatagramSocket socket = new DatagramSocket();
                    String message = IP_Aplicacion + ">" + tipo;
                    InetAddress address = InetAddress.getByName(IP_Aplicacion);
                    DatagramPacket packet = new DatagramPacket(message.getBytes(), message.getBytes().length, address, Puerto);
                    socket.send(packet);
                    socket.close();
                } catch (Exception e) {
                    System.out.println(e.getMessage());
                }
                return null;
            }
        }.execute();
    }

    private String ObtenerPackageEnEjecucion() {
        if (Build.VERSION.SDK_INT >= 21) {
            String currentApp = null;
            UsageStatsManager usm = (UsageStatsManager) this.getSystemService(Context.USAGE_STATS_SERVICE);
            long time = System.currentTimeMillis();
            List<UsageStats> applist = usm.queryUsageStats(UsageStatsManager.INTERVAL_DAILY, time - 1000 * 1000, time);
            if (applist != null && applist.size() > 0) {
                SortedMap<Long, UsageStats> mySortedMap = new TreeMap<>();
                for (UsageStats usageStats : applist) {
                    mySortedMap.put(usageStats.getLastTimeUsed(), usageStats);
                }
                if (mySortedMap != null && !mySortedMap.isEmpty()) {
                    currentApp = mySortedMap.get(mySortedMap.lastKey()).getPackageName();

                }
            }
            return currentApp;
        } else {

            ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
            String mm = (manager.getRunningTasks(1).get(0)).topActivity.getPackageName();
            return mm;
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private List<String> GetInstalledApps()
    {
        UsageStatsManager usm = (UsageStatsManager) this.getSystemService(Context.USAGE_STATS_SERVICE);
        long time = System.currentTimeMillis();
        List<UsageStats> applist = usm.queryUsageStats(UsageStatsManager.INTERVAL_YEARLY, time - 1000 * 1000, time);
        List<String> apps = new Vector<String>();
        for(UsageStats app : applist)
        {
            apps.add(app.getPackageName());
        }
        return apps;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private boolean IsAppInstalled(String app_name)
    {
        UsageStatsManager usm = (UsageStatsManager) this.getSystemService(Context.USAGE_STATS_SERVICE);
        long time = System.currentTimeMillis();
        List<UsageStats> applist = usm.queryUsageStats(UsageStatsManager.INTERVAL_YEARLY, time - 1000 * 1000, time);
        for(UsageStats app : applist)
        {
            if(app.getPackageName().contains(app_name))
            {
                return true;
            }
        }
        return false;
    }
}




    class Paquete {

        public static final String INICIO_COMUNICACION = "INICIO_COMUNICACION";
        public static final String CONFIRMAR_INICIO_COMUNICACION = "CONFIRMAR_INICIO_COMUNICACION";
        public static final String CONFIRMAR_FIN_COMUNICACION = "CONFIRMAR_FIN_COMUNICACION";
        public static final String FIN_COMUNICACION = "FIN_COMUNICACION";
        public static final String PAQUETE_DESCONOCIDO = "PAQUETE_DESCONOCIDO";
        public static final String SOLICITAR_PACKAGE_EN_EJECUCION = "SOLICITAR_PACKAGE_EN_EJECUCION";
        public static final String RTA_SOLICITUD_PACKAGE_EN_EJECUCION = "RTA_SOLICITUD_PACKAGE_EN_EJECUCION";
        public static final String EVENTO_CAMBIO_PACKAGE_EN_EJECUCION = "EVENTO_CAMBIO_PACKAGE_EN_EJECUCION";
        public static final String SOLICITAR_APP_INSTALADA = "SOLICITAR_APP_INSTALADA";
        public static final String RTA_SOLICITUD_CONFIRMADA = "RTA_SOLICITUD_CONFIRMADA";
        public static final String RTA_SOLICITUD_RECHAZADA = "RTA_SOLICITUD_RECHAZADA";

        private String IP;
        private String Tipo;
        private List<String> Info;

        public String ObtenerIP() {
            return IP;
        }

        public String ObtenerTipo() {
            return Tipo;
        }

        public List<String> ObtenerInfo() {
            return Info;
        }

        public Paquete(String msg) {
            String[] data = msg.split(">");
            this.IP = data[0];
            this.Tipo = data[1];
            if (data.length > 1) {
                Info = new Vector<String>();
                for (int i = 2; i < data.length; i++) {
                    Info.add(data[i]);
                }
            }
        }

        public Paquete(DatagramPacket packet) {
            String msg = new String(packet.getData(), packet.getOffset(), packet.getLength());
            String[] data = msg.split(">");
            this.IP = data[0];
            this.Tipo = data[1];
            if (data.length > 1) {
                Info = new Vector<String>();
                for (int i = 2; i < data.length; i++) {
                    Info.add(data[i]);
                }
            }
        }
    }