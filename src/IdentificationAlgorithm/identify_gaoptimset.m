function identify_gaoptimset(generations,populations,tolfun)
global dirname;
dirname = 'c:\inetpub\wwwroot\identifikace\logs\';
global filename;
logname = strcat('matlab-',datestr(now,'yy-mm-dd'));
filename = strcat(dirname,logname);
global debuglog;
debuglog = not(isempty(getenv('IDENTIFIKACE_MATLAB_DEBUG')));
global gaoptions;
gaoptions = gaoptimset(...
    'Vectorized','on',...
    'TolFun',tolfun,...
    'Generations',generations,...
    'PopulationSize',populations);
identify_log('gaoptimse tolfun generations populations',{tolfun generations populations});
