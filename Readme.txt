ABOUT

I use the symon/symux packages by Willem Dijkstra (http://wpd.home.xs4all.nl/symon/) to monitor system performance on FreeBSD and Linux servers, and we also use Windows Servers in our organization, currently symon system doesn't provide client for windows, so that motivated me to write symon client for windows.

DESCRIPTION

Symon client works as windows service. It is written in C#.

The command line options:

symonclient.exe [/i] [/u] [/list] [/test]

/i	Install symonclient as windows service.
/u	Uninstall symonclient service.
/list	Get a list of network interfaces on your system.
/test	Run symonclient without installation.

CONFIGURATION
     symon client obtains configuration data from symonclient.exe.config file based on the location of the executing assembly.