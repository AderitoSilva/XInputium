using System;

namespace XInputium;

/// <summary>
/// Delegate that represents a function that can be used 
/// to modify a normalized value in order to process it 
/// — i.e. to provide non-linear functions.
/// </summary>
/// <param name="normalValue">A normalized value (between 
/// 0 and 1).</param>
/// <returns>The modified value. This must not be 
/// <see cref="float.NaN"/>.</returns>
public delegate float ModifierFunction(float normalValue);
