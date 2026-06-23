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
using System.Collections.ObjectModel;

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

        public static async Task<UserModel> AuthenticateUserAsync(string username, string password, string requiredRole)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(requiredRole))
            {
                return null;
            }

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT User_ID, Username, Role
            FROM Users
            WHERE Username = @username
              AND Password = @password
              AND Role = @role";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@username", SqlDbType.VarChar, 255).Value = username.Trim();
                    command.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = PasswordHelper.HashPassword(password);
                    command.Parameters.Add("@role", SqlDbType.VarChar, 50).Value = requiredRole;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserModel
                            {
                                UserID = reader.GetInt32(reader.GetOrdinal("User_ID")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Role = reader.GetString(reader.GetOrdinal("Role"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static async Task<ObservableCollection<AdminRescuer>> GetAdminRescuersAsync(string filter, string searchText)
        {
            ObservableCollection<AdminRescuer> rescuers = new ObservableCollection<AdminRescuer>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) ? "All Rescuers" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                v.Volunteer_ID,
                v.Volunteer_Name,
                v.Contact_Number,
                v.User_ID,
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM [Operation Assignment] oa
                        WHERE oa.Volunteer_ID = v.Volunteer_ID
                    ) THEN 'Assigned'
                    WHEN v.User_ID IS NULL THEN 'Inactive'
                    ELSE 'Active'
                END AS Status
            FROM Volunteer v
            WHERE
                (
                    @searchText = '%%'
                    OR v.Volunteer_ID LIKE @searchText
                    OR ISNULL(v.Volunteer_Name, '') LIKE @searchText
                    OR ISNULL(v.Contact_Number, '') LIKE @searchText
                    OR CAST(ISNULL(v.User_ID, '') AS VARCHAR(20)) LIKE @searchText
                )
                AND
                (
                    @filter = 'All Rescuers'
                    OR (@filter = 'Active' AND v.User_ID IS NOT NULL)
                    OR (@filter = 'Inactive' AND v.User_ID IS NULL)
                    OR (@filter = 'Assigned' AND EXISTS (
                        SELECT 1
                        FROM [Operation Assignment] oa
                        WHERE oa.Volunteer_ID = v.Volunteer_ID
                    ))
                )
            ORDER BY v.Volunteer_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rescuers.Add(new AdminRescuer
                            {
                                RescuerId = reader["Volunteer_ID"] == DBNull.Value ? string.Empty : reader["Volunteer_ID"].ToString(),
                                Name = reader["Volunteer_Name"] == DBNull.Value ? string.Empty : reader["Volunteer_Name"].ToString(),
                                ContactNumber = reader["Contact_Number"] == DBNull.Value ? string.Empty : reader["Contact_Number"].ToString(),
                                UserId = reader["User_ID"] == DBNull.Value ? string.Empty : reader["User_ID"].ToString(),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return rescuers;
        }

        public static async Task AddAdminRescuerAsync(AdminRescuer rescuer)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            INSERT INTO Volunteer
                (Volunteer_ID, Volunteer_Name, Organization, Contact_Number, User_ID)
            VALUES
                (@volunteerId, @volunteerName, @organization, @contactNumber, @userId);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@volunteerId", SqlDbType.VarChar, 10).Value = rescuer.RescuerId.Trim();
                    command.Parameters.Add("@volunteerName", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(rescuer.Name) ? (object)DBNull.Value : rescuer.Name.Trim();

                    command.Parameters.Add("@organization", SqlDbType.VarChar, 255).Value = DBNull.Value;

                    command.Parameters.Add("@contactNumber", SqlDbType.VarChar, 50).Value =
                        string.IsNullOrWhiteSpace(rescuer.ContactNumber) ? (object)DBNull.Value : rescuer.ContactNumber.Trim();

                    command.Parameters.Add("@userId", SqlDbType.Int).Value =
                        string.IsNullOrWhiteSpace(rescuer.UserId) ? (object)DBNull.Value : int.Parse(rescuer.UserId.Trim());

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminRescuerAsync(AdminRescuer rescuer)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            UPDATE Volunteer
            SET
                Volunteer_Name = @volunteerName,
                Contact_Number = @contactNumber,
                User_ID = @userId
            WHERE Volunteer_ID = @volunteerId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@volunteerId", SqlDbType.VarChar, 10).Value = rescuer.RescuerId.Trim();

                    command.Parameters.Add("@volunteerName", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(rescuer.Name) ? (object)DBNull.Value : rescuer.Name.Trim();

                    command.Parameters.Add("@contactNumber", SqlDbType.VarChar, 50).Value =
                        string.IsNullOrWhiteSpace(rescuer.ContactNumber) ? (object)DBNull.Value : rescuer.ContactNumber.Trim();

                    command.Parameters.Add("@userId", SqlDbType.Int).Value =
                        string.IsNullOrWhiteSpace(rescuer.UserId) ? (object)DBNull.Value : int.Parse(rescuer.UserId.Trim());

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No rescuer record was updated. The rescuer may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminRescuerAsync(string rescuerId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM Volunteer
            WHERE Volunteer_ID = @volunteerId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@volunteerId", SqlDbType.VarChar, 10).Value = rescuerId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No rescuer record was deleted. The rescuer may no longer exist.");
                    }
                }
            }
        }


        public static async Task<ObservableCollection<AdminDisasterEvent>> GetAdminDisasterEventsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminDisasterEvent> disasterEvents = new ObservableCollection<AdminDisasterEvent>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) ? "All Events" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT
                de.Event_ID,
                de.Event_Name,
                de.Start_Date,
                de.End_Date,
                CASE
                    WHEN de.Start_Date > CAST(GETDATE() AS DATE) THEN 'Upcoming'
                    WHEN de.End_Date IS NOT NULL AND de.End_Date < CAST(GETDATE() AS DATE) THEN 'Completed'
                    ELSE 'Active'
                END AS Status
            FROM [Disaster Event] de
            WHERE
                (
                    @searchText = '%%'
                    OR de.Event_ID LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
                    OR CONVERT(VARCHAR(10), de.Start_Date, 120) LIKE @searchText
                    OR CONVERT(VARCHAR(10), de.End_Date, 120) LIKE @searchText
                )
                AND
                (
                    @filter = 'All Events'
                    OR (@filter = 'Upcoming' AND de.Start_Date > CAST(GETDATE() AS DATE))
                    OR (@filter = 'Completed' AND de.End_Date IS NOT NULL AND de.End_Date < CAST(GETDATE() AS DATE))
                    OR (@filter = 'Active' AND de.Start_Date <= CAST(GETDATE() AS DATE) AND (de.End_Date IS NULL OR de.End_Date >= CAST(GETDATE() AS DATE)))
                )
            ORDER BY de.Start_Date DESC, de.Event_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            disasterEvents.Add(new AdminDisasterEvent
                            {
                                EventId = reader["Event_ID"] == DBNull.Value ? string.Empty : reader["Event_ID"].ToString(),
                                EventName = reader["Event_Name"] == DBNull.Value ? string.Empty : reader["Event_Name"].ToString(),
                                StartDate = reader["Start_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Start_Date"]),
                                EndDate = reader["End_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["End_Date"]),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return disasterEvents;
        }

        public static async Task AddAdminDisasterEventAsync(AdminDisasterEvent disasterEvent)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            INSERT INTO [Disaster Event]
                (Event_ID, Event_Name, Start_Date, End_Date)
            VALUES
                (@eventId, @eventName, @startDate, @endDate);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = disasterEvent.EventId.Trim();
                    command.Parameters.Add("@eventName", SqlDbType.VarChar, 255).Value = disasterEvent.EventName.Trim();
                    command.Parameters.Add("@startDate", SqlDbType.Date).Value = disasterEvent.StartDate.Value.Date;
                    command.Parameters.Add("@endDate", SqlDbType.Date).Value =
                        disasterEvent.EndDate == null ? (object)DBNull.Value : disasterEvent.EndDate.Value.Date;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminDisasterEventAsync(AdminDisasterEvent disasterEvent)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            UPDATE [Disaster Event]
            SET
                Event_Name = @eventName,
                Start_Date = @startDate,
                End_Date = @endDate
            WHERE Event_ID = @eventId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = disasterEvent.EventId.Trim();
                    command.Parameters.Add("@eventName", SqlDbType.VarChar, 255).Value = disasterEvent.EventName.Trim();
                    command.Parameters.Add("@startDate", SqlDbType.Date).Value = disasterEvent.StartDate.Value.Date;
                    command.Parameters.Add("@endDate", SqlDbType.Date).Value =
                        disasterEvent.EndDate == null ? (object)DBNull.Value : disasterEvent.EndDate.Value.Date;

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No disaster event record was updated. The event may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminDisasterEventAsync(string eventId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Disaster Event]
            WHERE Event_ID = @eventId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = eventId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No disaster event record was deleted. The event may no longer exist.");
                    }
                }
            }
        }


        public static async Task<ObservableCollection<AdminRescueOperation>> GetAdminRescueOperationsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminRescueOperation> operations = new ObservableCollection<AdminRescueOperation>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) ? "All Operations" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT
                ro.Operation_ID,
                ro.Event_ID,
                ro.Location_ID,
                ro.Date_Started,
                ro.Rescue_Status
            FROM [Rescue Operation] ro
            LEFT JOIN [Disaster Event] de
                ON ro.Event_ID = de.Event_ID
            LEFT JOIN Location l
                ON ro.Location_ID = l.Location_ID
            WHERE
                (
                    @searchText = '%%'
                    OR ro.Operation_ID LIKE @searchText
                    OR ro.Event_ID LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
                    OR ro.Location_ID LIKE @searchText
                    OR ISNULL(l.Barangay, '') LIKE @searchText
                    OR ISNULL(l.City, '') LIKE @searchText
                    OR ISNULL(l.Province, '') LIKE @searchText
                    OR CONVERT(VARCHAR(10), ro.Date_Started, 120) LIKE @searchText
                    OR ISNULL(ro.Rescue_Status, '') LIKE @searchText
                )
                AND
                (
                    @filter = 'All Operations'
                    OR ro.Rescue_Status = @filter
                )
            ORDER BY ro.Date_Started DESC, ro.Operation_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            operations.Add(new AdminRescueOperation
                            {
                                OperationId = reader["Operation_ID"] == DBNull.Value ? string.Empty : reader["Operation_ID"].ToString(),
                                Event = reader["Event_ID"] == DBNull.Value ? string.Empty : reader["Event_ID"].ToString(),
                                Location = reader["Location_ID"] == DBNull.Value ? string.Empty : reader["Location_ID"].ToString(),
                                DateStarted = reader["Date_Started"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date_Started"]),
                                Status = reader["Rescue_Status"] == DBNull.Value ? string.Empty : reader["Rescue_Status"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return operations;
        }

        public static async Task AddAdminRescueOperationAsync(AdminRescueOperation operation)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            INSERT INTO [Rescue Operation]
                (Operation_ID, Event_ID, Location_ID, Date_Started, Rescue_Status)
            VALUES
                (@operationId, @eventId, @locationId, @dateStarted, @rescueStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = operation.OperationId.Trim();
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = operation.Event.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = operation.Location.Trim();
                    command.Parameters.Add("@dateStarted", SqlDbType.Date).Value = operation.DateStarted.Value.Date;
                    command.Parameters.Add("@rescueStatus", SqlDbType.VarChar, 255).Value = operation.Status.Trim();

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminRescueOperationAsync(AdminRescueOperation operation)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            UPDATE [Rescue Operation]
            SET
                Event_ID = @eventId,
                Location_ID = @locationId,
                Date_Started = @dateStarted,
                Rescue_Status = @rescueStatus
            WHERE Operation_ID = @operationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = operation.OperationId.Trim();
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = operation.Event.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = operation.Location.Trim();
                    command.Parameters.Add("@dateStarted", SqlDbType.Date).Value = operation.DateStarted.Value.Date;
                    command.Parameters.Add("@rescueStatus", SqlDbType.VarChar, 255).Value = operation.Status.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No rescue operation record was updated. The operation may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminRescueOperationAsync(string operationId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Rescue Operation]
            WHERE Operation_ID = @operationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = operationId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No rescue operation record was deleted. The operation may no longer exist.");
                    }
                }
            }
        }


        public static async Task<ObservableCollection<AdminOperationAssignment>> GetAdminOperationAssignmentsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminOperationAssignment> assignments = new ObservableCollection<AdminOperationAssignment>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Assignments" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT
                oa.Assignment_ID,
                oa.Operation_ID,
                oa.Volunteer_ID,
                v.Volunteer_Name,
                oa.Role,
                oa.Operation_Status
            FROM [Operation Assignment] oa
            LEFT JOIN Volunteer v
                ON oa.Volunteer_ID = v.Volunteer_ID
            WHERE
                (
                    @searchText = '%%'
                    OR oa.Assignment_ID LIKE @searchText
                    OR oa.Operation_ID LIKE @searchText
                    OR oa.Volunteer_ID LIKE @searchText
                    OR ISNULL(v.Volunteer_Name, '') LIKE @searchText
                    OR ISNULL(oa.Role, '') LIKE @searchText
                    OR ISNULL(oa.Operation_Status, '') LIKE @searchText
                )
                AND
                (
                    @filter = 'All Assignments'
                    OR oa.Operation_Status = @filter
                )
            ORDER BY oa.Assignment_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string volunteerId = reader["Volunteer_ID"] == DBNull.Value ? string.Empty : reader["Volunteer_ID"].ToString();
                            string volunteerName = reader["Volunteer_Name"] == DBNull.Value ? string.Empty : reader["Volunteer_Name"].ToString();

                            assignments.Add(new AdminOperationAssignment
                            {
                                AssignmentId = reader["Assignment_ID"] == DBNull.Value ? string.Empty : reader["Assignment_ID"].ToString(),
                                Operation = reader["Operation_ID"] == DBNull.Value ? string.Empty : reader["Operation_ID"].ToString(),
                                VolunteerId = volunteerId,
                                RescuerName = string.IsNullOrWhiteSpace(volunteerName) ? volunteerId : volunteerName,
                                Role = reader["Role"] == DBNull.Value ? string.Empty : reader["Role"].ToString(),
                                Status = reader["Operation_Status"] == DBNull.Value ? string.Empty : reader["Operation_Status"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return assignments;
        }

        public static async Task AddAdminOperationAssignmentAsync(AdminOperationAssignment assignment)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string volunteerId = await ResolveVolunteerIdAsync(connection, assignment.RescuerName);

                string query = @"
            INSERT INTO [Operation Assignment]
                (Assignment_ID, Operation_ID, Volunteer_ID, Role, Operation_Status)
            VALUES
                (@assignmentId, @operationId, @volunteerId, @role, @operationStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@assignmentId", SqlDbType.VarChar, 10).Value = assignment.AssignmentId.Trim();
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = assignment.Operation.Trim();
                    command.Parameters.Add("@volunteerId", SqlDbType.VarChar, 10).Value = volunteerId;
                    command.Parameters.Add("@role", SqlDbType.VarChar, 255).Value = assignment.Role.Trim();
                    command.Parameters.Add("@operationStatus", SqlDbType.VarChar, 255).Value = assignment.Status.Trim();

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminOperationAssignmentAsync(AdminOperationAssignment assignment)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string volunteerId = await ResolveVolunteerIdAsync(connection, assignment.RescuerName);

                string query = @"
            UPDATE [Operation Assignment]
            SET
                Operation_ID = @operationId,
                Volunteer_ID = @volunteerId,
                Role = @role,
                Operation_Status = @operationStatus
            WHERE Assignment_ID = @assignmentId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@assignmentId", SqlDbType.VarChar, 10).Value = assignment.AssignmentId.Trim();
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = assignment.Operation.Trim();
                    command.Parameters.Add("@volunteerId", SqlDbType.VarChar, 10).Value = volunteerId;
                    command.Parameters.Add("@role", SqlDbType.VarChar, 255).Value = assignment.Role.Trim();
                    command.Parameters.Add("@operationStatus", SqlDbType.VarChar, 255).Value = assignment.Status.Trim();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No assignment record was updated. The assignment may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminOperationAssignmentAsync(string assignmentId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Operation Assignment]
            WHERE Assignment_ID = @assignmentId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@assignmentId", SqlDbType.VarChar, 10).Value = assignmentId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No assignment record was deleted. The assignment may no longer exist.");
                    }
                }
            }
        }

        private static async Task<string> ResolveVolunteerIdAsync(SqlConnection connection, string rescuerInput)
        {
            string input = rescuerInput == null ? string.Empty : rescuerInput.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Rescuer name or Volunteer ID is required.");
            }

            string query = @"
        SELECT TOP 1 Volunteer_ID
        FROM Volunteer
        WHERE Volunteer_ID = @rescuerInput
           OR Volunteer_Name = @rescuerInput
        ORDER BY 
            CASE WHEN Volunteer_ID = @rescuerInput THEN 0 ELSE 1 END;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@rescuerInput", SqlDbType.VarChar, 255).Value = input;

                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("The rescuer was not found. Enter an existing Volunteer ID or exact Volunteer Name.");
                }

                return result.ToString();
            }
        }

        public static async Task<ObservableCollection<AdminRescueTeam>> GetAdminRescueTeamsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminRescueTeam> teams = new ObservableCollection<AdminRescueTeam>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Teams" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT
                ro.Operation_ID,
                ro.Event_ID,
                ro.Location_ID,
                COUNT(oa.Assignment_ID) AS Members,
                ro.Rescue_Status
            FROM [Rescue Operation] ro
            LEFT JOIN [Operation Assignment] oa
                ON ro.Operation_ID = oa.Operation_ID
            LEFT JOIN [Disaster Event] de
                ON ro.Event_ID = de.Event_ID
            LEFT JOIN Location l
                ON ro.Location_ID = l.Location_ID
            WHERE
                (
                    @searchText = '%%'
                    OR ro.Operation_ID LIKE @searchText
                    OR ro.Event_ID LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
                    OR ro.Location_ID LIKE @searchText
                    OR ISNULL(l.Barangay, '') LIKE @searchText
                    OR ISNULL(l.City, '') LIKE @searchText
                    OR ISNULL(l.Province, '') LIKE @searchText
                    OR ISNULL(ro.Rescue_Status, '') LIKE @searchText
                )
                AND
                (
                    @filter = 'All Teams'
                    OR ro.Rescue_Status = @filter
                )
            GROUP BY
                ro.Operation_ID,
                ro.Event_ID,
                ro.Location_ID,
                ro.Rescue_Status
            ORDER BY ro.Operation_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            teams.Add(new AdminRescueTeam
                            {
                                TeamOperation = reader["Operation_ID"] == DBNull.Value ? string.Empty : reader["Operation_ID"].ToString(),
                                Event = reader["Event_ID"] == DBNull.Value ? string.Empty : reader["Event_ID"].ToString(),
                                Location = reader["Location_ID"] == DBNull.Value ? string.Empty : reader["Location_ID"].ToString(),
                                Members = reader["Members"] == DBNull.Value ? "0" : reader["Members"].ToString(),
                                Status = reader["Rescue_Status"] == DBNull.Value ? string.Empty : reader["Rescue_Status"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return teams;
        }

        public static async Task AddAdminRescueTeamAsync(AdminRescueTeam team)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            INSERT INTO [Rescue Operation]
                (Operation_ID, Event_ID, Location_ID, Date_Started, Rescue_Status)
            VALUES
                (@operationId, @eventId, @locationId, CAST(GETDATE() AS DATE), @rescueStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = team.TeamOperation.Trim();
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = team.Event.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = team.Location.Trim();
                    command.Parameters.Add("@rescueStatus", SqlDbType.VarChar, 255).Value = team.Status.Trim();

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminRescueTeamAsync(AdminRescueTeam team)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            UPDATE [Rescue Operation]
            SET
                Event_ID = @eventId,
                Location_ID = @locationId,
                Rescue_Status = @rescueStatus
            WHERE Operation_ID = @operationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = team.TeamOperation.Trim();
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = team.Event.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = team.Location.Trim();
                    command.Parameters.Add("@rescueStatus", SqlDbType.VarChar, 255).Value = team.Status.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No team/operation record was updated. It may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminRescueTeamAsync(string operationId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Rescue Operation]
            WHERE Operation_ID = @operationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@operationId", SqlDbType.VarChar, 10).Value = operationId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No team/operation record was deleted. It may no longer exist.");
                    }
                }
            }
        }

    }
}