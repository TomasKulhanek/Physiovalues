function [xymat] = identify_simulate(p_names,p,variable_names,grid)
    %p
    %grid
    %pause(0.005);
%    [n_individuals,~] = size(p);    matlab 2008 fix
%     [n_individuals,tempfix2] = size(p);    
%     xymat = cell(n_individuals,1);
%     for i = 1:n_individuals
%         a = p(i,1);
%         b = p(i,2);
%         c = p(i,3);
%         d = p(i,4);
%         y = c*sinc(a*(grid-b)) + d*grid;
%         xymat{i} = [grid y];
%     end
% 
% p_names
% p
% variable_names
% grid 
%p_names,p,variable_names,grid
% identify_log('p_names',p_names);
% identify_logd('p',p);
% identify_log('variable_names',variable_names);
% identify_logd('grid',grid);
global mySimulator;
%identify_logs('p_names',p_names(1));
xymat = mySimulator.Simulate(p_names,p,variable_names,grid);


end