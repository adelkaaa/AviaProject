using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows;

namespace Avia
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private bool isMail = true;
        private int verificationCode;
        private string receiverMail;

        public Registration()
        {
            InitializeComponent();
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            backToOwner();
        }

        private void backToOwner()
        {
            //Возврат в окно входа
        }

        private SmtpClient smtpCreate()
        {
            SmtpClient smtpClient = new SmtpClient("smtp.mail.ru");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("testmeilbox0028@mail.ru", "XFehgm1UpW7TWcLwxtmH");
            smtpClient.EnableSsl = true;

            return smtpClient;
        }

        private void verificationGenirate()
        {
            Random random = new Random();
            verificationCode = random.Next(10000, 99999);
        }

        private string passwordGenerator()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                string AllCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                byte[] randomBytes = new byte[12];
                rng.GetBytes(randomBytes);

                return new string(randomBytes.Select(b => AllCharacters[b % AllCharacters.Length]).ToArray());
            }
        }

        private bool checkIfMailRegistered()
        {
            using (SqlConnection connection = new SqlConnection(dbContainer.getAdminString()))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM UsersLogin WHERE Email = @ReceiverMail";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReceiverMail", receiverMail);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Данный адрес электронной почты уже зарегистрирован.");
                        return true;
                    }
                    else
                        return false;
                }
            }

        }

        private bool sendCode()
        {
            receiverMail = mailBox.Text;
            if (checkIfMailRegistered())
                return false;
            verificationGenirate();
            MailMessage mail = new MailMessage("testmeilbox0028@mail.ru", receiverMail);
            mail.Subject = "Код подтверждения";
            mail.Body = $"Ваш код подтверждения: {verificationCode}";

            using (SmtpClient smtp = smtpCreate())
            {
                try
                {
                    smtp.Send(mail);
                    MessageBox.Show("Письмо с кодом успешно отправлено вам на почту!");
                    return true;
                }
                catch (SmtpException ex)
                {
                    MessageBox.Show("Данный адрес электронной почты не зарегистрирован");
                    return false;
                }

            }
        }

        private bool sendPassword()
        {
            if (codeBox.Text == verificationCode.ToString())
            {
                string password = passwordGenerator();
                MailMessage mail = new MailMessage("testmeilbox0028@mail.ru", receiverMail);
                mail.Subject = "Пароль для входа в C++ обучение";
                mail.Body = $"Ваш пароль: {password}";

                using (SmtpClient smtp = smtpCreate())
                {
                    if (enterNewUser(password))
                    {
                        smtp.Send(mail);
                        MessageBox.Show("Письмо с паролем успешно отправлено вам на почту!");
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool enterNewUser(string password)
        {
            string selectCountQuery = "SELECT COUNT(*) FROM UsersInfo";
            string insertLoginQuery = "INSERT INTO UsersLogin (PasswordHash, Email) VALUES (@PasswordHash, @Email)";
            string insertInfoQuery = "INSERT INTO UsersInfo (Email, Nickname) VALUES (@Email, @Nickname)";

            int newUserID;
            try
            {
                using (SqlConnection connection = new SqlConnection(dbContainer.getAdminString()))
                {
                    connection.Open();
                    using (SqlCommand countCommand = new SqlCommand(selectCountQuery, connection))
                    {
                        newUserID = Convert.ToInt32(countCommand.ExecuteScalar()) + 1;
                    }
                }

                using (SqlConnection connection = new SqlConnection(dbContainer.getAdminString()))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string hash = HashPassword(password);
                            using (SqlCommand command = new SqlCommand(insertLoginQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@PasswordHash", hash);
                                command.Parameters.AddWithValue("@Email", receiverMail);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected <= 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            using (SqlCommand command = new SqlCommand(insertInfoQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Email", receiverMail);
                                command.Parameters.AddWithValue("@Nickname", "User_" + newUserID);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected <= 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ServerPinger.PingServer())
            {
                if (isMail)
                {
                    if (sendCode())
                    {
                        mailBox.Visibility = Visibility.Hidden;
                        codeBox.Visibility = Visibility.Visible;
                        textLabel.Content = "Введите код";
                        isMail = false;
                    }
                }
                else
                {
                    if (sendPassword())
                    {
                        backToOwner();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при регистрации нового пользователя. Попробуйте позже");
                        mailBox.Visibility = Visibility.Visible;
                        codeBox.Visibility = Visibility.Hidden;
                        textLabel.Content = "Введите почту";
                        isMail = true;
                        receiverMail = string.Empty;
                        verificationCode = 0;
                    }
                }
            }
            else
            {
                MessageBox.Show("Для прохождения регистрации требуется подключение к интернету!");
            }
        }
    }
}
