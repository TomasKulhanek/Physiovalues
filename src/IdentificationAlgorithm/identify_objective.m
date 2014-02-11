function L2E = identify_objective(p_nonfixed_val)

    model_data = identify_evalModel(p_nonfixed_val);
    L2E = identify_ssq(model_data);
    identify_logf('parameters',p_nonfixed_val);
    identify_logf('L2E',L2E);
      
