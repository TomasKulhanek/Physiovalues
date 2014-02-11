namespace RestMasterService.ComputationNodes
{
    public static class ExtensionMethods
    {
        public static void PopulateNonNull(this IdentifyDTO identify, IdentifyDTO id2)
        {
            if (id2.model != null) identify.model = id2.model;
            if (id2.name != null) identify.name = id2.name;
            //if (id2.countcycles != null) identify.countcycles = id2.countcycles;
            if (id2.elapsedtime != null) identify.elapsedtime = id2.elapsedtime;
            if (id2.Variablenames != null) identify.Variablenames = id2.Variablenames;
            if (id2.Variablevalues != null) identify.Variablevalues = id2.Variablevalues;
            if (id2.Experimentalvalues != null) identify.Experimentalvalues = id2.Experimentalvalues;
            if (id2.Parameternames != null) identify.Parameternames = id2.Parameternames;
            if (id2.Parametervalues != null) identify.Parametervalues = id2.Parametervalues;
        }

        public static void PopulateNonNull(this ResultDTO identify, ResultDTO id2)
        {
            if (id2.model != null) identify.model = id2.model;
            if (id2.name != null) identify.name = id2.name;
            //if (id2.countcycles != null) identify.countcycles = id2.countcycles;
            if (id2.elapsedtime != null) identify.elapsedtime = id2.elapsedtime;
            if (id2.Variablenames != null) identify.Variablenames = id2.Variablenames;
            if (id2.Variablevalues != null) identify.Variablevalues = id2.Variablevalues;
            if (id2.Experimentalvalues != null) identify.Experimentalvalues = id2.Experimentalvalues;
            if (id2.Parameternames != null) identify.Parameternames = id2.Parameternames;
            if (id2.Parametervalues != null) identify.Parametervalues = id2.Parametervalues;
            if (id2.ParameterAssignment != null) identify.ParameterAssignment = id2.ParameterAssignment;
        }
    }
}