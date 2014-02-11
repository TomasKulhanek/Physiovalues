function [net_p_fitted, val,net_fitted_data] = identify_net(experiment,p_names,p_val,p_min,p_max,p_is_fixed,variable_names)

[p_fitted, val, fitted_data] = identify_main(experiment, p_names, p_val, p_min,p_max,p_is_fixed,variable_names);
net_p_fitted = NET.convertArray(p_fitted,'System.Double');
net_fitted_data = NET.convertArray(fitted_data(1),'System.Double');
