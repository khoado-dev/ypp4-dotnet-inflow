using Inflow.Dtos;
using Inflow.Models;
using Inflow.Repositories.AccountRepo;
using Inflow.Services.AuthService;
using Inflow.Services.EmailService;
using Moq;

namespace InflowTest;

[TestClass]
public class AuthServiceTest
{
    private Mock<IAccountRepository> _repoMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private AuthService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<IAccountRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _service = new AuthService(_repoMock.Object, _emailServiceMock.Object);
    }

    [TestMethod]
    public async Task Register_InvalidEmail_ReturnsError()
    {
        var dto = new RegisterDto { Email = "abc", Password = "Test1234", FirstName = "A" };

        var result = await _service.RegisterAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.InvalidEmail.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task Register_EmailExists_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("test@mail.com"))
            .ReturnsAsync(new Account());

        var dto = new RegisterDto { Email = "test@mail.com", Password = "Test1234", FirstName = "A" };

        var result = await _service.RegisterAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.EmailExists.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task Register_WeakPassword_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("test@mail.com"))
            .ReturnsAsync((Account?)null);

        var dto = new RegisterDto { Email = "test@mail.com", Password = "123", FirstName = "A" };

        var result = await _service.RegisterAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.WeakPassword.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task Register_Success_ReturnsSuccess()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("test@mail.com"))
            .ReturnsAsync((Account?)null);

        var dto = new RegisterDto { Email = "test@mail.com", Password = "Test1234", FirstName = "A" };

        var result = await _service.RegisterAsync(dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuthMessageKey.RegisterSuccess.GetMessage(), result.Message);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Account>()), Times.Once);
    }

    // ========== Login ==========
    [TestMethod]
    public async Task Login_InvalidCredentials_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("wrong@mail.com"))
            .ReturnsAsync((Account?)null);

        var dto = new LoginDto { Email = "wrong@mail.com", Password = "123" };

        var result = await _service.LoginAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.InvalidCredentials.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task Login_Success_ReturnsSuccess()
    {
        var account = new Account
        {
            Email = "test@mail.com",
            PasswordHash = new AuthService(_repoMock.Object, _emailServiceMock.Object)
                            .GetType()
                            .GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                            .Invoke(_service, new object[] { "Test1234" })!.ToString()!
        };

        _repoMock.Setup(r => r.GetByEmailAsync("test@mail.com")).ReturnsAsync(account);

        var dto = new LoginDto { Email = "test@mail.com", Password = "Test1234" };

        var result = await _service.LoginAsync(dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuthMessageKey.LoginSuccess.GetMessage(), result.Message);
    }

    // ========== Forgot Password ==========
    [TestMethod]
    public async Task ForgotPassword_EmailNotFound_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("notfound@mail.com"))
            .ReturnsAsync((Account?)null);

        var dto = new ForgotPasswordDto { Email = "notfound@mail.com" };

        var result = await _service.ForgotPasswordAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.EmailNotFound.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task ForgotPassword_Success_SendsEmail()
    {
        var account = new Account { Email = "test@mail.com" };
        _repoMock.Setup(r => r.GetByEmailAsync("test@mail.com")).ReturnsAsync(account);

        var dto = new ForgotPasswordDto { Email = "test@mail.com" };

        var result = await _service.ForgotPasswordAsync(dto);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.Message, "test@mail.com");
        _emailServiceMock.Verify(e => e.SendEmailAsync("test@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ========== Verify Code ==========
    [TestMethod]
    public async Task VerifyResetCode_Invalid_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByResetCodeAsync("test@mail.com", "000000"))
            .ReturnsAsync((Account?)null);

        var dto = new VerifyResetCodeDto { Email = "test@mail.com", ResetCode = "000000" };

        var result = await _service.VerifyResetCodeAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.InvalidResetCode.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task VerifyResetCode_Success_ReturnsSuccess()
    {
        var account = new Account { Email = "test@mail.com", ResetCode = "123456" };
        _repoMock.Setup(r => r.GetByResetCodeAsync("test@mail.com", "123456")).ReturnsAsync(account);

        var dto = new VerifyResetCodeDto { Email = "test@mail.com", ResetCode = "123456" };

        var result = await _service.VerifyResetCodeAsync(dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuthMessageKey.VerifySuccess.GetMessage(), result.Message);
    }

    // ========== Reset Password ==========
    [TestMethod]
    public async Task ResetPassword_InvalidCode_ReturnsError()
    {
        _repoMock.Setup(r => r.GetByResetCodeAsync("test@mail.com", "000000"))
            .ReturnsAsync((Account?)null);

        var dto = new ResetPasswordDto { Email = "test@mail.com", ResetCode = "000000", NewPassword = "New12345" };

        var result = await _service.ResetPasswordAsync(dto);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuthMessageKey.InvalidResetCode.GetMessage(), result.Message);
    }

    [TestMethod]
    public async Task ResetPassword_Success_ReturnsSuccess()
    {
        var account = new Account { Email = "test@mail.com", ResetCode = "123456" };
        _repoMock.Setup(r => r.GetByResetCodeAsync("test@mail.com", "123456")).ReturnsAsync(account);

        var dto = new ResetPasswordDto { Email = "test@mail.com", ResetCode = "123456", NewPassword = "New12345" };

        var result = await _service.ResetPasswordAsync(dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuthMessageKey.ResetPasswordSuccess.GetMessage(), result.Message);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.PasswordHash != null && a.ResetCode == null)), Times.Once);
    }
}
