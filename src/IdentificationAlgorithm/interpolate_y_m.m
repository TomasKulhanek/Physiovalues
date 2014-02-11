function [min_ind, max_ind, y_m_interp]  = ...
      interpolate_y_m(x_e,y_e,x_m,y_m)
% interpolates x_m, y_m on x_e

  
%evaluate the error:

%      searching for smallest index in x_e (min_ind) such that 
%      x_m(1) < x_e(min_ind),
%      so that we can interpolate model data to x_e(min_ind)
  [r length_m] = size(x_m);
  [r length_e] = size(x_e);
  

  min_ind = 1;
  while x_m(1) > x_e(min_ind)
    min_ind = min_ind + 1;
    if min_ind > length_e
      error(['x_m(first) > x_e(last)'])% for ' 'i' 'th variable'])
    end
  end
%     searching for biggest index in x_e (max_ind) such that 
%     x_m(last) > x_e(max_ind),
%     so that we can interpolate model data to x_e(max_ind)
  max_ind = length_e;
  while x_m(length_m) < x_e(max_ind)
    max_ind = max_ind - 1;
    if max_ind < 1 
      error(['x_m(last) < x_e(first) for'])% ' i 'th variable'])
    end
  end
  y_m_interp = interp1(x_m,y_m,x_e(min_ind:max_ind));
end
