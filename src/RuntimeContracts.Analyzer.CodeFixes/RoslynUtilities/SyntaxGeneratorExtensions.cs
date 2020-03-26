using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;

namespace Microsoft.CodeAnalysis.Shared.Extensions
{
    public static class SyntaxGeneratorExtensions
    {
        private const string LongLength = "LongLength";

        private static readonly Dictionary<BinaryOperatorKind, BinaryOperatorKind> s_negatedBinaryMap =
            new Dictionary<BinaryOperatorKind, BinaryOperatorKind>
            {
                {BinaryOperatorKind.Equals, BinaryOperatorKind.NotEquals},
                {BinaryOperatorKind.NotEquals, BinaryOperatorKind.Equals},
                {BinaryOperatorKind.LessThan, BinaryOperatorKind.GreaterThanOrEqual},
                {BinaryOperatorKind.GreaterThan, BinaryOperatorKind.LessThanOrEqual},
                {BinaryOperatorKind.LessThanOrEqual, BinaryOperatorKind.GreaterThan},
                {BinaryOperatorKind.GreaterThanOrEqual, BinaryOperatorKind.LessThan},
                {BinaryOperatorKind.Or, BinaryOperatorKind.And},
                {BinaryOperatorKind.And, BinaryOperatorKind.Or},
                {BinaryOperatorKind.ConditionalOr, BinaryOperatorKind.ConditionalAnd},
                {BinaryOperatorKind.ConditionalAnd, BinaryOperatorKind.ConditionalOr},
            };

        public static SyntaxNode? Negate(
            SyntaxNode expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return Negate(expression, semanticModel, negateBinary: true, cancellationToken);
        }

        public static SyntaxNode AddParentheses(SyntaxNode expression, bool includeElasticTrivia = true, bool addSimplifierAnnotation = true)
        {
            return Parenthesize(expression, includeElasticTrivia, addSimplifierAnnotation);
        }

        public static SyntaxNode? Negate(
            //this SyntaxGenerator generator,
            SyntaxNode expression,
            SemanticModel semanticModel,
            bool negateBinary,
            CancellationToken cancellationToken)
        {
            //var syntaxFacts = generator.SyntaxFacts;
            if (expression.IsParenthesizedExpression())
            {
                var negatedExpression = Negate(
                    expression.GetExpressionOfParenthesizedExpression(),
                    semanticModel,
                    negateBinary,
                    cancellationToken);
                
                if (negatedExpression == null)
                {
                    return null;
                }

                return AddParentheses(negatedExpression).WithTriviaFrom(expression);
            }
            if (negateBinary && expression.IsBinaryExpression())
            {
                return GetNegationOfBinaryExpression(expression, semanticModel, cancellationToken);
            }
            else if (expression.IsLiteralExpression())
            {
                return GetNegationOfLiteralExpression(expression, semanticModel);
            }
            else if (expression.IsLogicalNotExpression())
            {
                return GetNegationOfLogicalNotExpression(expression);
            }

            return expression.LogicalNotExpression();
        }

        private static SyntaxNode? GetNegationOfBinaryExpression(
            SyntaxNode expressionNode,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            expressionNode.GetPartsOfBinaryExpression(out var leftOperand, out var operatorToken, out var rightOperand);

            var binaryOperation = semanticModel.GetOperation(expressionNode, cancellationToken) as IBinaryOperation;
            if (binaryOperation == null)
            {
                // Apply the logical not operator if it is not a binary operation.
                return expressionNode.LogicalNotExpression();
            }

            if (!s_negatedBinaryMap.TryGetValue(binaryOperation.OperatorKind, out var negatedKind))
            {
                return expressionNode.LogicalNotExpression();
            }
            else
            {
                var negateOperands = false;
                switch (binaryOperation.OperatorKind)
                {
                    case BinaryOperatorKind.Or:
                    case BinaryOperatorKind.And:
                    case BinaryOperatorKind.ConditionalAnd:
                    case BinaryOperatorKind.ConditionalOr:
                        negateOperands = true;
                        break;
                }

                //Workaround for https://github.com/dotnet/roslyn/issues/23956
                //Issue to remove this when above is merged
                if (binaryOperation.OperatorKind == BinaryOperatorKind.Or && expressionNode.IsLogicalOrExpression())
                {
                    negatedKind = BinaryOperatorKind.ConditionalAnd;
                }
                else if (binaryOperation.OperatorKind == BinaryOperatorKind.And && expressionNode.IsLogicalAndExpression())
                {
                    negatedKind = BinaryOperatorKind.ConditionalOr;
                }

                SyntaxNode? newLeftOperand = leftOperand;
                SyntaxNode? newRightOperand = rightOperand;
                if (negateOperands)
                {
                    newLeftOperand = Negate(leftOperand, semanticModel, cancellationToken);
                    newRightOperand = Negate(rightOperand, semanticModel, cancellationToken);
                }

                if (newLeftOperand is null || newRightOperand is null)
                {
                    return null;
                }

                var newBinaryExpressionSyntax = NewBinaryOperation(binaryOperation, newLeftOperand, negatedKind, newRightOperand, cancellationToken)
                    .WithTriviaFrom(expressionNode);
                
                if (newBinaryExpressionSyntax == null)
                {
                    return null;
                }

                var newToken = newBinaryExpressionSyntax.GetOperatorTokenOfBinaryExpression();
                var newTokenWithTrivia = newToken.WithTriviaFrom(operatorToken);
                return newBinaryExpressionSyntax.ReplaceToken(newToken, newTokenWithTrivia);
            }
        }


