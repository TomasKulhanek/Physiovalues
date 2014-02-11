function L2E = objective(p_nonfixed_val)

    model_data = evalModel(p_nonfixed_val);
    L2E = ssq(model_data);
      
