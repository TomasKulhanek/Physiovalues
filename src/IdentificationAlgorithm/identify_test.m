function p_fitted = identify_test()
dirname = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\';
experiment = dlmread(strcat(dirname,'experiment.mat'));
p_names = {'im[1].k_imai[1]';'im[1].k_imai[2]';'im[1].k_imai[3]';'im[1].k_imai[4]'} %dlmread(strcat(dirname,'p_names.mat')); incredible strings written by matlab cannot be read !!!
p_val = dlmread(strcat(dirname,'p_val.mat'));
p_min = dlmread(strcat(dirname,'p_min.mat'));
p_max = dlmread(strcat(dirname,'p_max.mat'));
p_is_fixed = dlmread(strcat(dirname,'p_is_fixed.mat'));
variable_names = {'time';'im[1].sO2'}; %dlmread(strcat(dirname,'variable_names.mat'));
model_name = 'MatejakAB2013_Kulhanek2013.fmu';%dlmread(strcat(dirname,'model_name.mat'));
update_url= 'http://localhost/identifikace';%dlmread(strcat(dirname,'update_url.mat'));
p_fitted = identify_main(experiment,p_names,p_val,p_min,p_max,p_is_fixed,variable_names,model_name,update_url);
ssq = identify_getssq();
ssq
cycles = identify_getcomputationcycles();
cycles
simtime = identify_getsimulationtime();
simtime


%             %vectorized = 'on';
%             M = load ('data/mySinc.txt');
%             experiment_times = M(:,1);
%             experiment_data = M(:,2);
%             %p1 .. frekvence
%             %p2 .. f�ze
%             %p3 .. amplituda
%             %p4 .. sklon
% 
%             %p_val = [ 6.1237    0.0014    2.9330    0.2089];
%             p_names = {'a'; 'b'; 'c'; 'd'};
%             variable_names = {'x'; 'y'};
%             
%             p_bounds = [0 -2 0 0; 10 2 6 10];
%             p_val = [6 2 6 0.3];
%             p_is_fixed = [false false false false];
%             %alg_name = 'ga';
%             %alg_opt = '';
%             %verbose = false;
%     %draw first curve:
%     curve_fig = figure();
%     hold on;
%     plot(experiment_times,experiment_data,'color','red');
%     tic;
%     [p_fitted, val, fitted_data] = identify_main(M, p_names, p_val, p_bounds(1,:), p_bounds(2,:),p_is_fixed,variable_names);
%     toc;
%     %fitted_data = identify_simulate(p_names,p_fitted,variable_names,experiment_times);
%     figure(curve_fig);
%     m2 = fitted_data.double;
%     plot(m2(:,:,1),m2(:,:,2),'color','green');
%     legend('experimental','fitted');
%     hold off;
    

    