%ted tam mám nastaveno polulation size = 1 


function p = call_anni(p_n,p_bounds,eq_n)
  %in: p_n .. number of free parameters
  %p_bounds .. matrix of bounds of parameters
  %          first row - minimum, second row maximum
  %          i-th column - i-th parameter
  file_name = 'problemAnni.in';
  fid = fopen(file_name, 'w');
  fprintf(fid,'#Model name\nSome name\n');
  fprintf(fid,'#N (dimension of data space), i.e. number of equations\n%i\n',eq_n);
  fprintf(fid,'#M (dimension of model space), i.e. number of unknowns\n%i\n',p_n);
  fprintf(fid,'#Q (Population size). Recommended value is around 5-10 times the number\n%i\n',5*p_n);
  fprintf(fid,'#N_ITER Number of allowed iteration cycles. (-1 = Do not care)\n%i\n',-1);
  fprintf(fid,'#M_TIME Maximum allowed CPU time in seconds (-1 = Do not care)\n%i\n',-1);
  fprintf(fid,'#M_EVAL Maximum allowed number of function evaluations (-1 = Do not care)\n%i\n',800);
  fprintf(fid,'#M_FIT Desired norm of residuals to be reached (-1 = Do not care)\n%i\n',1.e-4);
  fprintf(fid,'#FCN_LOCATION (full_path_to_file_name function_name)\n%s %s\n','functions/','problemAnni');
  fprintf(fid,'#Model parameters\n');
% Following lines must be present M-times, while each line can be specified
% using any one from the following syntax:
% NAME MIN_VALUE MAX_VALUE (e.g. 3 parameters)
% NAME MIN_VALUE MAX_VALUE START_VALUE (e.g. 4 parameters)
% NAME MIN_VALUE MAX_VALUE START_VALUE * (e.g. 4 parameters and asterrisk at the end)
% In the last syntax specified the asterrisk forces masking the apropriate parameter from inversion and
% such parameter is fixed to the START_VALUE throughout the inversion. This can be used repeatedly for
% searching in different subspaces of the original parameter space. In our case we let both parameters
% to fall into <-10,+10> without specifying starting values.
  fprintf(fid,'#Parameter name\tmin\tmax\n');
  for i = 1:p_n
    fprintf(fid,'p%i\t%f\t%f\n',i,p_bounds(1,i),p_bounds(2,i));
  end
  fprintf(fid,'#Number of desired roots, their critical distance\n');
% If more than one solution to the problem is expected, the user can specify how many roots should be
% searched for. In such case the distance of solutions in parameter space must be specified, under which
% apparently different solutions are in fact the same. For definition of the critical distance (e.g. second
% number on the last line) see Eq(10) in the file ANNIT_EN.pdf, or look into the implementation of the
% function dist_p2() in anni.m. Unfortunatelly hardly anybody will know how far the roots
% should be each other in advance, so my recommendation is to select this distnace let us say to 
% (MAX_VALUE-MIN_VALUE)/1000 and continue by experimentation.
% If only one root is required, just type what follows:
  fprintf(fid,'%i\t%i\n',1,0);

  fclose(fid);
  
  p = ANNI('problemAnni.in');