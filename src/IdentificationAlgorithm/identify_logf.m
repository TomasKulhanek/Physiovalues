function identify_logf(title,message)
global debuglog;
if not(debuglog)
    return;
end

global filename;
%filename = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\RestMasterService\matlab.log';
fid=fopen(filename,'a');
  fprintf(fid,'--------- %s %s\n',datestr(now),title);
  fprintf(fid,'%f ',message);
  fprintf(fid,'\n');
fclose(fid);