﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.ComponentModel;
using System.IO.Pipes;
using System.Net;
using System.Runtime.Versioning;
using OpenIddict.Client.SystemIntegration;
using static OpenIddict.Client.SystemIntegration.OpenIddictClientSystemIntegrationAuthenticationMode;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Exposes the necessary methods required to configure
/// the OpenIddict client system integration.
/// </summary>
public sealed class OpenIddictClientSystemIntegrationBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="OpenIddictClientSystemIntegrationBuilder"/>.
    /// </summary>
    /// <param name="services">The services collection.</param>
    public OpenIddictClientSystemIntegrationBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    /// Amends the default OpenIddict client system integration configuration.
    /// </summary>
    /// <param name="configuration">The delegate used to configure the OpenIddict options.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder Configure(Action<OpenIddictClientSystemIntegrationOptions> configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        Services.Configure(configuration);

        return this;
    }

    /// <summary>
    /// Uses an AS web authentication session to start interactive authentication and logout flows.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [SupportedOSPlatform("ios12.0")]
    [SupportedOSPlatform("maccatalyst13.1")]
    [SupportedOSPlatform("macos10.15")]
    public OpenIddictClientSystemIntegrationBuilder UseASWebAuthenticationSession()
    {
        if (!IsASWebAuthenticationSessionSupported())
        {
            throw new PlatformNotSupportedException(SR.GetResourceString(SR.ID0446));
        }

        return Configure(options => options.AuthenticationMode = ASWebAuthenticationSession);
    }

    /// <summary>
    /// Uses a custom tabs intent to start interactive authentication and logout flows.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [SupportedOSPlatform("android21.0")]
    public OpenIddictClientSystemIntegrationBuilder UseCustomTabsIntent()
    {
        if (!IsCustomTabsIntentSupported())
        {
            throw new PlatformNotSupportedException(SR.GetResourceString(SR.ID0452));
        }

        return Configure(options => options.AuthenticationMode = CustomTabsIntent);
    }

    /// <summary>
    /// Uses the Windows web authentication broker to start interactive authentication and logout flows.
    /// </summary>
    /// <remarks>
    /// Note: the web authentication broker is only supported in UWP applications
    /// and its use is generally not recommended due to its inherent limitations.
    /// </remarks>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [SupportedOSPlatform("windows10.0.17763")]
    public OpenIddictClientSystemIntegrationBuilder UseWebAuthenticationBroker()
    {
        if (!IsWebAuthenticationBrokerSupported())
        {
            throw new PlatformNotSupportedException(SR.GetResourceString(SR.ID0392));
        }

        return Configure(options => options.AuthenticationMode = WebAuthenticationBroker);
    }

    /// <summary>
    /// Uses the system browser to start interactive authentication and logout flows.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder UseSystemBrowser()
        => Configure(options => options.AuthenticationMode = SystemBrowser);

    /// <summary>
    /// Sets the list of static ports the embedded web server will be allowed to
    /// listen on, if enabled. The first port in the list that is not already used
    /// by another program is automatically chosen and the other ports are ignored.
    /// </summary>
    /// <remarks>
    /// If this property is not explicitly set, a port in the 49152-65535
    /// dynamic ports range is automatically chosen by OpenIddict at runtime.
    /// </remarks>
    /// <param name="ports">The static ports the embedded web server will be allowed to listen on.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder SetAllowedEmbeddedWebServerPorts(params int[] ports)
    {
        if (Array.Exists(ports, static port => port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort))
        {
            throw new ArgumentOutOfRangeException(nameof(ports));
        }

        return Configure(options =>
        {
            options.AllowedEmbeddedWebServerPorts.Clear();
            options.AllowedEmbeddedWebServerPorts.AddRange(ports);
        });
    }

    /// <summary>
    /// Sets the timeout after which authentication demands that
    /// are not completed are automatically aborted by OpenIddict.
    /// </summary>
    /// <param name="timeout">The authentication timeout.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder SetAuthenticationTimeout(TimeSpan timeout)
        => Configure(options => options.AuthenticationTimeout = timeout);

    /// <summary>
    /// Disables the built-in protocol activation processing logic.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder DisableActivationHandling()
        => Configure(options => options.EnableActivationHandling = false);

    /// <summary>
    /// Enables the built-in protocol activation processing logic.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder EnableActivationHandling()
        => Configure(options => options.EnableActivationHandling = true);

    /// <summary>
    /// Disables the built-in protocol activation redirection logic.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder DisableActivationRedirection()
        => Configure(options => options.EnableActivationRedirection = false);

    /// <summary>
    /// Enables the built-in protocol activation redirection logic.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder EnableActivationRedirection()
        => Configure(options => options.EnableActivationRedirection = true);

    /// <summary>
    /// Disables the built-in web server used to handle callbacks.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder DisableEmbeddedWebServer()
        => Configure(options => options.EnableEmbeddedWebServer = false);

    /// <summary>
    /// Enables the built-in web server used to handle callbacks.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder EnableEmbeddedWebServer()
        => Configure(options => options.EnableEmbeddedWebServer = true);

    /// <summary>
    /// Disables the pipe server used to process notifications (e.g protocol
    /// activation redirections) sent by other instances of the application.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder DisablePipeServer()
        => Configure(options => options.EnablePipeServer = false);

    /// <summary>
    /// Enables the pipe server used to process protocol
    /// activations redirected by other instances of the application.
    /// </summary>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public OpenIddictClientSystemIntegrationBuilder EnablePipeServer()
        => Configure(options => options.EnablePipeServer = true);

    /// <summary>
    /// Sets the application discriminator used to define a static pipe
    /// name that will be shared by all the instances of the application.
    /// </summary>
    /// <param name="discriminator">The application discriminator.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OpenIddictClientSystemIntegrationBuilder SetApplicationDiscriminator(string discriminator)
    {
        if (string.IsNullOrEmpty(discriminator))
        {
            throw new ArgumentException(SR.FormatID0366(nameof(discriminator)), nameof(discriminator));
        }

        return Configure(options => options.ApplicationDiscriminator = discriminator);
    }

    /// <summary>
    /// Sets the identifier used to represent the current application
    /// instance and redirect protocol activations when necessary.
    /// </summary>
    /// <param name="identifier">The identifier of the current instance.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OpenIddictClientSystemIntegrationBuilder SetInstanceIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException(SR.FormatID0366(nameof(identifier)), nameof(identifier));
        }

        return Configure(options => options.InstanceIdentifier = identifier);
    }

    /// <summary>
    /// Sets the base name of the pipe created by OpenIddict to enable
    /// inter-process communication and handle protocol activation redirections.
    /// </summary>
    /// <param name="name">The name of the pipe.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OpenIddictClientSystemIntegrationBuilder SetPipeName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(SR.FormatID0366(nameof(name)), nameof(name));
        }

        return Configure(options => options.PipeName = name);
    }

    /// <summary>
    /// Sets the options applied to the pipe created by OpenIddict to enable
    /// inter-process communication and handle protocol activation redirections.
    /// </summary>
    /// <param name="flags">The options flags applied to the pipe.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OpenIddictClientSystemIntegrationBuilder SetPipeOptions(PipeOptions flags)
        => Configure(options => options.PipeOptions = flags);

    /// <summary>
    /// Sets the security policy applied to the pipe created by OpenIddict to enable
    /// inter-process communication and handle protocol activation redirections.
    /// </summary>
    /// <param name="security">The security policy applied to the pipe.</param>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced), SupportedOSPlatform("windows")]
    public OpenIddictClientSystemIntegrationBuilder SetPipeSecurity(PipeSecurity security)
    {
        if (security is null)
        {
            throw new ArgumentNullException(nameof(security));
        }

        return Configure(options => options.PipeSecurity = security);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, false.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => base.Equals(obj);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString() => base.ToString();
}
