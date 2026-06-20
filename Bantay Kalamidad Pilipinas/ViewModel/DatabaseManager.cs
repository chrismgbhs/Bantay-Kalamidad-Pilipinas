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
            // Guard: empty credentials
            if (string.IsNullOrWhiteSpace(CurrentUser.Username) || string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Please enter your username and password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    string query = "SELECT * FROM Users WHERE Username = @username AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", CurrentUser.Username);
                        command.Parameters.AddWithValue("@password", PasswordHelper.HashPassword(CurrentUser.Password));

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    string dbRole = reader.GetString(reader.GetOrdinal("Role"));

                                    if (dbRole == role)
                                    {
                                        // Populate CurrentUser with data from the database row
                                        CurrentUser.Username = reader.GetString(reader.GetOrdinal("Username"));
                                        CurrentUser.Role = dbRole;

                                        // Navigate based on feature — must be dispatched on UI thread
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            switch (feature)
                                            {
                                                case "admin":
                                                    Application.Current.MainWindow.Content = new View.admin_menu_view();
                                                    break;

                                                case "donation":
                                                    var donationWindow = new View.donationdashboard_mainlayout_view();
                                                    donationWindow.Show();
                                                    Application.Current.MainWindow.Close();
                                                    Application.Current.MainWindow = donationWindow;
                                                    break;

                                                case "rescue":
                                                    var rescueWindow = new View.rescuedashboard_mainlayout_view();
                                                    rescueWindow.Show();
                                                    Application.Current.MainWindow.Close();
                                                    Application.Current.MainWindow = rescueWindow;
                                                    break;

                                                default:
                                                    MessageBox.Show("Unknown feature. Contact support.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                    break;
                                            }
                                        });
                                    }
                                    else
                                    {
                                        MessageBox.Show(
                                            $"This account is not registered as a {role}. Please use the correct login portal.",
                                            "Wrong Portal",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Incorrect username or password. Please try again.",
                                    "Login Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your connection and try again.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Logs a user in via a verified Google account (email + display
        /// name already confirmed by Google — see GoogleAuthHelper). If no
        /// Users row exists for that email yet, one is created automatically
        /// (as a Donor or Volunteer depending on which login screen the
        /// button was clicked from), matching the auto-create behavior
        /// decided for this feature. If a row already exists, this just logs
        /// them straight in — same navigation behavior as the password path.
        ///
        /// Google-authenticated accounts never have a usable BKP password:
        /// a long cryptographically random value is hashed and stored in
        /// place of one, purely to satisfy the NOT NULL constraint on
        /// Users.Password. It is never shown to the user and the password
        /// login form will never produce a matching hash for it, so the
        /// account can only ever be accessed via Google sign-in.
        /// </summary>
        public static async Task LoginWithGoogle(string email, string displayName, string feature, string role)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Google did not return an email address for this account.", "Google Sign-In Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool userExists;
                string existingRole = null;

                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    string query = "SELECT * FROM Users WHERE Username = @username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", email);

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            userExists = await reader.ReadAsync();
                            if (userExists)
                            {
                                existingRole = reader.GetString(reader.GetOrdinal("Role"));
                            }
                        }
                    }
                }

                if (userExists)
                {
                    if (existingRole != role)
                    {
                        MessageBox.Show(
                            $"This Google account is already registered as a {existingRole}. Please use the correct sign-in portal.",
                            "Wrong Portal",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    // Existing account, correct role — fall through to navigation below.
                }
                else
                {
                    // First time this Google account has signed in here — auto-create.
                    // showSuccessMessage: false suppresses the normal "Account created!"
                    // popup since we show a Google-specific welcome message instead, below.
                    string randomPlaceholderPassword = GenerateUnguessablePlaceholder();

                    if (feature == "donation")
                    {
                        await AddDonor(email, randomPlaceholderPassword, displayName, contactNumber: null, showSuccessMessage: false);
                    }
                    else if (feature == "rescue")
                    {
                        await AddVolunteer(email, randomPlaceholderPassword, displayName, contactNumber: null, showSuccessMessage: false);
                    }

                    MessageBox.Show(
                        $"Welcome, {displayName}! Your account has been created using your Google sign-in.",
                        "Welcome to Bantay Kalamidad Pilipinas",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                // Navigate the same way the password-based Login() does.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (feature)
                    {
                        case "donation":
                            var donationWindow = new View.donationdashboard_mainlayout_view();
                            donationWindow.Show();
                            Application.Current.MainWindow.Close();
                            Application.Current.MainWindow = donationWindow;
                            break;

                        case "rescue":
                            var rescueWindow = new View.rescuedashboard_mainlayout_view();
                            rescueWindow.Show();
                            Application.Current.MainWindow.Close();
                            Application.Current.MainWindow = rescueWindow;
                            break;
                    }
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your connection and try again.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Generates a long, cryptographically random string to use as a
        /// placeholder password hash input for Google-authenticated
        /// accounts. Never shown to the user, never typed by anyone — it
        /// only exists to satisfy Users.Password's NOT NULL constraint
        /// while guaranteeing the password-login form can never match it.
        /// </summary>
        private static string GenerateUnguessablePlaceholder()
        {
            byte[] bytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }


        /// <summary>
        /// Adds a new volunteer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="organization"></param>
        /// <param name="contactNumber"></param>
        /// <param name="showSuccessMessage">
        /// Pass false when this is called as part of a flow that shows its
        /// own success message (e.g. Google sign-in's one-time welcome
        /// message) instead of the standard "Account created!" popup.
        /// </param>
        public static async Task AddVolunteer(string email, string password, string volunteerName, string contactNumber, bool showSuccessMessage = true)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    using (SqlCommand command = new SqlCommand("usp_AddVolunteer", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@password", PasswordHelper.HashPassword(password));
                        command.Parameters.AddWithValue("@volunteerName", volunteerName);
                        command.Parameters.AddWithValue("@contactNumber", (object)contactNumber ?? DBNull.Value);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        if (showSuccessMessage)
                        {
                            MessageBox.Show(
                                "Account created! You may now log in.",
                                "Registration Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (SqlException sqlex) when (sqlex.Message.Contains("already registered"))
            {
                // usp_AddVolunteer RAISERRORs when the email/username is already taken
                MessageBox.Show(
                    "An account with this email already exists. Please use a different email.",
                    "Registration Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your connection and try again.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Registers a new donor account. Calls usp_AddDonor, which hashes
        /// nothing itself (hashing happens here) and raises a custom error
        /// if the email/username is already registered.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="donorName"></param>
        /// <param name="contactNumber"></param>
        /// <param name="showSuccessMessage">
        /// Pass false when this is called as part of a flow that shows its
        /// own success message (e.g. Google sign-in's one-time welcome
        /// message) instead of the standard "Account created!" popup.
        /// </param>
        public static async Task AddDonor(string email, string password, string donorName, string contactNumber, bool showSuccessMessage = true)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                {
                    using (SqlCommand command = new SqlCommand("usp_AddDonor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@emailAddress", email);
                        command.Parameters.AddWithValue("@password", PasswordHelper.HashPassword(password));
                        command.Parameters.AddWithValue("@donorName", donorName);
                        command.Parameters.AddWithValue("@contactNumber", (object)contactNumber ?? DBNull.Value);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        if (showSuccessMessage)
                        {
                            MessageBox.Show(
                                "Account created! You may now log in.",
                                "Registration Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (SqlException sqlex) when (sqlex.Message.Contains("already registered"))
            {
                // usp_AddDonor RAISERRORs when the email/username is already taken
                MessageBox.Show(
                    "An account with this email already exists. Please use a different email.",
                    "Registration Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your connection and try again.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        MessageBox.Show(
                            "Your pledge has been submitted successfully.",
                            "Pledge Submitted",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your connection and try again.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (SqlException sqlex)
            {
                Console.WriteLine($"SQL Error [{sqlex.Number}]: {sqlex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

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
            catch (SqlException sqlex)
            {
                Console.WriteLine($"SQL Error [{sqlex.Number}]: {sqlex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

    }
}