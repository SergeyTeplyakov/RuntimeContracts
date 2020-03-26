using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeContracts.Analyzer;

namespace Microsoft.CodeAnalysis.Shared.Extensions
{
    public static class NegateExtensions
    {
        public static bool IsParenthesizedExpression([NotNullWhen(true)] this SyntaxNode? node)
            => node?.RawKind == (int)SyntaxKind.ParenthesizedExpression;

        public static bool IsExpressionOfInvocationExpression(this SyntaxNode node)
            => (node?.Parent as InvocationExpressionSyntax)?.Expression == node;

        public static bool IsExpressionOfAwaitExpression(this SyntaxNode node)
            => (node?.Parent as AwaitExpressionSyntax)?.Expression == node;

        public static bool IsExpressionOfMemberAccessExpression(this SyntaxNode node)
            => (node?.Parent as MemberAccessExpressionSyntax)?.Expression == node;

        public static SyntaxNode GetExpressionOfInvocationExpression(this SyntaxNode node)
            => ((InvocationExpressionSyntax)node).Expression;

        public static SyntaxNode GetExpressionOfAwaitExpression(this SyntaxNode node)
            => ((AwaitExpressionSyntax)node).Expression;

        public static bool IsExpressionOfForeach(this SyntaxNode node)
            => node?.Parent is ForEachStatementSyntax foreachStatement && foreachStatement.Expression == node;

        public static SyntaxNode GetExpressionOfExpressionStatement(this SyntaxNode node)
            => ((ExpressionStatementSyntax)node).Expression;

        public static bool IsBinaryExpression(this SyntaxNode node)
            => node is BinaryExpressionSyntax;

        public static bool IsLiteralExpression(this SyntaxNode node)
            => node is LiteralExpressionSyntax;

        public static bool IsLogicalNotExpression([NotNullWhen(true)] this SyntaxNode? node)
            => node?.Kind() == SyntaxKind.LogicalNotExpression;

        public static bool IsLogicalAndExpression([NotNullWhen(true)] this SyntaxNode? node)
            => node?.RawKind == (int)SyntaxKind.LogicalAndExpression;

        public static SyntaxToken GetOperatorTokenOfBinaryExpression(this SyntaxNode node)
        {
            node.GetPartsOfBinaryExpression(out _, out var token, out _);
            return token;
        }

        public static SyntaxNode GetExpressionOfParenthesizedExpression(this SyntaxNode node)
            => ((ParenthesizedExpressionSyntax)node).Expression;

        public static bool IsLogicalOrExpression([NotNullWhen(true)] this SyntaxNode? node)
            => node?.RawKind == (int)SyntaxKind.LogicalOrExpression;

        public static void GetPartsOfBinaryExpression(this SyntaxNode node, out SyntaxNode left, out SyntaxToken operatorToken, out SyntaxNode right)
        {
            var binaryExpression = (BinaryExpressionSyntax)node;
            left = binaryExpression.Left;
            operatorToken = binaryExpression.OperatorToken;
            right = binaryExpression.Right;
        }

        public static SyntaxNode LogicalNotExpression(this SyntaxNode expression)
        {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, 
                SyntaxGeneratorExtensions.Parenthesize(expression));
        }
    }
}