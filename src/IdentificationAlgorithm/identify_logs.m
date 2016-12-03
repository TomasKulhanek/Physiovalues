function identify_logs(title,message)
filename = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\matlab.log';
%global filename;
filename;
fid=fopen(filename,'a');
  fprintf(fid,'--------- %s \n',title);
  fprintf(fid,'%s\n',message);
fclose(fid);