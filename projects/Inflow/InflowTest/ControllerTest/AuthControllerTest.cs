using Inflow.Dtos;
using Inflow.Services.AuthService;
using InFlow.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InflowTest;

[TestClass]
public class AuthControllerTests
{
    private Mock<IAuthService> _serviceMock = null!;
    private AuthController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _serviceMock = new Mock<IAuthService>(MockBehavior.Strict);
        _controller = new AuthController(_serviceMock.Object);
    }

    [TestMethod]
    public async Task Register_ReturnsOk_WhenSuccess()
    {
        var dto = new RegisterDto
        {
            Email = "user@example.com",
            FirstName = "Khoa",
            Phone = "0900000000",
            Password = "Abc@1234"
        };
        var resp = new AuthResponseDto { Success = true, Message = AuthMessageKey.RegisterSuccess.GetMessage() };

        _serviceMock.Setup(s => s.RegisterAsync(It.Is<RegisterDto>(x =>
                x.Email == dto.Email &&
                x.FirstName == dto.FirstName &&
                x.Phone == dto.Phone &&
                x.Password == dto.Password)))
            .ReturnsAsync(resp);

        var result = await _controller.Register(dto);

        Assert.IsInstanceOfType<OkObjectResult>(result, out var ok);
        Assert.AreSame(resp, ok.Value);
        _serviceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>()), Times.Once);
    }

    [TestMethod]
    public async Task Register_ReturnsBadRequest_WhenFail()
    {
        var dto = new RegisterDto { Email = "user@example.com", FirstName = "Khoa", Phone = "0900", Password = "weak" };
        var resp = new AuthResponseDto { Success = false, Message = AuthMessageKey.EmailExists.GetMessage() };

        _serviceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync(resp);

        var result = await _controller.Register(dto);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result, out var bad);
        Assert.AreSame(resp, bad.Value);
        _serviceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>()), Times.Once);
    }

    // ========== LOGIN ==========
    [TestMethod]
    public async Task Login_ReturnsOk_WhenSuccess()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Abc@1234" };
        var resp = new AuthResponseDto { Success = true, Message = AuthMessageKey.LoginSuccess.GetMessage(), Token = "jwt" };

        _serviceMock.Setup(s => s.LoginAsync(It.Is<LoginDto>(x => x.Email == dto.Email && x.Password == dto.Password)))
                    .ReturnsAsync(resp);

        var result = await _controller.Login(dto);

        Assert.IsInstanceOfType<OkObjectResult>(result, out var ok);
        Assert.AreSame(resp, ok.Value);
        _serviceMock.Verify(s => s.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
    }

    [TestMethod]
    public async Task Login_ReturnsBadRequest_WhenFail()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "wrong" };
        var resp = new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidCredentials.GetMessage() };

        _serviceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(resp);

        var result = await _controller.Login(dto);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result, out var bad);
        Assert.AreSame(resp, bad.Value);
        _serviceMock.Verify(s => s.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
    }

    // ========== FORGOT PASSWORD ==========
    [TestMethod]
    public async Task ForgotPassword_ReturnsOk_WhenSuccess()
    {
        var dto = new ForgotPasswordDto { Email = "user@example.com" };
        var resp = new AuthResponseDto { Success = true, Message = AuthMessageKey.ResetCodeSent.GetMessage() };

        _serviceMock.Setup(s => s.ForgotPasswordAsync(It.Is<ForgotPasswordDto>(x => x.Email == dto.Email)))
                    .ReturnsAsync(resp);

        var result = await _controller.ForgotPassword(dto);

        Assert.IsInstanceOfType<OkObjectResult>(result, out var ok);
        Assert.AreSame(resp, ok.Value);
        _serviceMock.Verify(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ForgotPassword_ReturnsNotFound_WhenEmailNotFound()
    {
        var dto = new ForgotPasswordDto { Email = "missing@example.com" };
        var resp = new AuthResponseDto { Success = false, Message = AuthMessageKey.EmailNotFound.GetMessage() };

        _serviceMock.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordDto>())).ReturnsAsync(resp);

        var result = await _controller.ForgotPassword(dto);

        Assert.IsInstanceOfType<NotFoundObjectResult>(result, out var nf);
        Assert.AreSame(resp, nf.Value);
        _serviceMock.Verify(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordDto>()), Times.Once);
    }

    // ========== VERIFY RESET CODE ==========
    [TestMethod]
    public async Task VerifyResetCode_ReturnsOk_WhenSuccess()
    {
        var dto = new VerifyResetCodeDto { Email = "user@example.com", ResetCode = "123456" };
        var resp = new AuthResponseDto { Success = true, Message = AuthMessageKey.VerifySuccess.GetMessage() };

        _serviceMock.Setup(s => s.VerifyResetCodeAsync(It.Is<VerifyResetCodeDto>(x => x.Email == dto.Email && x.ResetCode == dto.ResetCode)))
                    .ReturnsAsync(resp);

        var result = await _controller.VerifyResetCode(dto);

        Assert.IsInstanceOfType<OkObjectResult>(result, out var ok);
        Assert.AreSame(resp, ok.Value);
        _serviceMock.Verify(s => s.VerifyResetCodeAsync(It.IsAny<VerifyResetCodeDto>()), Times.Once);
    }

    [TestMethod]
    public async Task VerifyResetCode_ReturnsBadRequest_WhenInvalid()
    {
        var dto = new VerifyResetCodeDto { Email = "user@example.com", ResetCode = "000000" };
        var resp = new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidResetCode.GetMessage() };

        _serviceMock.Setup(s => s.VerifyResetCodeAsync(It.IsAny<VerifyResetCodeDto>())).ReturnsAsync(resp);

        var result = await _controller.VerifyResetCode(dto);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result, out var bad);
        Assert.AreSame(resp, bad.Value);
        _serviceMock.Verify(s => s.VerifyResetCodeAsync(It.IsAny<VerifyResetCodeDto>()), Times.Once);
    }

    // ========== RESET PASSWORD ==========
    [TestMethod]
    public async Task ResetPassword_ReturnsOk_WhenSuccess()
    {
        var dto = new ResetPasswordDto { Email = "user@example.com", NewPassword = "New@12345" };
        var resp = new AuthResponseDto { Success = true, Message = AuthMessageKey.ResetPasswordSuccess.GetMessage() };

        _serviceMock.Setup(s => s.ResetPasswordAsync(It.Is<ResetPasswordDto>(x => x.Email == dto.Email && x.NewPassword == dto.NewPassword)))
                    .ReturnsAsync(resp);

        var result = await _controller.ResetPassword(dto);

        Assert.IsInstanceOfType<OkObjectResult>(result, out var ok);
        Assert.AreSame(resp, ok.Value);
        _serviceMock.Verify(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ResetPassword_ReturnsBadRequest_WhenFail()
    {
        var dto = new ResetPasswordDto { Email = "user@example.com", NewPassword = "short" };
        var resp = new AuthResponseDto { Success = false, Message = AuthMessageKey.WeakPassword.GetMessage() };

        _serviceMock.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>())).ReturnsAsync(resp);

        var result = await _controller.ResetPassword(dto);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result, out var bad);
        Assert.AreSame(resp, bad.Value);
        _serviceMock.Verify(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()), Times.Once);
    }
}