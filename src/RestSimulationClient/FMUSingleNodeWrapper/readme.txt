FMUSingleNodeWrapper [params]

Starts web server with REST API for controlling simulation of model within FMU.
Requires .NET Framework 4.0 and privileges to start HTTP server to listen on specific port 
on the machine (Windows platform: netsh http add urlacl url=http://hostname:port/  user=\Everyone listen=yes

by default the current directory is searched for all the files with extension .fmu
/t=[temp_directory] optional, temporary directory with read/write permission to unpack FMU (default tempfmu)
/p=[port_number] optional, sets the embeded HTTP server port number (default 48048)
/u=[host_name_or_IP] optional, sets hostname or IP address to listen, by default it is detected by system (default localhost)
/w=[register_worker_url] optional, sets URL of REST Worker service to register this worker (default physiome.lf1.cuni.cz/identifikace)
/h		will show this help
