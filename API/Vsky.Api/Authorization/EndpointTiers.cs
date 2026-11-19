using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Vsky.Services.Authorization;

namespace Vsky.Api.Authorization
{
    public enum EndpointTier
    {
        Public,
        Service,
        Authenticated,
        SiteMember,
        Menu,
        Admin
    }

    /// <summary>Declares what an action requires. Every protected action must have exactly one tier.</summary>
    public interface IEndpointTierAttribute
    {
        EndpointTier Tier { get; }
    }

    /// <summary>Any logged-in user, with no site check. For example: switching site.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthenticatedOnlyAttribute : Attribute, IEndpointTierAttribute
    {
        public EndpointTier Tier => EndpointTier.Authenticated;
    }

    /// <summary>Any member of the active site. For example: shared dropdown lookups.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SiteMemberAttribute : Attribute, IEndpointTierAttribute
    {
        public EndpointTier Tier => EndpointTier.SiteMember;
    }

    /// <summary>
    /// Requires permission on any of the listed menus in the active site. By default GET needs
    /// View and other verbs need Manage. Use [ReadAccess] on a POST that only reads.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireMenuAttribute : Attribute, IEndpointTierAttribute
    {
        public RequireMenuAttribute(params string[] links)
        {
            // MenuLinks group constants hold several links separated by |
            Links = links
                .SelectMany(link => link.Split('|', StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .ToArray();
        }

        public string[] Links { get; }

        public EndpointTier Tier => EndpointTier.Menu;
    }

    /// <summary>A trusted caller such as the cron scheduler, using the shared key.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServiceKeyAttribute : Attribute, IEndpointTierAttribute
    {
        public EndpointTier Tier => EndpointTier.Service;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ReadAccessAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ManageAccessAttribute : Attribute
    {
    }

    /// <summary>
    /// Writes a "User activity" row for every successful call, although the action only reads.
    /// For reads that hand data out of the application, such as exports. Other reads stay
    /// governed by LogRecordAccess / LogModuleAccess.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogActivityAttribute : Attribute
    {
    }

    public sealed class EndpointPolicy
    {
        public EndpointTier? Tier { get; init; }
        public string[] Links { get; init; } = Array.Empty<string>();
        public AccessLevel? ExplicitLevel { get; init; }
        public bool AlwaysLogActivity { get; init; }

        public AccessLevel ResolveLevel(string httpMethod)
        {
            if (ExplicitLevel.HasValue)
                return ExplicitLevel.Value;

            return HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod) || HttpMethods.IsOptions(httpMethod)
                ? AccessLevel.View
                : AccessLevel.Manage;
        }
    }

    /// <summary>
    /// Reads an action's requirement from its attributes. The most specific wins: the action over
    /// the controller, and at the same level an explicit tier over [AllowAnonymous]. That lets
    /// [ServiceKey] replace a class-level [AllowAnonymous].
    /// </summary>
    public static class EndpointPolicies
    {
        private static readonly ConcurrentDictionary<string, EndpointPolicy> Cache = new();

        public static EndpointPolicy For(ControllerActionDescriptor action)
        {
            return Cache.GetOrAdd(action.Id, _ => Build(action.MethodInfo, action.ControllerTypeInfo));
        }

        private static EndpointPolicy Build(MethodInfo method, TypeInfo controller)
        {
            var tierAttribute = method.GetCustomAttributes(true).OfType<IEndpointTierAttribute>().FirstOrDefault();
            var isPublic = false;

            if (tierAttribute == null)
            {
                isPublic = method.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any();
                if (!isPublic)
                {
                    tierAttribute = controller.GetCustomAttributes(true).OfType<IEndpointTierAttribute>().FirstOrDefault();
                    isPublic = tierAttribute == null && controller.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any();
                }
            }

            return new EndpointPolicy
            {
                Tier = isPublic ? EndpointTier.Public : tierAttribute?.Tier,
                Links = (tierAttribute as RequireMenuAttribute)?.Links ?? Array.Empty<string>(),
                ExplicitLevel = ResolveExplicitLevel(method) ?? ResolveExplicitLevel(controller),
                AlwaysLogActivity = method.GetCustomAttributes(true).OfType<LogActivityAttribute>().Any()
            };
        }

        private static AccessLevel? ResolveExplicitLevel(MemberInfo member)
        {
            if (member.GetCustomAttributes(true).OfType<ManageAccessAttribute>().Any())
                return AccessLevel.Manage;

            if (member.GetCustomAttributes(true).OfType<ReadAccessAttribute>().Any())
                return AccessLevel.View;

            return null;
        }
    }
}
