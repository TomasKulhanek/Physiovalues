function ics = identify_getsimulationtime()

    global mySimulator;
    %identify_log('simulation time'); 
    ics = mySimulator.GetSimulationTime();
    identify_logd('simulation time:',ics); 