        private static SyntaxNode CreateBinaryExpression(SyntaxKind syntaxKind, SyntaxNode left, SyntaxNode right)
        {
            return SyntaxFactory.BinaryExpression(syntaxKind, Parenthesize(left), Parenthesize(right));
        }

        public class MyGenerator
        {
            public SyntaxNode ValueEqualsExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.EqualsExpression, left, right);
            }

            public SyntaxNode ReferenceEqualsExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.EqualsExpression, left, right);
            }

            public SyntaxNode ValueNotEqualsExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
            }

            public SyntaxNode ReferenceNotEqualsExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
            }

            public SyntaxNode LessThanExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.LessThanExpression, left, right);
            }

            public SyntaxNode LessThanOrEqualExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.LessThanOrEqualExpression, left, right);
            }

            public SyntaxNode GreaterThanExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
            }

            public SyntaxNode GreaterThanOrEqualExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, left, right);
            }

            public SyntaxNode BitwiseAndExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.BitwiseAndExpression, left, right);
            }

            public SyntaxNode BitwiseOrExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.BitwiseOrExpression, left, right);
            }

            public SyntaxNode BitwiseNotExpression(SyntaxNode operand)
            {
                return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, Parenthesize(operand));
            }

            public SyntaxNode LogicalOrExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.LogicalOrExpression, left, right);
            }

            public SyntaxNode LogicalNotExpression(SyntaxNode expression)
            {
                return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, Parenthesize(expression));
            }

            public SyntaxNode LogicalAndExpression(SyntaxNode left, SyntaxNode right)
            {
                return CreateBinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
            }
        }

        private static SyntaxNode? NewBinaryOperation(
            IBinaryOperation binaryOperation,
            SyntaxNode leftOperand,
            BinaryOperatorKind operationKind,
            SyntaxNode rightOperand,
            CancellationToken cancellationToken)
        {
            MyGenerator generator = new MyGenerator();
            switch (operationKind)
            {
                case BinaryOperatorKind.Equals:
                    return binaryOperation.LeftOperand.Type?.IsValueType == true && binaryOperation.RightOperand.Type?.IsValueType == true
                        ? generator.ValueEqualsExpression(leftOperand, rightOperand)
                        : generator.ReferenceEqualsExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.NotEquals:
                    return binaryOperation.LeftOperand.Type?.IsValueType == true && binaryOperation.RightOperand.Type?.IsValueType == true
                        ? generator.ValueNotEqualsExpression(leftOperand, rightOperand)
                        : generator.ReferenceNotEqualsExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.LessThanOrEqual:
                    return IsSpecialCaseBinaryExpression(binaryOperation, operationKind, cancellationToken)
                        ? generator.ValueEqualsExpression(leftOperand, rightOperand)
                        : generator.LessThanOrEqualExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return IsSpecialCaseBinaryExpression(binaryOperation, operationKind, cancellationToken)
                        ? generator.ValueEqualsExpression(leftOperand, rightOperand)
                        : generator.GreaterThanOrEqualExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.LessThan:
                    return generator.LessThanExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.GreaterThan:
                    return generator.GreaterThanExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.Or:
                    return generator.BitwiseOrExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.And:
                    return generator.BitwiseAndExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.ConditionalOr:
                    return generator.LogicalOrExpression(leftOperand, rightOperand);
                case BinaryOperatorKind.ConditionalAnd:
                    return generator.LogicalAndExpression(leftOperand, rightOperand);
            }

            return null;
        }

        /// <summary>
        /// Returns true if the binaryExpression consists of an expression that can never be negative, 
        /// such as length or unsigned numeric types, being compared to zero with greater than, 
        /// less than, or equals relational operator.
        /// </summary>
        public static bool IsSpecialCaseBinaryExpression(
            IBinaryOperation binaryOperation,
            BinaryOperatorKind operationKind,
            CancellationToken cancellationToken)
        {
            if (binaryOperation == null)
            {
                return false;
            }

            var rightOperand = RemoveImplicitConversion(binaryOperation.RightOperand);
            var leftOperand = RemoveImplicitConversion(binaryOperation.LeftOperand);

            switch (operationKind)
            {
                case BinaryOperatorKind.LessThanOrEqual when rightOperand.IsNumericLiteral():
                    return CanSimplifyToLengthEqualsZeroExpression(
                        leftOperand, (ILiteralOperation)rightOperand);
                case BinaryOperatorKind.GreaterThanOrEqual when leftOperand.IsNumericLiteral():
                    return CanSimplifyToLengthEqualsZeroExpression(
                        rightOperand, (ILiteralOperation)leftOperand);
            }

            return false;
        }

        private static IOperation RemoveImplicitConversion(IOperation operation)
        {
            return operation is IConversionOperation conversion && conversion.IsImplicit
                ? RemoveImplicitConversion(conversion.Operand)
                : operation;
        }

        private static bool CanSimplifyToLengthEqualsZeroExpression(
            IOperation variableExpression, ILiteralOperation numericLiteralExpression)
        {
            var numericValue = numericLiteralExpression.ConstantValue;
            if (numericValue.HasValue && numericValue.Value is 0)
            {
                if (variableExpression is IPropertyReferenceOperation propertyOperation)
                {
                    var property = propertyOperation.Property;
                    if ((property.Name == nameof(Array.Length) || property.Name == LongLength))
                    {
                        var containingType = property.ContainingType;
                        if (containingType?.SpecialType == SpecialType.System_Array ||
                            containingType?.SpecialType == SpecialType.System_String)
                        {
                            return true;
                        }
                    }
                }

                var type = variableExpression.Type;
                if (type != null)
                {
                    switch (type.SpecialType)
                    {
                        case SpecialType.System_Byte:
                        case SpecialType.System_UInt16:
                        case SpecialType.System_UInt32:
                        case SpecialType.System_UInt64:
                            return true;
                    }
                }
            }

            return false;
        }

        public static SyntaxNode FalseLiteralExpression()
        {
            return GenerateBooleanLiteralExpression(false);
        }

        public static SyntaxNode TrueLiteralExpression()
        {
            return GenerateBooleanLiteralExpression(true);
        }

        private static ExpressionSyntax GenerateBooleanLiteralExpression(bool val)
        {
            return SyntaxFactory.LiteralExpression(val
                ? SyntaxKind.TrueLiteralExpression
                : SyntaxKind.FalseLiteralExpression);
        }

        //public static SyntaxNode NullLiteralExpression()
        //{
        //    return LiteralExpression(null);
        //}

        public static SyntaxNode LogicalNotExpression(SyntaxNode expression)
        {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, Parenthesize(expression));
        }

        public static ExpressionSyntax Parenthesize(SyntaxNode expression, bool includeElasticTrivia = true, bool addSimplifierAnnotation = true)
        {
            return ((ExpressionSyntax)expression).Parenthesize(includeElasticTrivia, addSimplifierAnnotation);
        }

        private static SyntaxNode GetNegationOfLiteralExpression(
            SyntaxNode expression,
            SemanticModel semanticModel)
        {
            var operation = semanticModel.GetOperation(expression);
            SyntaxNode newLiteralExpression;

            if (operation?.Kind == OperationKind.Literal && operation.ConstantValue.HasValue && operation.ConstantValue.Value is true)
            {
                newLiteralExpression = FalseLiteralExpression();
            }
            else if (operation?.Kind == OperationKind.Literal && operation.ConstantValue.HasValue && operation.ConstantValue.Value is false)
            {
                newLiteralExpression = TrueLiteralExpression();
            }
            else
            {
                newLiteralExpression = LogicalNotExpression(expression.WithoutTrivia());
            }

            return newLiteralExpression.WithTriviaFrom(expression);
        }

        private static SyntaxNode GetNegationOfLogicalNotExpression(
            SyntaxNode expression)
        {
            var operatorToken = GetOperatorTokenOfPrefixUnaryExpression(expression);
            var operand = GetOperandOfPrefixUnaryExpression(expression);

            return operand.WithPrependedLeadingTrivia(operatorToken.LeadingTrivia)
                .WithAdditionalAnnotations(Simplifier.Annotation);
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            SyntaxTriviaList trivia) where T : SyntaxNode
        {
            if (trivia.Count == 0)
            {
                return node;
            }

            return node.WithLeadingTrivia(trivia.Concat(node.GetLeadingTrivia()));
        }

        public static bool IsNumericLiteral(this IOperation operation)
            => operation.Kind == OperationKind.Literal && operation.Type.IsNumericType();

        public static bool IsNullLiteral(this IOperation operand)
            => operand is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } };

        public static SyntaxToken WithPrependedLeadingTrivia(
            this SyntaxToken token,
            SyntaxTriviaList trivia)
        {
            if (trivia.Count == 0)
            {
                return token;
            }

            return token.WithLeadingTrivia(trivia.Concat(token.LeadingTrivia));
        }

        public static SyntaxToken GetOperatorTokenOfPrefixUnaryExpression(SyntaxNode node)
            => ((PrefixUnaryExpressionSyntax)node).OperatorToken;

        public static SyntaxNode GetOperandOfPrefixUnaryExpression(SyntaxNode node)
            => ((PrefixUnaryExpressionSyntax)node).Operand;
    }
}