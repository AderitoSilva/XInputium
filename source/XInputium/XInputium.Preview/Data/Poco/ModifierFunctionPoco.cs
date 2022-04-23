using System;
using XInputium;

namespace XInputium.Preview.Data.Poco;

public sealed record ModifierFunctionPoco(
    string DisplayName, ModifierFunction? Function);
