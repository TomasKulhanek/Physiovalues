function resid = problemAnni(fmode, p_nonfixed_val)
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

      resid = problem(p_nonfixed_val);
      return;
    case 3
% Call to this function with fmode = 3 is performed exactly once at the end of the inversion.
% User can use the solution in param() according his/her wishes.
      resid = 0;
    end
end


