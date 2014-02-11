function L2E = identify_ssq(net_model_data)
    global model_struct;
    exact_data = model_struct.experiment_data;
    experiment_times = model_struct.experiment_times;
    % needs reshape data from .net
    try
        model_data = permute(net_model_data.double,[2 3 1]);
        %model_data =net_model_data.double;
    [n_data,n_var, n_individuals] = size(model_data);
%    n_curves = n_curves - 1; % exclude time

    L2E = zeros(n_individuals,1);
    
    for i_individual = 1:n_individuals %loop over all evaluated model data
        model_times = model_data(:,1,i_individual);
        if (size(experiment_times) ~= size(model_times))
            error('size of exact data and model data does not match');
        end
        %identify_logd('experiment-times',experiment_times);
        %identify_logd('model_times',model_times);
        if norm(experiment_times - model_times, inf) > 1e-12
            error('experiment and model times are not the same');
        end
%        for i_sample = 1:n_samles
    %evaluate the error:
    % bug - exact data with more variables has different dimension
        L2E(i_individual) = ...
            norm(exact_data - model_data(:,2:end,i_individual),'fro');
            
%            d = abs((y_e(min_ind:max_ind) - y_m_interp).*dx_e_trimed);
    end
    catch err
        %identify_logs('exception in ssq',err);%,net_model_data);
        %rethrow(err);
        L2E= 1e+10;
    end
    if not(isa(L2E,'float'))
        L2E
        %identify_logs('L2E is not a number',L2E);
        error('L2E is not a number')
    end
    
end
      
