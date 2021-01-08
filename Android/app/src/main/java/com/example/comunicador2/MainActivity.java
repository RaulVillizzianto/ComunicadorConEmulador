package com.example.comunicador2;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.AppOpsManager;
import android.app.Service;
import android.app.usage.UsageStats;
import android.app.usage.UsageStatsManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.location.Address;
import android.net.Uri;
import android.nfc.Tag;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;

import android.os.IBinder;
import android.os.Parcelable;
import android.provider.Settings;
import android.text.PrecomputedText;
import android.util.Log;
import android.widget.TextView;

import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.util.Date;
import java.util.Enumeration;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.util.List;
import java.util.SortedMap;
import java.util.TreeMap;
import java.util.Vector;
import java.util.concurrent.ConcurrentLinkedQueue;


public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        infoIp = (TextView) findViewById(R.id.infoip);
        infoPort = (TextView) findViewById(R.id.infoport);
        infoIp.setText(getIpAddress());
        infoPort.setText(String.valueOf(UdpServerPORT));
    }
     static final int UdpServerPORT = 4445;

    TextView infoIp, infoPort;

    private boolean isMyServiceRunning(Class<?> serviceClass) {
        ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
        for (ActivityManager.RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
            if (serviceClass.getName().equals(service.service.getClassName())) {
                return true;
            }
        }
        return false;
    }

    @Override
    protected void onDestroy()
    {

        super.onDestroy();
    }


    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    @Override
    protected void onStart() {

        if(UserHasActionUsageAccess() == false)
        {
             requestReadNetworkHistoryAccess();
        }
        Intent i= new Intent(getApplicationContext(), Comunicador.class);
        if(!isMyServiceRunning(Comunicador.class)) {
            getApplicationContext().startService(i);
        }
        super.onStart();
    }
        @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private String GetPackageForApp(String app_name)
    {
        UsageStatsManager usm = (UsageStatsManager) this.getSystemService(Context.USAGE_STATS_SERVICE);
        long time = System.currentTimeMillis();
        List<UsageStats> applist = usm.queryUsageStats(UsageStatsManager.INTERVAL_YEARLY, time - 1000 * 1000, time);

        for(UsageStats app : applist)
        {

            if(app.getPackageName().contains(app_name))
            {
                return app.getPackageName();
            }
        }
        return null;
    }

    private void requestReadNetworkHistoryAccess() {
    Intent intent = new Intent(Settings.ACTION_USAGE_ACCESS_SETTINGS);
    startActivity(intent);
    }
    @RequiresApi(api = Build.VERSION_CODES.KITKAT)
    private boolean UserHasActionUsageAccess()
    {
        try {
           PackageManager packageManager = getApplicationContext().getPackageManager();
           ApplicationInfo applicationInfo = packageManager.getApplicationInfo(getApplicationContext().getPackageName(), 0);
           AppOpsManager appOpsManager = (AppOpsManager) getApplicationContext().getSystemService(Context.APP_OPS_SERVICE);
           int mode = appOpsManager.checkOpNoThrow(AppOpsManager.OPSTR_GET_USAGE_STATS, applicationInfo.uid, applicationInfo.packageName);
           return (mode == AppOpsManager.MODE_ALLOWED);

        } catch (PackageManager.NameNotFoundException e) {
           return false;
        }

    }

    private String getIpAddress() {
        String ip = "";
        try {
            Enumeration<NetworkInterface> enumNetworkInterfaces = NetworkInterface
                    .getNetworkInterfaces();
            while (enumNetworkInterfaces.hasMoreElements()) {
                NetworkInterface networkInterface = enumNetworkInterfaces
                        .nextElement();
                Enumeration<InetAddress> enumInetAddress = networkInterface
                        .getInetAddresses();
                while (enumInetAddress.hasMoreElements()) {
                    InetAddress inetAddress = enumInetAddress.nextElement();

                    if (inetAddress.isSiteLocalAddress()) {
                        ip += "SiteLocalAddress: "
                                + inetAddress.getHostAddress() + "\n";
                    }

                }

            }

        } catch (SocketException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            ip += "Something Wrong! " + e.toString() + "\n";
        }

        return ip;
    }
}

/*
reference:
https://docs.oracle.com/javase/tutorial/networking/datagrams/clientServer.html
*/
