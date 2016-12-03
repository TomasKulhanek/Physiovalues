function identify_logd(title,messaged)
%filename = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\matlab.log';
global filename;
fid=fopen(filename,'a');
[rows,collumns] = size(messaged);
  fprintf(fid,'--------- %s \n',title);
  fprintf(fid,'type of struct:%s %d:%d\n',class(messaged),rows,collumns);
  fprintf(fid,'type of item: %s \n',class(messaged(1,1)));
for i=1:rows
    for j=1:collumns
  fprintf(fid,'%d ',messaged(i,j));
    end
    fprintf(fid,'\n');
end

fclose(fid);