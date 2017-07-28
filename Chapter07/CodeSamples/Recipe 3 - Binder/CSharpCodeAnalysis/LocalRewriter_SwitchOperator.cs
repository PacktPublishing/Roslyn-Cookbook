// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        public override BoundNode VisitSwitchOperator(BoundSwitchOperator node)
        {
            // TODO: Implement lowering for switch operator.
            return MakeLiteral(node.Syntax,
                ConstantValue.Create($"CodeGen not yet implemented for: '{node.Syntax.ToString()}'"),
                _compilation.GetSpecialType(SpecialType.System_String));
        }
    }
}
