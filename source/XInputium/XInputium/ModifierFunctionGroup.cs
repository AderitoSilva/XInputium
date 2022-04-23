using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XInputium;

/// <summary>
/// Provides a collection that combines multiple 
/// <see cref="XInputium.ModifierFunction"/> delegates into 
/// one single <see cref="XInputium.ModifierFunction"/>.
/// </summary>
/// <remarks>
/// Use <see cref="ModifierFunction"/> property to obtain 
/// the <see cref="XInputium.ModifierFunction"/> delegate 
/// that represents the <see cref="ModifierFunctionGroup"/>.
/// <br/><br/>
/// Modifier functions in the <see cref="ModifierFunctionGroup"/> 
/// are evaluated sequentially, from the first element to the 
/// last.
/// </remarks>
/// <seealso cref="XInputium.ModifierFunction"/>
public class ModifierFunctionGroup : Collection<ModifierFunction>
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a <see cref="ModifierFunctionGroup"/> 
    /// class.
    /// </summary>
    public ModifierFunctionGroup()
        : base()
    {
        ModifierFunction = new ModifierFunction(Evaluate);
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the modifier function that evaluates a value using all 
    /// of the modifier functions in the <see cref="ModifierFunctionGroup"/>.
    /// </summary>
    public ModifierFunction ModifierFunction { get; }

    #endregion Properties


    #region Methods

    private float Evaluate(float normalValue)
    {
        if (float.IsNaN(normalValue))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for " +
                $"'{normalValue}' parameter.",
                nameof(normalValue));

        for (int i = 0; i < Count; i++)
        {
            normalValue = this[i]?.Invoke(normalValue) ?? normalValue;
        }
        return normalValue;
    }

    #endregion Methods


}
