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
                                                Application.Current.MainWindow = window;
                                                break;

                                            case "rescue":
                                                window = new View.rescuedashboard_mainlayout_view(); // this is a UserControl
                                                window.Show();
                                                Application.Current.MainWindow.Close();
                                                Application.Current.MainWindow = window;
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
        public static async Task AddVolunteer(string email, string password, string volunteerName, string contactNumber)
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
        public static async Task AddDonor(string email, string password, string donorName, string contactNumber)
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

        /// <summary>
        /// This method adds a new pledge to the database. It takes the username of the donor, the item name, quantity, unit, expected delivery date, and event ID as parameters. It uses a stored procedure called "usp_AddPledge" to insert the data into the database. If the operation is successful, it shows a message box confirming that the pledge has been submitted. If there is an error, it catches the exception and displays an error message.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="itemName"></param>
        /// <param name="quantity"></param>
        /// <param name="unit"></param>
        /// <param name="expectedDeliveryDate"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public static async Task AddPledge(string username, string itemName, int quantity, string unit, DateTime expectedDeliveryDate, string eventID)
        {
                try
                {
                    using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                    {
                        using (SqlCommand command = new SqlCommand("usp_AddPledge", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
    
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@itemName", itemName);
                            command.Parameters.AddWithValue("@quantity", quantity);
                            command.Parameters.AddWithValue("@unit", unit);
                            command.Parameters.AddWithValue("@expectedDeliveryDate", expectedDeliveryDate);
                            command.Parameters.AddWithValue("@eventID", eventID);
    
                            connection.Open();
                            command.ExecuteNonQuery();
                            MessageBox.Show("Pledge submitted.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// This is suitable for getting table data with simple queries that don't require joins or other complex SQL features.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="data">The retrieved data as a DataTable.</param>
        /// <returns>True if data was retrieved successfully; otherwise, false.</returns>
        public static bool GetTableData(string query, SqlParameter[] parameters, out DataTable data)
        {
            data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters if provided
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(data);
                    }
                }

                return data.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// This is suitable for getting table data with join queries or other complex queries that don't fit the simple GetTableData method.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool GetTableDataWithCustomizedQuery(string query, out DataTable data)
        {
            data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(data);
                        }
                    }
                }
                return data.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

    }
}
