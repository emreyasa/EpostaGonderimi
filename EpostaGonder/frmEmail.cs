using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EpostaGonder
{
    public partial class frmEmail : Form
    {
        public frmEmail()
        {
            InitializeComponent();
        }

        private void frmEmail_Load(object sender, EventArgs e)
        {

        }

        private void btnGonder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                MessageBox.Show("Gönderim yapacak e-posta adresini giriniz.", "HATA!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Select();
                return;
            }

            Regex rgx = new Regex("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$");
            if (!rgx.IsMatch(txtEmail.Text))
            {
                MessageBox.Show("Gönderim yapacak e-posta adresi doğru formatta değil.", "HATA!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Select();
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Gönderim yapacak e-posta adresinin şifresini giriniz.", "HATA!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Select();
                return;
            }

            string emailBodyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email-body.txt");
            FileInfo emailBodyFileInfo = new FileInfo(emailBodyFilePath);
            if (!emailBodyFileInfo.Exists)
            {
                MessageBox.Show("Gönderim yapılacak e-posta adresleri bulunamadı.", "HATA!",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string emailListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email-list.txt");
            FileInfo emailListFileInfo = new FileInfo(emailListFilePath);
            if (!emailListFileInfo.Exists)
            {
                MessageBox.Show("Gönderim yapılacak e-posta adresleri bulunamadı.", "HATA!",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder sb = new StringBuilder();
            string[] emailBody = File.ReadAllLines(emailBodyFileInfo.FullName);
            foreach (var text in emailBody)
                sb.Append(text);

            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new NetworkCredential(txtEmail.Text, txtPassword.Text);
            smtp.EnableSsl = true;
            smtp.Port = 587;

            if (rbGmail.Checked)
                smtp.Host = "smtp.gmail.com";

            if (rbHotmail.Checked)
                smtp.Host = "smtp.live.com";

            if (rbYahoo.Checked)
                smtp.Host = "smtp.mail.yahoo.com";

            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = sb.ToString();
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.High;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.SubjectEncoding = Encoding.UTF8;

            string displayName = string.IsNullOrEmpty(txtAdSoyad.Text) ? txtEmail.Text : txtEmail.Text;
            mailMessage.From = new MailAddress(txtEmail.Text, displayName);
            mailMessage.Subject = "About Galatasaray SK Case";

            string[] emailList = File.ReadAllLines(emailListFileInfo.FullName);
            foreach (string email in emailList)
            {
                if (!rgx.IsMatch(email))
                    continue;

                mailMessage.To.Add(new MailAddress(email));

                try
                {
                    smtp.Send(mailMessage);
                }
                catch (SmtpException ex)
                {
                    MessageBox.Show(
                        string.Format("{0} adresine e-posta gönderiminde hata oluştu: {1}", email, ex.Message), 
                        "HATA!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("{0} adresine e-posta gönderiminde hata oluştu: {1}", email, ex.Message),
                        "HATA!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            MessageBox.Show("E-posta gönderimi tamamlandı!", "Başarı!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
