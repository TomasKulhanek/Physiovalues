function identify_log(title,message)
%filename = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\matlab.log';
global debuglog;
if not(debuglog)
    return;
end
global filename;
fid=fopen(filename,'a');
[rows,cols] = size(message);
  fprintf(fid,'--------- %s \n',title);
  fprintf(fid,'type of struct: %s %d:%d\n',class(message),rows,cols);
  fprintf(fid,'type of item: %s \n',class(message{1}));
for i=1:rows
 for j=1:cols
  fprintf(fid,'%s ',message{i,j});
  fprintf(fid,'%e \n',message{i,j});
 end
 
end

fclose(fid);