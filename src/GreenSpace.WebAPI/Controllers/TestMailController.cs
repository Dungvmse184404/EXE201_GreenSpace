using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.Runtime.CompilerServices;

[Route("api/[controller]")]
[ApiController]
public class TestMailController : ControllerBase
{
    private readonly IEmailSender _emailService;

    public TestMailController(IEmailSender emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTest(string mail)
    {
        var mailData = new SendEmailDto
        {
            To = mail,
            Subject = "Test Mail GreenSpace",
            Body = "<h1>Xin chào!</h1><p>Đây là mail test từ dự án GreenSpace.</p>"
        };

        try
        {
            await _emailService.SendEmailAsync(mailData);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Lỗi: {ex.Message}");
        }
    }
}