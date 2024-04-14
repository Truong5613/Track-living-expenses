// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using DoAnLapTrinhWeb.Service;

namespace DoAnLapTrinhWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<AppliactionUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ISendMailService _sendMailService;
        public ForgotPasswordModel(UserManager<AppliactionUser> userManager, IEmailSender emailSender, ISendMailService sendMailService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _sendMailService = sendMailService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                //await _emailSender.SendEmailAsync(
                  //  Input.Email,
                   // "Reset Password",
                   // $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                MailContent content = new MailContent
                {
                    To = user.Email,
                    Subject = "Xác thực tài khoản",
                    Body = $"<p>Xin chào,</p>\r\n    <p>Chúng tôi nhận được yêu cầu thay đổi mật khẩu cho tài khoản của bạn tại Track_Living_Expenses. Để hoàn tất quá trình này, bạn cần xác thực địa chỉ email của mình.</p>\r\n    <p>Vui lòng nhấp vào liên kết dưới đây để xác thực email của bạn và tiếp tục quá trình thay đổi mật khẩu:</p>\r\n    <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click here to verify</a></p>\r\n    <p>Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.</p>\r\n    <p>Trân trọng,</p>\r\n    <p>Đội ngũ hỗ trợ Track_Living_Expense</p>",

                };


                await _sendMailService.SendMail(content);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
