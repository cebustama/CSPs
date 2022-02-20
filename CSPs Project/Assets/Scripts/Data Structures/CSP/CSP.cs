
using System;
using UnityEngine;

[Serializable]
/// <summary>
/// Constraint Satisfaction Problem
/// Un problema discreto de satisfacción de restricciones (CSP) es un 
/// COP (X,D,R) tal que todas las relaciones ri∈R son restricciones duras.
/// </summary>
/// <typeparam name="T">Datatype of value (variable of concrete domain)</typeparam>
public partial class CSP<T> : COP<T>
{
    public CSP(string[] names, T[][] domains, T defaultValue = default) 
        : base(names, domains, defaultValue)
    {
    }

    protected override void ProcessConstraint(string[] vIds, Constraint<T> constraint)
    {
        base.ProcessConstraint(vIds, constraint);

        // TODO: Setup each variable utility function as hard constraint
        // If satisfied ->  utility = 0
        // If broken    ->  utility = -inf
    }
}
