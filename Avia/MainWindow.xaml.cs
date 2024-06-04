using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

namespace Avia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            if (checkIfMailRegistered(mailBox.Text, out string passwordHash, out int userID, out int role))
            {
                if (VerifyPassword(passwordBox.Text, passwordHash))
                {
                    //Открытие новой формы

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Пароль введен неверно!");
                    passwordBox.Text = "";
                }
            }
        }

        private bool checkIfMailRegistered(string receiverMail, out string passwordHash, out int userID, out int role)
        {
            using (SqlConnection connection = new SqlConnection(dbContainer.getAdminString()))
            {
                connection.Open();
                string query = "SELECT UL.PasswordHash, UI.UserID, UI.RoleID FROM UsersLogin UL " +
                               "JOIN UsersInfo UI ON UL.Email = UI.Email " +
                               "WHERE UL.Email = @ReceiverMail";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReceiverMail", receiverMail);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            passwordHash = reader["PasswordHash"].ToString();
                            userID = Convert.ToInt32(reader["UserID"]);
                            role = Convert.ToInt32(reader["RoleID"]);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Данный адрес электронной почты не зарегистрирован.");
                            passwordHash = null;
                            userID = 0;
                            role = 0;
                            return false;
                        }
                    }
                }
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string inputHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return string.Equals(inputHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
            }
        }

        //Работа eyeButton
        /*private void eyeButton_Click(object sender, EventArgs e)
        {
            if (passwordBox.UseSystemPasswordChar == true)
            {
                passwordBox.UseSystemPasswordChar = false;
                eyeButton.BackgroundImage = Resources.openEye;

            }
            else
            {
                passwordBox.UseSystemPasswordChar = true;
                eyeButton.BackgroundImage = Resources.closeEye;
            }
        }*/

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            //октрытие формы восстановления пароля
        }
    }
}
}