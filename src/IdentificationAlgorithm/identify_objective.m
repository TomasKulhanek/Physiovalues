function L2E = identify_objective(p_nonfixed_val)
    %TODO add such functionality into .NET assembly global mySimulator;
    identify_logf('parameters',p_nonfixed_val);
    
    model_data = identify_evalModel(p_nonfixed_val);
    identify_logs('model_data','evaluation finished');
    L2E = identify_ssq(model_data);
    identify_logf('L2E',L2E);
      
