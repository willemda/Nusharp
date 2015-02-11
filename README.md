# Nusharp

A (very) simple nuget c# nuget server, using [Nancy](https://github.com/NancyFx/Nancy) and running under [Mono](https://github.com/mono/mono).

#Installation

```bash
sudo adduser --disabled-login --gecos 'Nusharp' nusharp
cd /home/nusharp
sudo -u nusharp -H git clone https://github.com/willemda/Nusharp.git
sudo mkdir /opt/nusharp #nusharp will be installed here
sudo mkdir /var/nusharp/packages #packages store
sudo chown nusharp /opt/nusharp
sudo chown nusharp /var/nusharp/packages
sudo -u nusharp xbuild /property:OutputPath='/opt/nusharp'
sudo -u nusharp vim /opt/nusharp/Nusharp.SelfHost.exe.config #edit config, see below
```

Change the config to match your setup

```csharp
<add key="packageRepositoryPath" value="/var/nusharp/packages"/> /*package store*/
<add key="port" value="80"/> /*local port to bind*/
<add key="uri" value="http://mynugetserver.mydomain.com"/> /*external uri*/
<add key="username" value="Nusharp"/> /*basic auth, leave empty to disable basic auth*/
<add key="password" value="D64767AA-CCC2-43C9-BA99-F39247963628"/> /*basic auth passw*/
```

```bash
sudo apt-get install supervisor
sudo vim /etc/supervisor/conf.d/nusharp
```

Make sure to edit the config file to match your setup:

```bash
[program:nusharp]
command=/opt/mono/bin/mono Nusharp.SelfHost.exe -d
user=nusharp
stderr_logfile = /var/log/supervisor/nusharp-err.log
stdout_logfile = /var/log/supervisor/nusharp-stdout.log
directory=/opt/nusharp
```


