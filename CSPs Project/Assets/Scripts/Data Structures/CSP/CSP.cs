
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[Serializable]
/// <summary>
/// Constraint Satisfaction Problem
/// </summary>
/// <typeparam name="T">Datatype of value (variable of concrete domain)</typeparam>
public partial class CSP<T> : COP<T>
{
    public CSP(string[] names, T[][] domains, T defaultValue = default) 
        : base(names, domains, defaultValue)
    {
    }
}
