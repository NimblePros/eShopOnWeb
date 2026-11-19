using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests;

[TestClass]
public class ProgramTest
{
    // One host for the whole assembly; it starts on first use. It is deliberately not disposed:
    // stopping it waits on the Seq exporters' final flush, which keeps retrying for ~10s when no
    // Seq server is running, and the test process exits right after the run anyway.
    private static readonly WebApplicationFactory<Program> s_application = new();

    public static HttpClient NewClient => s_application.CreateClient();

    public static IServiceProvider Services => s_application.Services;
}
