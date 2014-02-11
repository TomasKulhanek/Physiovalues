function [p_fitted val] = parid_main(model_name)
    vectorized = 'off';
    switch model_name
        case 'mySinc'
            vectorized = 'on';
            M = load ('data/mySinc.txt');
            experiment_times = M(:,1);
            experiment_data = M(:,2);
            %p1 .. frekvence
            %p2 .. f�ze
            %p3 .. amplituda
            %p4 .. sklon

            pp = [ 6.1237    0.0014    2.9330    0.2089];
            p_bounds = [0 -2 0 0; 10 2 6 10];
            p_val = [6 2 6 0.3];
            p_is_fixed = [false false false false];
            alg_name = 'ga';
            alg_opt = '';
            verbose = false;
            %-----------------------------------------------------------

        otherwise
            error('this model doesnt exist')
    end
    %draw first curve:
    curve_fig = figure();
    hold on;
    plot(experiment_times,experiment_data,'color','red');
    tic;
    [p_fitted, val] = minimize(model_name, experiment_data, experiment_times, p_bounds, ...
        p_val, p_is_fixed, alg_name, alg_opt, verbose, vectorized);
    toc;
    switch model_name
        case 'mySinc'
            fitted_data = mySinc(p_fitted,experiment_times);
        otherwise
            error('this model doesnt exist')
    end
    p_fitted
    figure(curve_fig);
    plot(fitted_data{1}(:,1),fitted_data{1}(:,2),'color','green');
    legend('experimental','fitted');
    hold off;