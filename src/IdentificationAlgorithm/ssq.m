function L2E = ssq(model_data)
    global model_struct;
    exact_data = model_struct.experiment_data;
    experiment_times = model_struct.experiment_times;
    [n_individuals, n_curves] = size(model_data);

%    n_curves = n_curves - 1; % exclude time

    L2E = zeros(n_individuals,1);
    
    for i_individual = 1:n_individuals %loop over all evaluated model data
        model_times = model_data{i_individual}(:,1);
        if (size(experiment_times) ~= size(model_times))
            error('size of exact data and model data does not match');
        end
        if norm(experiment_times - model_times, inf) > 1e-12
            error('experiment and model times are not the same');
        end
%        for i_sample = 1:n_samles
    %evaluate the error:
        L2E(i_individual) = ...
            norm(exact_data - model_data{i_individual}(:,2:end),'fro');
            
%            d = abs((y_e(min_ind:max_ind) - y_m_interp).*dx_e_trimed);
    end
    if not(isa(L2E,'float'))
        L2E
        error('L2E is not a number')
    end
end
      
