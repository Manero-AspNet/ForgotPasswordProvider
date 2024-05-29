using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Data.Entities;
using ForgotPasswordProvider.Models;
using ForgotPasswordProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Tests.Tests;

public class ForgotPasswordService_Tests
{
    private readonly Mock<ILogger<ForgotPasswordService>> _mockLogger;
    private readonly Mock<DataContext> _mockContext;
    private readonly ForgotPasswordService _service;

    public ForgotPasswordService_Tests()
    {
        _mockLogger = new Mock<ILogger<ForgotPasswordService>>();
        _mockContext = new Mock<DataContext>();
        _service = new ForgotPasswordService(_mockLogger.Object, _mockContext.Object);
    }

    [Fact]
    public async Task UnpackForgotPasswordRequest_ShouldReturnRequestObject_WhenValidRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        var httpRequest = new Mock<HttpRequest>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(JsonConvert.SerializeObject(request));
        await writer.FlushAsync();
        stream.Position = 0;
        httpRequest.Setup(x => x.Body).Returns(stream);

        // Act
        var result = await _service.UnpackForgotPasswordRequest(httpRequest.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.IsType<ForgotPasswordRequest>(result);
    }

    [Fact]
    public void GeneratedCode_ShouldReturnFiveDigitCode()
    {
        // Act
        var result = _service.GeneratedCode();

        // Assert
        Assert.NotNull(result);
        Assert.True(int.TryParse(result, out int code));
        Assert.InRange(code, 10000, 99999);
    }

    [Fact]
    public async Task SaveForgotPasswordRequest_ShouldSaveRequest_WhenValidInput()
    {
        // Arrange
        var email = "test@example.com";
        var code = "12345";
        var mockSet = new Mock<DbSet<ForgotPasswordRequestEntity>>();
        _mockContext.Setup(x => x.ForgotPasswordRequests).Returns(mockSet.Object);
        _mockContext.Setup(x => x.ForgotPasswordRequests.Add(It.IsAny<ForgotPasswordRequestEntity>()));
        _mockContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveForgotPasswordRequest(email, code);

        // Assert
        Assert.True(result);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public void GenerateEmailRequest_ShouldReturnEmailRequest_WhenValidInput()
    {
        // Arrange
        var email = "test@example.com";
        var code = "12345";

        // Act
        var result = _service.GenerateEmailRequest(email, code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.To);
        Assert.Contains(code, result.Body);
        Assert.Contains(code, result.PlainText);
    }

    [Fact]
    public void GenerateServiceBusMessage_ShouldReturnSerializedMessage_WhenValidInput()
    {
        // Arrange
        var emailRequest = new EmailRequest { To = "test@example.com", Subject = "Test", Body = "Test Body" };

        // Act
        var result = _service.GenerateServiceBusMessage(emailRequest);

        // Assert
        Assert.NotNull(result);
        var deserialized = JsonConvert.DeserializeObject<EmailRequest>(result);
        Assert.Equal(emailRequest.To, deserialized.To);
    }
}
