function L2E = identify_objective(p_nonfixed_val)
    %TODO add such functionality into .NET assembly global mySimulator;
    model_data = identify_evalModel(p_nonfixed_val);
    L2E = identify_ssq(model_data);
    %mySimulator.notify(L2E(1));
    identify_logf('parameters',p_nonfixed_val);
    identify_logf('L2E',L2E);
      
