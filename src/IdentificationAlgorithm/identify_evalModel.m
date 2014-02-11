function model_data = identify_evalModel(p_nonfixed_val)
    %model_data array (different model evaluations) of 2D array ([m,n] - m 
    %combine fixed and nonfixed parameters:
    %global model_struct;
    %model_struct=my_model_struct;
    global model_struct;
    %model_name = model_struct.model_name;
    grid = model_struct.experiment_times;
    names = model_struct.p_names;
    variables = model_struct.variable_names;
    %p_nonfix_val(ith_individuals, jth_parameter) 
    [nIndividuals,nNonFix] = size(p_nonfixed_val);
    %combining fixed and nonfixed parameters:
    p_fixed_val = model_struct.p_fixed_val;
    %[~,nFix] = size(p_fixed_val); matlab 2008 fix
    [tempfix,nFix] = size(p_fixed_val);
    p_is_fixed = model_struct.p_is_fixed;
    %[~, nPar] = size(p_is_fixed);matlab 2008 fix
    [tempfix, nPar] = size(p_is_fixed);
    if (nPar ~= nFix + nNonFix)
        error('nFix + nNonFix ~= nPar')
    end
    i_fixed = 1;
    i_nonfixed = 1;
    p = zeros(nIndividuals,nPar);
    for i = 1:nPar
        if p_is_fixed(i)
          p(:,i) = p_fixed_val(i_fixed)*ones(nIndividuals,1);
          i_fixed = i_fixed + 1;
        else
          p(:,i) = p_nonfixed_val(:,i_nonfixed);
          i_nonfixed = i_nonfixed + 1;
        end
    end

    %number_evaluation
  

%solve the model:
  model_data = identify_simulate(names,p,variables,grid);

  %model_struct.evalN = model_struct.evalN + 1;

end