# ComunicadorConEmulador
 Clase en C# que comunica nuestra PC con el emulador de android usando un APK a través del puerto 4445
Para habilitar la comunicacion con el emulador se requiere hacer una redireccion de los paquetes UDP esto ocurre porque el emulador establece que la IP de la computadora es 10.0.2.2 para la comunicacion y se establece como 10.0.2.15 (127.0.0.1) para el mismo. De todas formas la PC no especifica la IP para el emulador de android, así que la unica manera en la que la PC se puede comunicar con el emulador es a través de la redirección de puertos.
Instalar el cliente de telnet, si todavía no está instalado
1- Para hacer esto en el menu de inicio escribir cmd y abrir la consola de comandos.
2- ejecutar telnet localhost (puerto del emulador normalmente 5554/5556)
3- luego una vez que ingrese a la consola de telnet buscar el token de autenticacion, en mi caso ne la misma consola me indica la ruta dónde esta ('C:\Users\--\.emulator_console_auth_token')
asi que vamos a la carpeta indicada abrimos el archivo como un archivo de texto y copiamos el token.
3. Luego ejecutamos la redirección el formato es el siguiente:
redir add  <  udp/tcp  > : <  pc puerto  > : <  emulador puerto  >
Por ejemplo: redir add udp: 2888: 2888 
     
En este caso ejecutamos: redir add udp: 4445: 4445 
Despues de ejecutar este comando toda la informacion recibida en el puerto 4445 de la pc sera transferida al puerto 4445 del emulador.
