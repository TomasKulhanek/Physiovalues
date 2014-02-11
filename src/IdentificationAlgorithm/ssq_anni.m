function resid = ssq_anni(fmode, p_nonfixed_val)
  switch fmode
    case 0
% Anytime ANNI has been started, this function is called with fmode = 0
% exactly once at the beginning of inversion. The purpose is to allow the user
% to make preparatory steps. Output can be arbitrary, since resid() is not used at all.
      resid = 0;
      return;
    case 1
% This part of code is called usually many times. User should respond properly using resid()
% according the actual values in param().

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
	  error('Some points to evalate residual value in are outside models ...
		independent variable interval')
	end
	resid = [resid (y_e - y_m_interp)]
      end
      if not(isa(resid,'float'))
	resid
	error('L2E is not a number')
      end
      resid 
      return;
    case 3
% Call to this function with fmode = 3 is performed exactly once at the end of the inversion.
% User can use the solution in param() according his/her wishes.
      resid = 0;
    end
  end
end

