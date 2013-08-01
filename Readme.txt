ABOUT

I use the symon system by Willem Dijkstra (http://wpd.home.xs4all.nl/symon/) to monitor system performance on FreeBSD and Linux servers, and we also use Windows Servers in our organization, currently symon system doesn't provide client for windows OS, so that motivated me to write symon client for windows.

DESCRIPTION

Symon client works as windows service. It uses windows Performance Data Helper (PDH) counters. It is written in C#. It requires .NET Framework 3.5.

The command line options:

symon.exe [/i] [/u] [/list] [/test]

/i	Install symon as windows service.
/u	Uninstall symon service.
/list	Get a list of network interfaces on your system.
/test	Run symon without installation.

CONFIGURATION
     symon client obtains configuration data from symon.exe.config file based on the location of the executing assembly.

EXAMPLE

<userSettings>
    <RemoteHost host="192.168.0.1" port="2100" />
    <Counters>
      <add key="cpu(0)" value="Symon.CPUInfo"/>
      <add key="mem" value="Symon.MEMInfo"/>
      <add key="io(C:)" value="Symon.IOInfo"/>
      <add key="io(E:)" value="Symon.IOInfo"/>
      <add key="io(F:)" value="Symon.IOInfo"/>
      <add key="if(1)" value="Symon.IFInfo"/>
    </Counters>
  </userSettings>
	
cpu			Processor(_Total) counter.	

mem			Memory and Process counters.

io(<diskname>)		LogicalDisk counter ( for server-side config (symux): C: -> ad0, D: -> ad1, E: -> ad2, F: -> ad3 ).

if(<iface index>)	Network Interface counter (for server-side config (symux): 0 -> eth0, 1 -> eth1, ...) To get <iface index> on your system use symonclient.exe /list command.

BUILD

%SystemRoot%\Microsoft.NET\Framework\v3.5\MSBuild.exe Symon.csproj /t:Rebuild /p:Configuration=Release /p:Platform=anycpu

INSTALL

symon.exe /i

