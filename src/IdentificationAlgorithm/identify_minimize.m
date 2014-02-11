function [p_fitted, val] = identify_minimize(experiment, p_names, p_min,p_max, p_val, p_is_fixed,variable_names)
    %in:
    %model_name
    %experiment_data .. cell array
    %            each cell contains two-row matrix. First row contains 
    %            independent variable valus, second dependent variable values.
    %p_bounds .. matrix of bounds of parameters
    %          first row - minimum, second row maximum
    %          i-th column - i-th parameter
    %v_val .. vector of parameters - exact value of known parameters and initialg uess of unknown
    %p_is_fixed .. logical vector
    %alg_name
    %alg_opt
    %verbose .. verbose output (true/false)
    %gr .. grid to evaluate model on

    [temp_var, n_par] = size(p_val);
    n_fixed = sum(p_is_fixed);
    p_fixed_val = zeros(1,n_fixed);
    n_nonfixed = n_par - n_fixed;
    p_nonfixed_val = zeros(1,n_nonfixed);
    i_fixed = 0;
    i_nonfixed = 0;
    for i = 1:n_par
        if p_is_fixed(i)
            i_fixed = i_fixed + 1;
            p_fixed_val(i_fixed) = p_val(i);
        else
            i_nonfixed = i_nonfixed + 1;
            p_nonfixed_val(i_nonfixed) = p_val(i);
        end
    end
%     {experiment(:,2)}
%     experiment(:,1)
%     p_names
%     variable_names
%     p_is_fixed
%     p_fixed_val
    global model_struct
    [em,en] = size(experiment);
    model_struct = struct('experiment_data',{experiment(:,2:en)},'experiment_times',experiment(:,1),...
    'p_names',{p_names},'variable_names',{variable_names},...
    'p_is_fixed',p_is_fixed,'p_fixed_val',p_fixed_val);


    %objective([1 0])

    %    case 'ga'
            %p_bounds(1,:);
            %p_bounds(2,:);
            %'PopInitRange',[p_min; p_max],'Generations',100,'PopulationSize',popSize,
            global gaoptions;
            if not(isstruct(gaoptions)) 
                identify_logs('setting default gaoptions','1000 generations 20 population 1e-6 TolFun');
                identify_gaoptimset(1000,20,1e-6);
            %gaoptions = gaoptimset('Vectorized','on','TolFun',1e-6,'Generations',100000,'PopulationSize',200);%,'UseParallel','always');%,'TolFun',1e-20,'Generations',10000);
            end
            %error()
            %start_time = tic;
            %elitecount = idivide(n_nonfixed, 4);
            %newoptions = gaoptimset('EliteCount',elitecount);
            %myoptions = gaoptimset(gaoptions,newoptions);
            
            p_nonfixed_fitted = ga(@identify_objective,n_nonfixed,[],[],[],[], p_min, p_max,[],gaoptions);
            %time_ga_serial = toc(start_time);
            %------------------------------------------------------------------
    val = identify_objective(p_nonfixed_fitted);


    %combine found nonfixed and fixed parameters
    i_nonfixed = 0;
    i_fixed = 0;
    
    for i = 1:n_par
        if p_is_fixed(i)
            i_fixed = i_fixed + 1;
            p_fitted(i) = p_fixed_val(i_fixed);
        else
            i_nonfixed = i_nonfixed + 1;
            p_fitted(i) = p_nonfixed_fitted(i_nonfixed);
        end
    end
    %'number of function evaluation'
    % model_struct.evalN
end


