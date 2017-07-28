// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        /// <summary>
        /// Rewrite switch operator into nested conditional operators.
        /// </summary>
        public override BoundNode VisitSwitchOperator(BoundSwitchOperator node)
        {
            // just a fact, not a requirement (VisitExpression would have rewritten otherwise)
            Debug.Assert(node.ConstantValue == null);

            var rewrittenExpression = VisitExpression(node.Expression);
            var rewrittenLabels = node.Labels.SelectAsArray(l => VisitExpression(l));
            var rewrittenValues = node.Values.SelectAsArray(l => VisitExpression(l));
            var rewrittenType = VisitType(node.Type);
            var booleanType = _compilation.GetSpecialType(SpecialType.System_Boolean);

            return RewriteSwitchOperator(
                node.Syntax,
                rewrittenExpression,
                rewrittenLabels,
                rewrittenValues,
                rewrittenType,
                booleanType);
        }

        private static BoundExpression RewriteSwitchOperator(
            SyntaxNode syntax,
            BoundExpression rewrittenExpression,
            ImmutableArray<BoundExpression> rewrittenLabels,
            ImmutableArray<BoundExpression> rewrittenValues,
            TypeSymbol rewrittenType,
            TypeSymbol booleanType)
        {
            Debug.Assert(rewrittenLabels.Length >= 1);
            Debug.Assert(rewrittenLabels.Length + 1 == rewrittenValues.Length);

            var label = rewrittenLabels[0];
            var consequence = rewrittenValues[0];
            var condition = new BoundBinaryOperator(label.Syntax, BinaryOperatorKind.Equal, rewrittenExpression, label, null, null, LookupResultKind.Viable, booleanType);
            BoundExpression alternative = rewrittenLabels.Length > 1 ?
                RewriteSwitchOperator(syntax, rewrittenExpression, rewrittenLabels.RemoveAt(0), rewrittenValues.RemoveAt(0), rewrittenType, booleanType) :
                rewrittenValues[1];
            return new BoundConditionalOperator(label.Syntax, condition, consequence, alternative, null, rewrittenType);
        }
    }
}