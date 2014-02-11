function resid = problem(p_nonfixed_val)
  resid_n = 0;
  resid = [];
  [exact_data, model_data] = evalModel(p_nonfixed_val);
  [r data_n] = size(model_data);
  for i = 1:data_n %loop over all curves
    x_m = model_data{i}(1,:);  %model x value 
    y_m = model_data{i}(2,:);  %model y value
    x_e = exact_data{i}(1,:);  %exact x value
    y_e = exact_data{i}(2,:);  %exact y value
    [r length_m] = size(x_m);
    [r length_e] = size(x_e);
    
    [min_ind, max_ind, y_m_interp] = interpolate_y_m(x_e,y_e,x_m,y_m);
    if (min_ind ~= 1) || (max_ind ~= length_e)
      error('Some points to evalate residual value in are outside models independent variable interval')
    end
    resid = [resid (y_e - y_m_interp)];
  end
  if not(isa(resid,'float'))
    resid
    error('L2E is not a number')
  end
  return;
end

