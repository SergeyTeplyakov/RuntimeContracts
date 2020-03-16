using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using System;

namespace RuntimeContracts.Analyzer
{
    internal class SolutionChangeAction : SimpleCodeAction
    {
        private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

        public SolutionChangeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey = null)
            : base(title, equivalenceKey)
        {
            _createChangedSolution = createChangedSolution;
        }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            return _createChangedSolution(cancellationToken);
        }
    }
}
