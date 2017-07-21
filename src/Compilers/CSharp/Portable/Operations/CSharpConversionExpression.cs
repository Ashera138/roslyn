﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal abstract class BaseCSharpConversionExpression : BaseConversionExpression<Conversion>
    {
        protected BaseCSharpConversionExpression(Conversion conversion, bool isExplicitInCode, bool throwsExceptionOnFailure, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue) :
            base(conversion, isExplicitInCode, throwsExceptionOnFailure, syntax, type, constantValue)
        {
        }

        public override string LanguageName => LanguageNames.CSharp;
        // We override this here so that we don't return the method symbol if the internal conversion is a MethodGroup conversion, instead of
        // a user defined conversion. Operator method should only be returning a MethodSymbol if IsUserDefined is true.
        public override IMethodSymbol OperatorMethod => ConversionInternal.IsUserDefined ? ConversionInternal.MethodSymbol : null;
    }

    internal sealed partial class CSharpConversionExpression : BaseCSharpConversionExpression
    {
        public CSharpConversionExpression(IOperation operand, Conversion conversion, bool isExplicit, bool throwsExceptionOnFailure, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue) :
            base(conversion, isExplicit, throwsExceptionOnFailure, syntax, type, constantValue)
        {
            Operand = operand;
        }

        public override IOperation Operand { get; }
    }

    internal sealed partial class LazyCSharpConversionExpression : BaseCSharpConversionExpression
    {
        private readonly Lazy<IOperation> _operand;
        public LazyCSharpConversionExpression(Lazy<IOperation> operand, Conversion conversion, bool isExplicit, bool throwsExceptionOnFailure, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue) :
            base(conversion, isExplicit, throwsExceptionOnFailure, syntax, type, constantValue)
        {
            _operand = operand;
        }

        public override IOperation Operand => _operand.Value;
    }

    public static class IConversionExpressionExtensions
    {
        /// <summary>
        /// Gets the underlying <see cref="Conversion"/> information from this <see cref="IConversionExpression"/>. This
        /// <see cref="IConversionExpression"/> must have been created from CSharp code.
        /// </summary>
        /// <param name="conversionExpression">The conversion expression to get original info from.</param>
        /// <returns>The underlying <see cref="Conversion"/>.</returns>
        /// <exception cref="InvalidCastException">If the <see cref="IConversionExpression"/> was not created from CSharp code.</exception>
        public static Conversion GetCSharpConversion(this IConversionExpression conversionExpression)
        {
            if (conversionExpression is BaseCSharpConversionExpression csharpConversionExpression)
            {
                return csharpConversionExpression.ConversionInternal;
            }
            else
            {
                throw new InvalidCastException(string.Format(CSharpResources.IConversionExpression_Is_Not_A_Valid_CSharp_Conversion, conversionExpression));
            }
        }
    }
}
