using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vsky.Services.Authorization;

namespace Vsky.Api.Authorization
{
    public sealed record EndpointCoverage(
        int TotalActions,
        IReadOnlyDictionary<string, int> ActionsPerTier,
        IReadOnlyList<string> UnclassifiedActions);

    /// <summary>
    /// Finds every controller action with no declared tier, so a forgotten endpoint is visible
    /// without a test project. Once everything is enforced, a gap stops startup outside Production.
    /// In Production it logs instead: the filter already denies those actions, and one missed
    /// attribute shouldn't take the whole application down.
    /// </summary>
    public class EndpointClassificationScan : IHostedService
    {
        private readonly IActionDescriptorCollectionProvider _actions;
        private readonly SecurityAuthorizationOptions _options;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<EndpointClassificationScan> _logger;

        public EndpointClassificationScan(
            IActionDescriptorCollectionProvider actions,
            IOptions<SecurityAuthorizationOptions> options,
            IHostEnvironment environment,
            ILogger<EndpointClassificationScan> logger)
        {
            _actions = actions;
            _options = options.Value;
            _environment = environment;
            _logger = logger;
        }

        public EndpointCoverage GetCoverage()
        {
            var actions = _actions.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToList();

            return new EndpointCoverage(
                actions.Count,
                actions
                    .GroupBy(action => EndpointPolicies.For(action).Tier?.ToString() ?? "Unclassified")
                    .OrderBy(group => group.Key)
                    .ToDictionary(group => group.Key, group => group.Count()),
                actions
                    .Where(action => EndpointPolicies.For(action).Tier == null)
                    .Select(action => $"{action.ControllerName}.{action.ActionName}")
                    .OrderBy(name => name)
                    .ToList());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var coverage = GetCoverage();
            var mustBeComplete = _options.Mode == AuthorizationMode.Enforce && _options.EnforceAllLinks;

            if (coverage.UnclassifiedActions.Count == 0)
            {
                _logger.LogInformation("Authorization scan: all {Total} actions are classified.", coverage.TotalActions);
            }
            else if (!mustBeComplete)
            {
                _logger.LogWarning("Authorization scan: {Count} of {Total} actions are unclassified: {Actions}",
                    coverage.UnclassifiedActions.Count, coverage.TotalActions, string.Join(", ", coverage.UnclassifiedActions.Take(100)));
            }
            else if (_environment.IsProduction())
            {
                _logger.LogCritical("Authorization scan: these unclassified actions are being denied: {Actions}",
                    string.Join(", ", coverage.UnclassifiedActions));
            }
            else
            {
                throw new InvalidOperationException(
                    "Authorization scan: every action must declare a tier. Unclassified: "
                    + string.Join(", ", coverage.UnclassifiedActions));
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
