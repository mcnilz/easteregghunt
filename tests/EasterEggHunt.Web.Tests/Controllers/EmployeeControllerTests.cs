using System.Security.Claims;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Unit Tests für EmployeeController
/// </summary>
[TestFixture]
public sealed class EmployeeControllerTests : IDisposable
{
    private Mock<ILogger<EmployeeController>> _mockLogger = null!;
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private EmployeeController _controller = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private Mock<ISession> _mockSession = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _controller = new EmployeeController(_mockLogger.Object, _mockApiClient.Object);

        // Setup HttpContext und Session
        _mockHttpContext = new Mock<HttpContext>();
        _mockSession = new Mock<ISession>();
        _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);

        // Mock ServiceProvider für Authentication
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockAuthenticationService = new Mock<IAuthenticationService>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthenticationService.Object);
        _mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);

        var tempData = new TempDataDictionary(_mockHttpContext.Object, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    #region Basic Tests

    [Test]
    public void Controller_CanBeInstantiated()
    {
        // Arrange & Act
        using var controller = new EmployeeController(_mockLogger.Object, _mockApiClient.Object);

        // Assert
        Assert.That(controller, Is.Not.Null);
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _controller?.Dispose();
        }
    }
}

