function identify_logf(title,message)
global filename;
%filename = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\RestMasterService\matlab.log';
fid=fopen(filename,'a');
  fprintf(fid,'--------- %s \n',title);
  fprintf(fid,'%f ',message);
  fprintf(fid,'\n');
fclose(fid);