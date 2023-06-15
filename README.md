Documentation “Deploy Framework ASP.Net 6 to Linux Server”

## Install dotnet runtime dan sdk 6

Command : 

```wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb```
```sudo dpkg -i packages-microsoft-prod.deb```
```rm packages-microsoft-prod.deb```

```sudo apt-get update && \```
  ```sudo apt-get install -y dotnet-sdk-6.0```

```sudo apt-get update && \```
  ```sudo apt-get install -y aspnetcore-runtime-6.0```

```sudo apt-get install -y dotnet-runtime-6.0```

## Install Nginx to reverse proxy

Modification that path : ```nano /etc/nginx/sites-available/default```

server {

listen 80;

location / {

proxy_pass http://localhost:5000;

proxy_http_version 1.1;

proxy_set_header Upgrade $http_upgrade;

proxy_set_header Connection keep-alive;

proxy_set_header Host $host;

proxy_cache_bypass $http_upgrade;

proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

proxy_set_header X-Forwarded-Proto $scheme;

}
}

Proxy pass is filled with port 5000 because that port is the default for publishing dotnet 6.

## Publish framework .net

Command : ```dotnet publish -o cualivy```

Note "cualivy" is the name of the file after publishing.

## Convert a .NET application into a service on a Linux server to ensure continuous operation of the application.

Make file in that directory

Command : ```sudo nano /etc/systemd/system/nama_file.service```

Please fill in the following file with the configuration below.

[Unit]

Description=My first .NET Core application on Ubuntu

[Service]

WorkingDirectory=/home/ec2-user/CoreRESTServer

ExecStart=/usr/bin/dotnet /home/ec2-user/CoreRESTServer/CoreRESTServer.dll

Restart=always

RestartSec=10 # Restart service after 10 seconds if dotnet service crashes

SyslogIdentifier=offershare-web-app

Environment=ASPNETCORE_ENVIRONMENT=Production


[Install]

WantedBy=multi-user.target

The path highlighted in yellow is the resulting path from the .net framework publish.

## Enable service
		
Command : 

```sudo systemctl enable nama_file.service```

```sudo systemctl start nama_file.service```

```sudo systemctl status nama_file.service```

## Update API in ASP.NET 6

```sudo systemctl stop nama_file.service```

```Git pull origin master```

```sudo systemctl start nama_file.service```

## Add Dependencies For Machine Learning Model

Go to that path : ```nano /Cuacllivy-CC/bin/Debug/net6/x64```

Then use command :

```ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 liblept.so.5```

```ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 libleptonica-1.78.0.so```

```ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 libleptonica-1.80.0.so```

```ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.4.0.1 libtesseract41.so```

Note : used for dependencies for machine learning model


