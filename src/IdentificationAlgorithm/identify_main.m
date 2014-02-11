function p_fitted = identify_main(experiment,p_names,p_val,p_min,p_max,p_is_fixed,variable_names,model_name,update_url)
    
            % debugging
            %diary('c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\RestMasterService\matlab.log');           
            %vectorized = 'on';
            %M = load ('data/mySinc.txt'); %TODO load values
            %experiment_times = experiment(:,1); %=== experiment :,1
            %experiment_data = M(:,2);  === experiment :,2..n
            %p1 .. frekvence
            %p2 .. f�ze
            %p3 .. amplituda
            %p4 .. sklon

            %pp = [ 6.1237    0.0014    2.9330    0.2089];
            %p_bounds = [0 -2 0 0; 10 2 6 10];
            %p_val = [6 2 6 0.3];
            %p_is_fixed = [false false false false];
            %alg_name = 'ga';
            %alg_opt = '';
            %verbose = false;
            %p_bounds = [p_min;p_max];
            
            % register .net assembly
            identify_loaddll(model_name,update_url);
            %identify_initializesettings();
            %filename1 = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\RestMasterService\exp.log';
    [m,n] = size(p_names);
    if (m>1) 
        p_names = reshape(p_names,n,m);
    end
    [m,n] = size(variable_names);
    if (m>1) 
        variable_names = reshape(variable_names,n,m);
    end
%     dlmwrite(filename,'----------------------------------','-append');
 %dlmwrite(filename,experiment,'-append');
% dlmwrite(filename,p_names,'-append');
% dlmwrite(filename,p_val,'-append');
% dlmwrite(filename,p_min,'-append');
% dlmwrite(filename,p_max,'-append');
% dlmwrite(filename,p_is_fixed,'-append');
% dlmwrite(filename,variable_names,'-append');
%  ans1 = methods('SimulatorBalancerLibrary.GenericSimulator','-full');
%  identify_log('method signatures',ans1);
% debug write input arguments
dirname = 'c:\Users\tomaton\Documents\KOFRLAB-simenv\VersionedProjects\';
 dlmwrite(strcat(dirname,'experiment.mat'),experiment);
 dlmwrite(strcat(dirname,'p_names.mat'),p_names);
 dlmwrite(strcat(dirname,'p_val.mat'),p_val);
 dlmwrite(strcat(dirname,'p_min.mat'),p_min);
 dlmwrite(strcat(dirname,'p_max.mat'),p_max);
 dlmwrite(strcat(dirname,'p_is_fixed.mat'),p_is_fixed);
 dlmwrite(strcat(dirname,'variable_names.mat'),variable_names);
 dlmwrite(strcat(dirname,'model_name.mat'),model_name);
 dlmwrite(strcat(dirname,'update_url.mat'),update_url);
    [p_fitted, val] = identify_minimize( experiment, p_names,p_min,p_max, ...
        p_val, p_is_fixed,variable_names);
    global result_ssq
    result_ssq = val;
    %add to the end of array the value of ssq - 
            %p_fitted_size = size(p_fitted);
            %p_fitted( p_fitted_size+1) = val;
            
