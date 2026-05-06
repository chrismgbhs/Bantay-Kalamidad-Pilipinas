using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class DatabaseManager
    {

        /// <summary>
        /// This is a comment.
        /// </summary>
        /// <param name="CurrentUser"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static async Task Login(UserModel CurrentUser, string feature, string role)
        {
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
                                    if (reader.GetString(reader.GetOrdinal("Role")) == role)
                                    {
                                        var userControl = new UserControl();
                                        var window = new Window();

                                        switch (feature)
                                        {
                                            case "admin":
                                                userControl = new View.admin_menu_view(); // this is a UserControl
                                                Application.Current.MainWindow.Content = userControl;
                                                break;

                                            case "donation":
                                                window = new View.donationdashboard_mainlayout_view();
                                                window.Show();
                                                Application.Current.MainWindow.Close();
                                                break;

                                            case "rescue":
                                                window = new View.rescuedashboard_mainlayout_view(); // this is a UserControl
                                                window.Show();
                                                Application.Current.MainWindow.Close();
                                                break;

                                            default:
                                                MessageBox.Show("Invalid role.");
                                                break;
                                        }
                                    }

                                    else
                                    {
                                        //var mainWindow = new View.DonationUser();
                                        //Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
                                        //mainWindow.Show();                           // ✅ Non-blocking
                                        //Application.Current.Windows
                                        //    .OfType<View.DonationLogin>()
                                        //    .FirstOrDefault()?.Close();                 // ✅ Close login after
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

        /// <summary>
        /// Adds a new volunteer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="organization"></param>
        /// <param name="contactNumber"></param>
        public static void AddVolunteer(string email, string password, string volunteerName, string contactNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    using (SqlCommand command = new SqlCommand("usp_AddVolunteer", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@volunteerName", volunteerName);
                        command.Parameters.AddWithValue("@contactNumber", contactNumber);

                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Login now.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="donorName"></param>
        /// <param name="contactNumber"></param>
        public static void AddDonor(string email, string password, string donorName, string contactNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    using (SqlCommand command = new SqlCommand("usp_AddDonor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@emailAddress", email);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@donorName", donorName);
                        command.Parameters.AddWithValue("@contactNumber", contactNumber);

                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Login now.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

    }
}
