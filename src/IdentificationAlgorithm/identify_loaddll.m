function identify_loaddll(model_name,update_url)
%currentfolderdll = strcat(pwd,'\SimulatorBalancerLibrary.dll')
%identify_logs('current directory:',pwd);
identify_logs('update_url',update_url);
currentfolderdll ='c:\inetpub\wwwroot\identifikace\bin\SimulatorBalancerLibrary.dll';
%currentfolderdll = 'SimulatorBalancerLibrary.dll';
%disp(currentfolderdll);
NET.addAssembly(currentfolderdll);
global mySimulator;
mySimulator = SimulatorBalancerLibrary.GenericSimulator(model_name,update_url);
%SimulatorBalancerLibrary.GenericSimulator.Description()

%    desc = char(mySimulator.Description());
%    identify_logs('description',desc);
