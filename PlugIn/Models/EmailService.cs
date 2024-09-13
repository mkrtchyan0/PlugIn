using System.Net.Mail;
using System.Net;

namespace PlugIn.Models
{
	public class EmailService
	{
		private readonly string _smtpServer;
		private readonly int _smtpPort;
		private readonly string _smtpUser;
		private readonly string _smtpPass;
		public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPass)
		{
			_smtpServer = smtpServer;
			_smtpPort = smtpPort;
			_smtpUser = smtpUser;
			_smtpPass = smtpPass;
		}
		public async Task<bool> SendEmailAsync(string toEmail, string body)
		{
			using (var client = new SmtpClient(_smtpServer, _smtpPort))
			{
				client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
				client.EnableSsl = true;

				var mailMessage = new MailMessage
				{
					From = new MailAddress(_smtpUser),
					Subject = "Confirm email",
					Body = body,
					IsBodyHtml = true
				};
				mailMessage.To.Add(toEmail);

				try
				{
					await client.SendMailAsync(mailMessage);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending email: {ex.Message}");
					return false;
				}
			}
			return true;
		}
	}
}
