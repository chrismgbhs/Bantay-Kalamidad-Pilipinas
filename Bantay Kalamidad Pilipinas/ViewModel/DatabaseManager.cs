using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class DatabaseManager
    {
        public static async Task DonationLogin()
        {
            UserModel CurrentUser = DonationLoginViewModel.CurrentUser;
            MessageBox.Show($"Attempting login with:\nUsername: '{CurrentUser.Username}'\nPassword: '{CurrentUser.Password}'");

            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    string query = $"SELECT * FROM Users WHERE Username = @username AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", CurrentUser.Username);
                        command.Parameters.AddWithValue("@password", CurrentUser.Password);

                        await connection.OpenAsync();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                MessageBox.Show("User found.");

                                while (reader.Read())
                                {
                                    if (reader.GetString(reader.GetOrdinal("Role")) == "admin")
                                    {
                                        CurrentUser.Role = "admin";
                                        var DonationAdmin = new View.DonationAdmin();
                                        Application.Current.MainWindow = DonationAdmin; // ✅ Set BEFORE closing
                                        DonationAdmin.Show();                           // ✅ Non-blocking
                                        Application.Current.Windows
                                            .OfType<View.DonationLogin>()
                                            .FirstOrDefault()?.Close();                 // ✅ Close login after
                                    }

                                    else
                                    {
                                        var mainWindow = new View.DonationUser();
                                        Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
                                        mainWindow.Show();                           // ✅ Non-blocking
                                        Application.Current.Windows
                                            .OfType<View.DonationLogin>()
                                            .FirstOrDefault()?.Close();                 // ✅ Close login after
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("User not found.");
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
                return;
            }
        }
        public static async Task RescueLogin()
        {

            UserModel CurrentUser = RescueLoginViewModel.CurrentUser;
            MessageBox.Show($"Attempting login with:\nUsername: '{CurrentUser.Username}'\nPassword: '{CurrentUser.Password}'");

            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    string query = $"SELECT * FROM Users WHERE Username = @username AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", CurrentUser.Username);
                        command.Parameters.AddWithValue("@password", CurrentUser.Password);

                        await connection.OpenAsync();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                MessageBox.Show("User found.");

                                while (reader.Read())
                                {
                                    if (reader.GetString(reader.GetOrdinal("Role")) == "admin")
                                    {
                                        var mainWindow = new View.RescueAdmin();
                                        Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
                                        mainWindow.Show();                           // ✅ Non-blocking
                                        Application.Current.Windows
                                            .OfType<View.RescueLogin>()
                                            .FirstOrDefault()?.Close();                 // ✅ Close login after
                                    }

                                    else
                                    {
                                        var mainWindow = new View.RescueUser();
                                        Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
                                        mainWindow.Show();                           // ✅ Non-blocking
                                        Application.Current.Windows
                                            .OfType<View.RescueLogin>()
                                            .FirstOrDefault()?.Close();                 // ✅ Close login after
                                    }
                                }
                            }

                            else
                            {
                                MessageBox.Show("User not found.");
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
                return;
            }
        }
    }
}
