function [p_fitted, val] = minimize(model_name,experiment_data,experiment_times, ...
    p_bounds, p_val, p_is_fixed, alg_name, alg_opt, verbose, vectorized)
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

    %tests of input validity
    if (not(isa(model_name,'char')))
        error('model_name parameter must be character array.')
    end

    [~, n_par] = size(p_val);
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
    global model_struct
    model_struct = struct('model_name',model_name,'experiment_data',...
        {experiment_data},'experiment_times',experiment_times,'p_is_fixed',...
        p_is_fixed,'p_fixed_val',p_fixed_val,'evalN',0);


    %objective([1 0])

    switch alg_name
        case 'monteGradMinimize'
            p_nonfixed_fitted = monteGradMinimize(@objective,p_bounds);

            %  case 'globalSearch'
            %    gs = GlobalSearch
            %    problem = createOptimProblem('solverName', 'fmincon','lb',p_bounds(1,:),...
            %		'ub',p_bounds(2,:),'objective',@objective,'x0',p_nonfixed_val)
            %    [p,fbal] = run(gs,problem)
            %---------------------------------------------------------------
        case 'ga'
            %p_bounds(1,:);
            %p_bounds(2,:);
            %'PopInitRange',[p_min; p_max],'Generations',100,'PopulationSize',popSize,
            gaoptions = gaoptimset('Vectorized',vectorized,'TolFun',1e-20,'Generations',10000);%,'UseParallel','always');%,'TolFun',1e-20,'Generations',10000);
            %error()
            start_time = tic;
            p_nonfixed_fitted = ga(@objective,n_nonfixed,[],[],[],[],p_bounds(1,:), ...
               p_bounds(2,:),[],gaoptions);
            time_ga_serial = toc(start_time);
            fprintf('Serial GA optimization vectorized takes %g seconds.\n',time_ga_serial);           
            %------------------------------------------------------------------

        case 'anni'
            eq_n = product(size(experiment_data));
            p_nonfixed_fitted = call_anni(p_nonfixed_n,p_bounds,eq_n);

        otherwise 
            error('wrong algorithm')
    end
    val = objective(p_nonfixed_fitted);


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
    model_struct.evalN;
end


