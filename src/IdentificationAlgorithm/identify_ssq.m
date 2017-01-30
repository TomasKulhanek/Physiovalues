function L2E = identify_ssq(net_model_data)
    global model_struct;
    exact_data = model_struct.experiment_data;
    experiment_times = model_struct.experiment_times;
    % needs reshape data from .net
%    identify_logs('identify_ssq','trace1');
    
    try
        model_data = permute(net_model_data.double,[2 3 1]);
        %model_data =net_model_data.double;
    [n_data,n_var, n_individuals] = size(model_data);
%    n_curves = n_curves - 1; % exclude time

%    identify_logs('identify_ssq','trace2');
    L2E = zeros(n_individuals,1);
    
    for i_individual = 1:n_individuals %loop over all evaluated model data
%        identify_logs('identify_ssq','trace29');
        model_times = model_data(:,1,i_individual);
%        identify_logf('identify_ssq model_times',model_times);
        if (size(experiment_times) ~= size(model_times))
            identify_logs('identify_ssq','size of exact data and model data does not match');
            error('size of exact data and model data does not match');
        end
%        identify_logs('identify_ssq','trace3');
        %identify_logd('experiment-times',experiment_times);
        %identify_logd('model_times',model_times);
        if norm(experiment_times - model_times, inf) > 1e-12
            %error('experiment and model times are not the same');
            identify_logs('identify_ssq','error in ssq.experiment and model times are not the same');
            identify_logf('identify_ssq trace exact_data',exact_data);
            identify_logf('identify_ssq trace model_times',model_times);
            identify_logf('identify_ssq trace model_data',model_data(:,2:end,i_individual));
            L2E= 1e+5;
        else
%        for i_sample = 1:n_samles
    %evaluate the error:
    % bug - exact data with more variables has different dimension
        %identify_logs('identify_ssq workaround 2','trace4');
        %identify_logf('identify_ssq trace exact_data',exact_data);
        %identify_logf('identify_ssq trace model_data',model_data(:,2:end,i_individual));
        
        % bug, MATLAB hangs here sometimes
        % L2E(i_individual) = norm(exact_data - model_data(:,2:end,i_individual),'fro');
        L2E(i_individual)= 0; %bug workaround 1 
        for j=1:size(exact_data)
            md = model_data(:,2:end,i_individual);
          L2E(i_individual) = L2E(i_individual)+ (exact_data(j)-md(j))^2;
        end
        % L2E(i_individual) = norm(exact_data - model_data(:,2:end,i_individual));
        %identify_logs('identify_ssq','trace41');
%            d = abs((y_e(min_ind:max_ind) - y_m_interp).*dx_e_trimed);
        end
        %identify_logs('identify_ssq','trace42');
    end
%    identify_logs('identify_ssq','trace5');    
    catch err
        identify_logs('exception in ssq',getReport(err));%,net_model_data);
        %rethrow(err);
        L2E= 1e+10;
    end
%    identify_logs('identify_ssq','trace6');    
    if not(isa(L2E,'float'))
        %L2E
        identify_logs('identify_ssq','L2E is not a number');
        error('L2E is not a number')
    end
    %identify_logs('identify_ssq','trace7');    
end
      
