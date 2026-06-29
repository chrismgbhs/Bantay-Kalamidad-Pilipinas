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
                    @searchText = '%'
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
                    @searchText = '%'
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
                ISNULL(de.Event_Name, ro.Event_ID) AS EventDisplay,
                ro.Location_ID,
                ISNULL(l.Barangay + ', ' + l.City, ro.Location_ID) AS LocationDisplay,
                ro.Date_Started,
                ro.Rescue_Status
            FROM [Rescue Operation] ro
            LEFT JOIN [Disaster Event] de
                ON ro.Event_ID = de.Event_ID
            LEFT JOIN Location l
                ON ro.Location_ID = l.Location_ID
            WHERE
                (
                    @searchText = '%'
                    OR ro.Operation_ID LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
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
                                EventId = reader["Event_ID"] == DBNull.Value ? string.Empty : reader["Event_ID"].ToString(),
                                Event = reader["EventDisplay"] == DBNull.Value ? string.Empty : reader["EventDisplay"].ToString(),
                                LocationId = reader["Location_ID"] == DBNull.Value ? string.Empty : reader["Location_ID"].ToString(),
                                Location = reader["LocationDisplay"] == DBNull.Value ? string.Empty : reader["LocationDisplay"].ToString(),
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
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = operation.EventId.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = operation.LocationId.Trim();
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
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = operation.EventId.Trim();
                    command.Parameters.Add("@locationId", SqlDbType.VarChar, 10).Value = operation.LocationId.Trim();
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
                    @searchText = '%'
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
                ISNULL(de.Event_Name, ro.Event_ID) AS EventDisplay,
                ISNULL(l.Barangay + ', ' + l.City, ro.Location_ID) AS LocationDisplay,
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
                    @searchText = '%'
                    OR ro.Operation_ID LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
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
                de.Event_Name,
                l.Barangay,
                l.City,
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
                                Event = reader["EventDisplay"] == DBNull.Value ? string.Empty : reader["EventDisplay"].ToString(),
                                Location = reader["LocationDisplay"] == DBNull.Value ? string.Empty : reader["LocationDisplay"].ToString(),
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

        public static async Task<ObservableCollection<AdminDonations>> GetAdminDonationsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminDonations> donations = new ObservableCollection<AdminDonations>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Donations" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                d.Donation_ID,
                do.Donor_Name,
                ISNULL(de.Event_Name, '') AS Event_Name,
                d.Date_Received,
                CASE
                    WHEN d.Date_Received IS NULL THEN 'Pending'
                    WHEN d.Event_ID IS NULL THEN 'Received'
                    ELSE 'Completed'
                END AS Status
            FROM Donation d
            INNER JOIN Donor do ON d.Donor_ID = do.Donor_ID
            LEFT JOIN [Disaster Event] de ON d.Event_ID = de.Event_ID
            WHERE
                (
                    @searchText = '%'
                    OR d.Donation_ID LIKE @searchText
                    OR ISNULL(do.Donor_Name, '') LIKE @searchText
                    OR ISNULL(de.Event_Name, '') LIKE @searchText
                    OR CONVERT(VARCHAR(10), d.Date_Received, 120) LIKE @searchText
                )
                AND
                (
                    @filter = 'All Donations'
                    OR (@filter = 'Pending'   AND d.Date_Received IS NULL)
                    OR (@filter = 'Received'  AND d.Date_Received IS NOT NULL AND d.Event_ID IS NULL)
                    OR (@filter = 'Completed' AND d.Date_Received IS NOT NULL AND d.Event_ID IS NOT NULL)
                )
            ORDER BY d.Donation_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            donations.Add(new AdminDonations
                            {
                                DonationId = reader["Donation_ID"] == DBNull.Value ? string.Empty : reader["Donation_ID"].ToString(),
                                Donor = reader["Donor_Name"] == DBNull.Value ? string.Empty : reader["Donor_Name"].ToString(),
                                Event = reader["Event_Name"] == DBNull.Value ? string.Empty : reader["Event_Name"].ToString(),
                                DateReceived = reader["Date_Received"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date_Received"]),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString(),
                            });
                        }
                    }
                }
            }


            return donations;
        }

        private static async Task<string> ResolveDonorIdAsync(SqlConnection connection, string donorInput)
        {
            string input = donorInput == null ? string.Empty : donorInput.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Donor name or Donor ID is required.");
            }

            string query = @"
            SELECT TOP 1 Donor_ID
            FROM Donor
            WHERE Donor_ID = @donorInput
               OR Donor_Name = @donorInput
            ORDER BY 
                CASE WHEN Donor_ID = @donorInput THEN 0 ELSE 1 END;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@donorInput", SqlDbType.VarChar, 255).Value = input;

                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("The donor was not found. Enter an existing Donor ID or exact Donor Name.");
                }

                return result.ToString();
            }
        }

        public static async Task AddAdminDonationsAsync(AdminDonations donations)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donorId = await ResolveDonorIdAsync(connection, donations.Donor);

                string query = @"
            INSERT INTO Donation
                (Donation_ID, Donor_ID, Event_ID, Date_Received)
            VALUES
                (@donationId, @donorId, @eventId, @dateReceived);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donations.DonationId.Trim();
                    command.Parameters.Add("@donorId", SqlDbType.VarChar, 10).Value = donorId;
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value =
                        string.IsNullOrWhiteSpace(donations.EventId) ? (object)DBNull.Value : donations.EventId.Trim();
                    command.Parameters.Add("@dateReceived", SqlDbType.DateTime).Value =
                        donations.DateReceived == null ? (object)DBNull.Value : donations.DateReceived.Value.Date;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminDonationsAsync(AdminDonations donations)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donorId = await ResolveDonorIdAsync(connection, donations.Donor);

                string query = @"
                UPDATE Donation
                SET
                    Donation_ID = @donationId,
                    Donor_ID = @donorId,
                    Event_ID = @eventId,
                    Date_Received = @dateReceived
                WHERE Donation_ID = @donoationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donations.DonationId.Trim();
                    command.Parameters.Add("@donorId", SqlDbType.VarChar, 10).Value = donorId;
                    command.Parameters.Add("@eventId", SqlDbType.VarChar, 10).Value = donations.Event.Trim();
                    command.Parameters.Add("@dateReceived", SqlDbType.DateTime).Value =
                                           donations.DateReceived == null ? (object)DBNull.Value : donations.DateReceived.Value.Date;
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No donation record was updated. The donation may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminDonationsAsync(string donationId)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
                DELETE FROM Donation
                WHERE Donation_ID = @donoationId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No donation record was deleted. The donation may no longer exist.");
                    }
                }
            }
        }

        private static async Task<string> ResolveDonationIdAsync(SqlConnection connection, string donationInput)
        {
            string input = donationInput == null ? string.Empty : donationInput.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Donation ID is required.");
            }

            string query = @"
            SELECT TOP 1 Donation_ID
            FROM Donation
            WHERE Donation_ID = @donationInput
            ORDER BY 
                CASE WHEN Donation_ID = @donationInput THEN 0 ELSE 1 END;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@donationInput", SqlDbType.VarChar, 255).Value = input;

                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("The donation was not found. Enter an existing Donation ID.");
                }

                return result.ToString();
            }
        }

        public static async Task<ObservableCollection<AdminDonations>> GetAdminDonatedItemsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminDonations> donatedItem = new ObservableCollection<AdminDonations>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Items" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                di.DonatedItem_ID,
                di.Donation_ID,
                di.Item_Name,
                di.Quantity_Received
            FROM [Donated Items] di
            INNER JOIN Donation d ON di.Donation_ID = d.Donation_ID
            WHERE
                (
                    @searchText = '%'
                    OR di.DonatedItem_ID LIKE @searchText
                    OR di.Donation_ID LIKE @searchText
                    OR di.Item_Name LIKE @searchText
                )
                AND
                (
                    @filter = 'All Items'
                    OR (@filter = 'Food' AND di.Item_Name IN ('Rice','Canned Goods'))
                    OR (@filter = 'Water' AND di.Item_Name LIKE '%Water%')
                    OR (@filter = 'Clothing' AND di.Item_Name IN ('Blankets'))
                    OR (@filter = 'Medical' AND di.Item_Name IN ('Medicines','First Aid Kit'))
                )
            ORDER BY di.DonatedItem_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            donatedItem.Add(new AdminDonations
                            {
                                DonatedItemID = reader["DonatedItem_ID"] == DBNull.Value ? string.Empty : reader["DonatedItem_ID"].ToString(),
                                DonationId = reader["Donation_ID"] == DBNull.Value ? string.Empty : reader["Donation_ID"].ToString(),
                                ItemName = reader["Item_Name"] == DBNull.Value ? string.Empty : reader["Item_Name"].ToString(),
                                Quantity = reader["Quantity_Received"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity_Received"]),
                            });
                        }
                    }
                }
            }


            return donatedItem;
        }

        public static async Task AddAdminDonatedItemsAsync(AdminDonations donatedItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donationId = await ResolveDonationIdAsync(connection, donatedItems.DonationId);

                string query = @"
            INSERT INTO [Donated Items]
                (DonatedItem_ID, Donation_ID, Item_Name, Quantity_Received)
            VALUES
                (@donateditemId, @donationId, @itemName, @quantityReceived);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donateditemId", SqlDbType.VarChar, 10).Value = donatedItems.DonatedItemID.Trim();
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId;
                    command.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = donatedItems.ItemName.Trim();
                    command.Parameters.Add("@quantityReceived", SqlDbType.Int).Value = donatedItems.Quantity;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminDonatedItemsAsync(AdminDonations donatedItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donationId = await ResolveDonationIdAsync(connection, donatedItems.DonationId);

                string query = @"
            UPDATE [Donated Items]
            SET
                DonatedItem_ID = @donateditemId,
                Donation_ID = @donationId,                
                Item_Name = @itemName,
                Quantity_Received = @quantityReceived
            WHERE DonatedItem_ID = @donateditemId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donateditemId", SqlDbType.VarChar, 10).Value = donatedItems.DonatedItemID.Trim();
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId;
                    command.Parameters.Add("@itemName", SqlDbType.VarChar, 10).Value = donatedItems.ItemName.Trim();
                    command.Parameters.Add("@quantityReceived", SqlDbType.Int).Value = donatedItems.Quantity;

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No donated item record was updated. The donated item may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminDonatedItemAsync(string donatedItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Donated Items]
            WHERE DonatedItem_ID = @donateditemId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@donateditemId", SqlDbType.VarChar, 10).Value = donatedItems.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No donated item record was deleted. The donated item may no longer exist.");
                    }
                }
            }
        }

        public static async Task<ObservableCollection<AdminPledges>> GetAdminPledgesAsync(string filter, string searchText)
        {
            ObservableCollection<AdminPledges> pledges = new ObservableCollection<AdminPledges>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Pledges" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                p.Pledge_ID,
                p.Donor_ID,
                p.Date_Pledge,
                p.Pledge_Status
            FROM Pledge p
            LEFT JOIN Donor do ON p.Donor_ID = do.Donor_ID
            WHERE
                (
                    @searchText = '%'
                    OR p.Pledge_ID LIKE @searchText                    
                    OR p.Donor_ID LIKE @searchText
                )
                AND
                (
                    @filter = 'All Pledges'
                    OR (@filter = 'Pending'   AND p.Pledge_Status = 'Pending')
                    OR (@filter = 'Approved'  AND p.Pledge_Status = 'Approved')
                    OR (@filter = 'Fulfilled' AND p.Pledge_Status = 'Fulfilled')
                    OR (@filter = 'Cancelled' AND p.Pledge_Status = 'Cancelled')
                )
            ORDER BY p.Pledge_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pledges.Add(new AdminPledges
                            {
                                PledgeId = reader["Pledge_ID"] == DBNull.Value ? string.Empty : reader["Pledge_ID"].ToString(),
                                DonorId = reader["Donor_ID"] == DBNull.Value ? string.Empty : reader["Donor_ID"].ToString(),
                                DatePledge = reader["Date_Pledge"] == DBNull.Value ? (DateTime?)null
                                : Convert.ToDateTime(reader["Date_Pledge"]),
                                PledgeStatus = reader["Pledge_Status"] == DBNull.Value ? string.Empty : reader["Pledge_Status"].ToString(),

                            });
                        }
                    }
                }

                return pledges;

            }
        }

        public static async Task AddAdminPledgeAsync(AdminPledges pledges)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donorId = await ResolveDonorIdAsync(connection, pledges.DonorId);

                string query = @"
            INSERT INTO Pledge
                (Pledge_ID, Donor_ID, Date_Pledge, Pledge_Status)
            VALUES
                (@pledgeId, @donorId, @datePledge, @pledgeStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeId", SqlDbType.VarChar, 10).Value = pledges.PledgeId.Trim();
                    command.Parameters.Add("@Donor_ID", SqlDbType.VarChar, 10).Value = donorId;
                    command.Parameters.Add("@dateReceived", SqlDbType.DateTime).Value =
                       pledges.DatePledge == null ? (object)DBNull.Value : pledges.DatePledge.Value.Date;
                    command.Parameters.Add("@Pledge_Status", SqlDbType.VarChar, 50).Value = pledges.PledgeStatus.Trim();

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminPledgeAsync(AdminPledges pledges)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donorId = await ResolveDonorIdAsync(connection, pledges.DonorId);

                string query = @"
            UPDATE Pledge
            SET
                Pledge_ID = @pledgeId,
                Donor_ID = @donorId,
                Date_Pledge = @datePledge,
                Pledge_Status = @pledgeStatus
            WHERE Pledge_ID = @pledgeId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeId", SqlDbType.VarChar, 10).Value = pledges.PledgeId.Trim();
                    command.Parameters.Add("@donorId", SqlDbType.VarChar, 10).Value = donorId;
                    command.Parameters.Add("@datePledge", SqlDbType.DateTime).Value =
                                           pledges.DatePledge == null ? (object)DBNull.Value : pledges.DatePledge.Value.Date;
                    command.Parameters.Add("@pledgeStatus", SqlDbType.VarChar, 10).Value = pledges.PledgeStatus.Trim();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pledge record was updated. The pledge may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminPledgeAsync(string pledges)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM Pledge
            WHERE Pledge_ID = @pledgeId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeId", SqlDbType.VarChar, 10).Value = pledges.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pledge record was deleted. The pledge may no longer exist.");
                    }
                }
            }
        }

        public static async Task<ObservableCollection<AdminPledges>> GetAdminPledgeItemsAsync(string filter, string searchText)
        {
            ObservableCollection<AdminPledges> pledgeItem = new ObservableCollection<AdminPledges>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Items" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                pi.PledgeItem_ID,
                pi.Pledge_ID,
                pi.Item_Name,
                pi.Quantity,
                pi.ExpectedDelivery_Date
                
            FROM [Pledge Item] pi
            INNER JOIN Pledge p ON pi.Pledge_ID = p.Pledge_ID
            WHERE
                (
                    @searchText = '%'
                    OR pi.PledgeItem_ID LIKE @searchText
                    OR pi.Pledge_ID LIKE @searchText
                    OR pi.Item_Name LIKE @searchText
                )
                AND
                (
                    @filter = 'All Items'
                    OR (@filter = 'Food' AND pi.Item_Name IN ('Rice','Canned Goods','eggs'))
                    OR (@filter = 'Water' AND pi.Item_Name LIKE '%Water%')
                    OR (@filter = 'Clothing' AND pi.Item_Name IN ('Blankets'))
                    OR (@filter = 'Medical' AND pi.Item_Name IN ('Medicines','First Aid Kit'))
                )
            ORDER BY pi.PledgeItem_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pledgeItem.Add(new AdminPledges
                            {
                                PledgeItemId = reader["PledgeItem_ID"] == DBNull.Value ? string.Empty : reader["PledgeItem_ID"].ToString(),
                                PledgeId = reader["Pledge_ID"] == DBNull.Value ? string.Empty : reader["Pledge_ID"].ToString(),
                                ItemName = reader["Item_Name"] == DBNull.Value ? string.Empty : reader["Item_Name"].ToString(),
                                Quantity = reader["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity"]),
                                ExpectedDelivery = reader["ExpectedDelivery_Date"] == DBNull.Value ? (DateTime?)null
                                : Convert.ToDateTime(reader["ExpectedDelivery_Date"]),
                            });
                        }
                    }
                }
            }


            return pledgeItem;
        }
        private static async Task<string> ResolvePledgeIdAsync(SqlConnection connection, string pledgeInput)
        {
            string input = pledgeInput == null ? string.Empty : pledgeInput.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Pledge ID is required.");
            }

            string query = @"
        SELECT TOP 1 Pledge_ID
        FROM Pledge
        WHERE Pledge_ID = @pledgeInput
        ORDER BY 
            CASE WHEN Pledge_ID = @pledgeInput THEN 0 ELSE 1 END;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@pledgeInput", SqlDbType.VarChar, 255).Value = input;

                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("The pledge was not found. Enter an existing Pledge ID.");
                }

                return result.ToString();
            }
        }
        public static async Task AddAdminPledgeItemsAsync(AdminPledges pledgeItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string pledgeId = await ResolvePledgeIdAsync(connection, pledgeItems.PledgeId);

                string query = @"
            INSERT INTO [Pledge Item]
                (PledgeItem_ID, Pledge_ID, Item_Name, Quantity, ExpectedDelivery_Date)
            VALUES
                (@pledgeitemId, @pledgeId, @itemName, @quantity, @expectedDeliveryDate);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeitemId", SqlDbType.VarChar, 10).Value = pledgeItems.PledgeItemId.Trim();
                    command.Parameters.Add("@pledgeId", SqlDbType.VarChar, 10).Value = pledgeId;
                    command.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = pledgeItems.ItemName.Trim();
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = pledgeItems.Quantity;
                    command.Parameters.Add("@expectedDeliveryDate", SqlDbType.DateTime).Value =
                        pledgeItems.ExpectedDelivery == null ? (object)DBNull.Value : pledgeItems.ExpectedDelivery.Value.Date;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public static async Task UpdateAdminPledgeItemsAsync(AdminPledges pledgeItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string pledgeId = await ResolvePledgeIdAsync(connection, pledgeItems.PledgeId);

                string query = @"
            UPDATE [Pledge Item]
            SET
                PledgeItem_ID = @pledgeitemId,
                Pledge_ID = @pledgeId,                
                Item_Name = @itemName,
                Quantity = @quantity,
                ExpectedDelivery_Date = @expectedDeliveryDate
            WHERE PledgeItem_ID = @pledgeitemId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeitemId", SqlDbType.VarChar, 10).Value = pledgeItems.PledgeItemId.Trim();
                    command.Parameters.Add("@pledgeId", SqlDbType.VarChar, 10).Value = pledgeId;
                    command.Parameters.Add("@itemName", SqlDbType.VarChar, 10).Value = pledgeItems.ItemName.Trim();
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = pledgeItems.Quantity;
                    command.Parameters.Add("@expectedDeliveryDate", SqlDbType.DateTime).Value =
                       pledgeItems.ExpectedDelivery == null ? (object)DBNull.Value : pledgeItems.ExpectedDelivery.Value.Date;

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pledge item record was updated. The pledge item may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminPledgeItemAsync(string pledgeItems)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Pledge Item]
            WHERE PledgeItem_ID = @pledgeitemId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pledgeitemId", SqlDbType.VarChar, 10).Value = pledgeItems.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pledge item record was deleted. The pledge item may no longer exist.");
                    }
                }
            }
        }

        /// <summary>Update only the Pledge_Status column — admin can only change status, not donor data.</summary>
        public static async Task UpdateAdminPledgeStatusAsync(string pledgeId, string newStatus)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "UPDATE Pledge SET Pledge_Status = @status WHERE Pledge_ID = @id";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@status", SqlDbType.VarChar, 50).Value = newStatus;
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = pledgeId;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Called when a pledge is marked Fulfilled and the item is already received.
        /// Adds a donation record using usp_AddDonation, linked to the donor.
        /// Event_ID is left null since the fulfilled pledge donation may not be event-linked.
        /// </summary>
        public static async Task AddDonationFromPledgeAsync(string donorId)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_AddDonation", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@donorID", SqlDbType.VarChar, 10).Value = donorId;
                    cmd.Parameters.Add("@eventID", SqlDbType.VarChar, 10).Value = DBNull.Value;
                    cmd.Parameters.Add("@dateReceived", SqlDbType.DateTime).Value = DateTime.Today;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>Generate next pickup ID using PU prefix with 4-digit zero-padding.</summary>
        public static async Task<string> GeneratePickupIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Pickup_ID, 3, LEN(Pickup_ID)) AS INT)), 0) + 1
                             FROM [Pickup Schedule] WHERE Pickup_ID LIKE 'PU[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "PU" + next.ToString("D4");
                }
            }
        }

        /// <summary>Add a pickup schedule row directly.</summary>
        public static async Task AddPickupScheduleAsync(string pickupId, string donationId, DateTime pickupDate, string status)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"INSERT INTO [Pickup Schedule] (Pickup_ID, Donation_ID, Pickup_Date, Pickup_Status)
                             VALUES (@id, @donationId, @pickupDate, @status)";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = pickupId;
                    cmd.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId;
                    cmd.Parameters.Add("@pickupDate", SqlDbType.Date).Value = pickupDate.Date;
                    cmd.Parameters.Add("@status", SqlDbType.VarChar, 50).Value = status;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>Link an existing donation to a distribution by inserting a distribution item or just validating the relationship.</summary>
        public static async Task LinkDonationToDistributionAsync(string donationId, string distributionId)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand check = new SqlCommand(
                    "SELECT COUNT(1) FROM Distribution WHERE Distribution_ID = @did", conn))
                {
                    check.Parameters.Add("@did", SqlDbType.VarChar, 10).Value = distributionId;
                    int count = Convert.ToInt32(await check.ExecuteScalarAsync());
                    if (count == 0)
                        throw new Exception($"Distribution {distributionId} does not exist.");
                }
            }
        }

        /// <summary>Returns all distributions as (ID, display) pairs for dropdown selection.</summary>
        public static async Task<List<(string Id, string Display)>> GetAvailableDistributionsAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"
                    SELECT d.Distribution_ID,
                           d.Distribution_ID + ISNULL(' — ' + de.Event_Name, '') AS Display
                    FROM Distribution d
                    LEFT JOIN [Disaster Event] de ON d.Event_ID = de.Event_ID
                    ORDER BY d.Distribution_ID";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Distribution_ID"].ToString(), reader["Display"].ToString()));
                }
            }
            return list;
        }

        /// <summary>Generate next delivery ID using DL prefix (matches usp_AddDeliverySchedule).</summary>
        public static async Task<string> GenerateDeliveryIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                // usp_AddDeliverySchedule uses 'DL' prefix
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Delivery_ID, 3, LEN(Delivery_ID)) AS INT)), 0) + 1
                             FROM [Delivery Schedule] WHERE Delivery_ID LIKE 'DL[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "DL" + next.ToString("D4");
                }
            }
        }

        /// <summary>Add a delivery schedule using usp_AddDeliverySchedule stored procedure.</summary>
        public static async Task AddDeliveryViaSpAsync(string distributionId, DateTime deliveryDate, string status)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_AddDeliverySchedule", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@distributionID", SqlDbType.VarChar, 10).Value = distributionId;
                    cmd.Parameters.Add("@deliveryDate", SqlDbType.Date).Value = deliveryDate.Date;
                    cmd.Parameters.Add("@deliveryStatus", SqlDbType.VarChar, 50).Value = status;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Add a distribution. Accepts a typed beneficiary name — finds or creates the Beneficiary row.
        /// Uses usp_AddDistribution stored procedure.
        /// </summary>
        public static async Task AddDistributionViaSpAsync(string beneficiaryName, string eventId, string centerId, DateTime? dateDistributed)
        {
            string beneficiaryId = await FindOrCreateBeneficiaryAsync(beneficiaryName);
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_AddDistribution", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@beneficiaryID", SqlDbType.VarChar, 10).Value = beneficiaryId;
                    cmd.Parameters.Add("@eventID", SqlDbType.VarChar, 10).Value = eventId;
                    cmd.Parameters.Add("@centerID", SqlDbType.VarChar, 10).Value = centerId;
                    cmd.Parameters.Add("@dateDistributed", SqlDbType.Date).Value =
                        dateDistributed.HasValue ? (object)dateDistributed.Value.Date : DBNull.Value;
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Finds an existing Beneficiary by name (case-insensitive) or creates a new one.
        /// Returns the Beneficiary_ID.
        /// </summary>
        public static async Task<string> FindOrCreateBeneficiaryAsync(string name, string category = "General")
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand find = new SqlCommand(
                    "SELECT TOP 1 Benificiary_ID FROM Benificiary WHERE LOWER(Name) = LOWER(@name)", conn))
                {
                    find.Parameters.Add("@name", SqlDbType.VarChar, 255).Value = name.Trim();
                    object r = await find.ExecuteScalarAsync();
                    if (r != null && r != DBNull.Value) return r.ToString();
                }
                using (SqlCommand create = new SqlCommand("usp_AddBenificiary", conn))
                {
                    create.CommandType = System.Data.CommandType.StoredProcedure;
                    create.Parameters.Add("@name", SqlDbType.VarChar, 255).Value = name.Trim();
                    create.Parameters.Add("@category", SqlDbType.VarChar, 100).Value = category;
                    create.Parameters.Add("@centerID", SqlDbType.VarChar, 10).Value = DBNull.Value;
                    await create.ExecuteNonQueryAsync();
                }
                using (SqlCommand get = new SqlCommand(
                    "SELECT TOP 1 Benificiary_ID FROM Benificiary WHERE LOWER(Name)=LOWER(@name) ORDER BY Benificiary_ID DESC", conn))
                {
                    get.Parameters.Add("@name", SqlDbType.VarChar, 255).Value = name.Trim();
                    return (await get.ExecuteScalarAsync())?.ToString() ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// When a pickup is completed, add its donation to Inventory via usp_AddDonatedItem.
        /// Gets the Donation_ID from the pickup, then calls the stored procedure.
        /// </summary>
        public static async Task AddPickupToInventoryAsync(string pickupId, string itemName, int quantity, string unit, string category)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();
                // First get the donation ID for this pickup
                string donationId;
                using (SqlCommand cmd = new SqlCommand("SELECT Donation_ID FROM [Pickup Schedule] WHERE Pickup_ID = @id", conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = pickupId;
                    donationId = (await cmd.ExecuteScalarAsync())?.ToString();
                }
                if (string.IsNullOrEmpty(donationId))
                    throw new Exception("No donation linked to this pickup.");

                // usp_AddDonatedItem auto-links to latest donation and creates Inventory row
                using (SqlCommand cmd = new SqlCommand("usp_AddDonatedItem", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 255).Value = itemName;
                    cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                    cmd.Parameters.Add("@unit", SqlDbType.VarChar, 50).Value = unit;
                    cmd.Parameters.Add("@category", SqlDbType.VarChar, 50).Value = category;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Add a distribution item. Verifies distribution exists, validates inventory quantity,
        /// uses DSI prefix to avoid collision with DT (Donated Items) and DI (old generator).
        /// Returns remaining inventory quantity.
        /// </summary>
        public static async Task<int> AddDistributionItemAsync(string distributionId, string inventoryId, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();

                // Verify distribution exists
                using (SqlCommand chk = new SqlCommand(
                    "SELECT COUNT(1) FROM Distribution WHERE Distribution_ID = @did", conn))
                {
                    chk.Parameters.Add("@did", SqlDbType.VarChar, 10).Value = distributionId.Trim();
                    if (Convert.ToInt32(await chk.ExecuteScalarAsync()) == 0)
                        throw new Exception($"Distribution '{distributionId}' not found. Select an existing distribution from the grid first.");
                }

                // Check inventory
                int available;
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(Quantity_Available,0) FROM Inventory WHERE Inventory_ID=@id", conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = inventoryId.Trim();
                    available = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                if (quantity > available)
                    throw new Exception($"Cannot distribute {quantity} — only {available} available.");

                // Look up the DonatedItem_ID linked to this Inventory row.
                // The live [Distribution Items] table has a FK on DonatedItem_ID
                // referencing [Donated Items](DonatedItem_ID), so we must supply it.
                string donatedItemId;
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT DonatedItem_ID FROM Inventory WHERE Inventory_ID = @id", conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = inventoryId.Trim();
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                        throw new Exception($"No DonatedItem_ID found for Inventory '{inventoryId}'. Cannot insert distribution item.");
                    donatedItemId = result.ToString();
                }

                // Generate DSI#### ID
                string newId;
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT ISNULL(MAX(CAST(SUBSTRING(DistributionItem_ID,4,LEN(DistributionItem_ID)) AS INT)),0)+1
                      FROM [Distribution Items] WHERE DistributionItem_ID LIKE 'DSI[0-9]%'", conn))
                    newId = "DSI" + Convert.ToInt32(await cmd.ExecuteScalarAsync()).ToString("D3");

                // INSERT uses DonatedItem_ID (FK to Donated Items) + Inventory_ID (FK to Inventory)
                // Try with both columns first; if the live schema only has DonatedItem_ID, the second
                // parameter is ignored gracefully via the catch-and-retry below.
                string q = @"
                            INSERT INTO [Distribution Items]
                                (DistributionItem_ID, Distribution_ID, Inventory_ID, Quantity)
                            VALUES(@itemId, @donatedItemId, @invId, @qty);

                            UPDATE Inventory SET Quantity_Available = Quantity_Available - @qty
                            WHERE Inventory_ID = @invId AND Quantity_Available >= @qty;";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@itemId", SqlDbType.VarChar, 10).Value = newId;
                    cmd.Parameters.Add("@distId", SqlDbType.VarChar, 10).Value = distributionId.Trim();
                    cmd.Parameters.Add("@donatedItemId", SqlDbType.VarChar, 10).Value = donatedItemId;
                    cmd.Parameters.Add("@qty", SqlDbType.Int).Value = quantity;
                    cmd.Parameters.Add("@invId", SqlDbType.VarChar, 10).Value = inventoryId.Trim();
                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows < 2)
                        throw new Exception("Inventory deduction failed — quantity may have changed. Please retry.");
                }
                return available - quantity;
            }
        }


        public static async Task<ObservableCollection<AdminLogistics>> GetAdminDeliveriesAsync(string filter, string searchText)
        {
            ObservableCollection<AdminLogistics> deliveries = new ObservableCollection<AdminLogistics>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Deliveries" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                dv.Delivery_ID,
                dv.Distribution_ID,
                dv.Delivery_Date,
                dv.Delivery_Status
            FROM [Delivery Schedule] dv
            INNER JOIN Distribution dt ON dv.Distribution_ID = dt.Distribution_ID
            WHERE
                (
                    @searchText = '%'
                    OR dv.Delivery_ID LIKE @searchText
                    OR dv.Distribution_ID LIKE @searchText
                )
                AND
                (
                    @filter = 'All Deliveries'
                    OR (@filter = 'Pending' AND dv.Delivery_Status = 'Pending')
                    OR (@filter = 'In Transit' AND dv.Delivery_Status = 'In Transit')
                    OR (@filter = 'Delivered' AND dv.Delivery_Status = 'Delivered')
                )
            ORDER BY dv.Delivery_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            deliveries.Add(new AdminLogistics
                            {
                                DeliveryId = reader["Delivery_ID"] == DBNull.Value ? string.Empty : reader["Delivery_ID"].ToString(),
                                Distribution = reader["Distribution_ID"] == DBNull.Value ? string.Empty : reader["Distribution_ID"].ToString(),
                                DeliveryDate = reader["Delivery_Date"] == DBNull.Value ? (DateTime?)null
                                : Convert.ToDateTime(reader["Delivery_Date"]),
                                Status = reader["Delivery_Status"] == DBNull.Value ? string.Empty : reader["Delivery_Status"].ToString(),

                            });
                        }
                    }
                }
            }

            return deliveries;
        }
        private static async Task<string> ResolveDistributionIdAsync(SqlConnection connection, string distributionInput)
        {
            string input = distributionInput == null ? string.Empty : distributionInput.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Distribution ID is required.");
            }

            string query = @"
        SELECT TOP 1 Distribution_ID
        FROM Distribution
        WHERE Distribution_ID = @distributionInput
        ORDER BY 
            CASE WHEN Distribution_ID = @distributionInput THEN 0 ELSE 1 END;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@distributionInput", SqlDbType.VarChar, 255).Value = input;

                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("The distribution was not found. Enter an existing Distribution ID.");
                }

                return result.ToString();
            }
        }

        public static async Task AddAdminDeliveriesAsync(AdminLogistics deliveries)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string distributionId = await ResolveDistributionIdAsync(connection, deliveries.Distribution);

                string query = @"
            INSERT INTO [Delivery Schedule]
                (Delivery_ID, Distribution_ID, Delivery_Date, Delivery_Status)
            VALUES
                (@deliveryId, @distributionId, @deliveryDate, @deliveryStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@deliveryId", SqlDbType.VarChar, 10).Value = deliveries.DeliveryId.Trim();
                    command.Parameters.Add("@distributionId", SqlDbType.VarChar, 10).Value = distributionId;
                    command.Parameters.Add("@deliveryDate", SqlDbType.DateTime).Value =
                       deliveries.DeliveryDate == null ? (object)DBNull.Value : deliveries.DeliveryDate.Value.Date;
                    command.Parameters.Add("@deliveryStatus", SqlDbType.VarChar, 50).Value = deliveries.Status.Trim();

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminDeliveriesAsync(AdminLogistics deliveries)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string distributionId = await ResolveDistributionIdAsync(connection, deliveries.Distribution);

                string query = @"
                UPDATE [Delivery Schedule]
                SET
                    Delivery_ID = @deliveryId,
                    Distribution_ID = @distributionId,
                    Delivery_Date = @deliveryDate,
                    Delivery_Status = @deliveryStatus
                WHERE Delivery_ID = @deliveryId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@deliveryId", SqlDbType.VarChar, 10).Value = deliveries.DeliveryId.Trim();
                    command.Parameters.Add("@distributionId", SqlDbType.VarChar, 10).Value = distributionId;
                    command.Parameters.Add("@deliveryDate", SqlDbType.DateTime).Value =
                                           deliveries.DeliveryDate == null ? (object)DBNull.Value : deliveries.DeliveryDate.Value.Date;
                    command.Parameters.Add("@deliveryStatus", SqlDbType.VarChar, 50).Value = deliveries.Status.Trim();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No delivery record was updated. The delivery may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminDeliveriesAsync(string deliveries)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Delivery Schedule]
            WHERE Delivery_ID = @deliveryId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@deliveryId", SqlDbType.VarChar, 10).Value = deliveries.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No delivery record was deleted. The delivery may no longer exist.");
                    }
                }
            }
        }

        public static async Task<ObservableCollection<AdminLogistics>> GetAdminPickupAsync(string filter, string searchText)
        {
            ObservableCollection<AdminLogistics> pickups = new ObservableCollection<AdminLogistics>();

            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Pickups" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            SELECT 
                pc.Pickup_ID,
                pc.Donation_ID,
                pc.Pickup_Date,
                pc.Pickup_Status
            FROM [Pickup Schedule] pc
            INNER JOIN Donation d ON pc.Donation_ID = d.Donation_ID
            WHERE
                (
                    @searchText = '%'
                    OR pc.Pickup_ID LIKE @searchText
                    OR pc.Donation_ID LIKE @searchText
                )
                AND
                (
                    @filter = 'All Pickups'
                    OR (@filter = 'Scheduled' AND pc.Pickup_Status = 'Scheduled')
                    OR (@filter = 'Completed' AND pc.Pickup_Status = 'Completed')
                    OR (@filter = 'Cancelled' AND pc.Pickup_Status = 'Cancelled')
                )
            ORDER BY pc.Pickup_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    command.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pickups.Add(new AdminLogistics
                            {
                                PickupId = reader["Pickup_ID"] == DBNull.Value ? string.Empty : reader["Pickup_ID"].ToString(),
                                Donation = reader["Donation_ID"] == DBNull.Value ? string.Empty : reader["Donation_ID"].ToString(),
                                PickupDate = reader["Pickup_Date"] == DBNull.Value ? (DateTime?)null
                                : Convert.ToDateTime(reader["Pickup_Date"]),
                                Status = reader["Pickup_Status"] == DBNull.Value ? string.Empty : reader["Pickup_Status"].ToString(),

                            });
                        }
                    }
                }
            }

            return pickups;
        }
        public static async Task AddAdminPickupsAsync(AdminLogistics pickups)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donationId = await ResolveDonationIdAsync(connection, pickups.Donation);

                string query = @"
            INSERT INTO [Pickup Schedule]
                (Pickup_ID, Donation_ID, Pickup_Date, Pickup_Status)
            VALUES
                (@pickupId, @donationId, @pickupDate, @pickupStatus);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pickupId", SqlDbType.VarChar, 10).Value = pickups.PickupId.Trim();
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId;
                    command.Parameters.Add("@pickupDate", SqlDbType.DateTime).Value =
                       pickups.PickupDate == null ? (object)DBNull.Value : pickups.PickupDate.Value.Date;
                    command.Parameters.Add("@pickupStatus", SqlDbType.VarChar, 50).Value = pickups.Status.Trim();

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAdminPickupsAsync(AdminLogistics pickups)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                await connection.OpenAsync();

                string donationId = await ResolveDonationIdAsync(connection, pickups.Donation);

                string query = @"
            UPDATE [Pickup Schedule]
            SET
                Pickup_ID = @pickupId,
                Donation_ID = @donationId,
                Pickup_Date = @pickupDate,
                Pickup_Status = @pickupStatus
            WHERE Pickup_ID = @pickupId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pickupId", SqlDbType.VarChar, 10).Value = pickups.PickupId.Trim();
                    command.Parameters.Add("@donationId", SqlDbType.VarChar, 10).Value = donationId;
                    command.Parameters.Add("@pickupDate", SqlDbType.DateTime).Value =
                                           pickups.PickupDate == null ? (object)DBNull.Value : pickups.PickupDate.Value.Date;
                    command.Parameters.Add("@pickupStatus", SqlDbType.VarChar, 50).Value = pickups.Status.Trim();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pickup record was updated. The pickup may no longer exist.");
                    }
                }
            }
        }

        public static async Task DeleteAdminPickupsAsync(string pickups)
        {
            using (SqlConnection connection = new SqlConnection(SQL.connectionString))
            {
                string query = @"
            DELETE FROM [Pickup Schedule]
            WHERE Pickup_ID = @pickupId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pickupId", SqlDbType.VarChar, 10).Value = pickups.Trim();

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("No pickup record was deleted. The pickup may no longer exist.");
                    }
                }
            }
        }

        /* ================================================================
           INVENTORY — read-only view of donated items in stock
           ================================================================ */

        public static async Task<List<AdminInventory>> GetAdminInventoryAsync(string filter, string searchText)
        {
            var list = new List<AdminInventory>();
            string selectedFilter = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Inventory" : filter;
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"
                    SELECT
                        inv.Inventory_ID,
                        i.Item_Name,
                        di.Item_Name AS DonatedItem,
                        inv.Quantity_Available,
                        inv.Expiration_Date,
                        inv.Storage_Location
                    FROM Inventory inv
                    LEFT JOIN Item i ON inv.Item_ID = i.Item_ID
                    LEFT JOIN [Donated Items] di ON inv.DonatedItem_ID = di.DonatedItem_ID
                    WHERE
                        inv.Quantity_Available > 0
                        AND
                        (
                            @searchText = '%'
                            OR inv.Inventory_ID LIKE @searchText
                            OR ISNULL(i.Item_Name, '') LIKE @searchText
                            OR ISNULL(di.Item_Name, '') LIKE @searchText
                            OR ISNULL(inv.Storage_Location, '') LIKE @searchText
                        )
                        AND
                        (
                            @filter = 'All Inventory'
                            OR (@filter = 'Available'      AND inv.Quantity_Available > 0
                                                           AND (inv.Expiration_Date IS NULL OR inv.Expiration_Date > CAST(GETDATE() AS DATE)))
                            OR (@filter = 'Expiring Soon'  AND inv.Expiration_Date IS NOT NULL
                                                           AND inv.Expiration_Date > CAST(GETDATE() AS DATE)
                                                           AND inv.Expiration_Date <= DATEADD(DAY, 30, CAST(GETDATE() AS DATE)))
                            OR (@filter = 'Low Stock'      AND inv.Quantity_Available > 0
                                                           AND inv.Quantity_Available <= 10)
                        )
                    ORDER BY inv.Inventory_ID";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    cmd.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = selectedFilter;
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new AdminInventory
                            {
                                InventoryId = reader["Inventory_ID"].ToString(),
                                Item = reader["Item_Name"] == DBNull.Value ? "" : reader["Item_Name"].ToString(),
                                DonatedItem = reader["DonatedItem"] == DBNull.Value ? "" : reader["DonatedItem"].ToString(),
                                QuantityAvailable = reader["Quantity_Available"] == DBNull.Value ? "" : reader["Quantity_Available"].ToString(),
                                ExpirationDate = reader["Expiration_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Expiration_Date"]),
                                StorageLocation = reader["Storage_Location"] == DBNull.Value ? "" : reader["Storage_Location"].ToString(),
                            });
                        }
                    }
                }
            }
            return list;
        }

        /* ================================================================
           DISTRIBUTION + DISTRIBUTION ITEMS
           ================================================================ */

        public static async Task<List<AdminDistribution>> GetAdminDistributionAsync(string filter, string searchText)
        {
            var list = new List<AdminDistribution>();
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"
                    SELECT
                        dist.Distribution_ID,
                        b.Name AS Beneficiary,
                        de.Event_Name,
                        ec.Center_Name AS DistributionLocation,
                        dist.Date_Distributed
                    FROM Distribution dist
                    LEFT JOIN Benificiary b ON dist.Beneficiary_ID = b.Benificiary_ID
                    LEFT JOIN [Disaster Event] de ON dist.Event_ID = de.Event_ID
                    LEFT JOIN [Evacuation Center] ec ON dist.Center_ID = ec.Center_ID
                    WHERE
                        (
                            @searchText = '%'
                            OR dist.Distribution_ID LIKE @searchText
                            OR ISNULL(b.Name, '') LIKE @searchText
                            OR ISNULL(de.Event_Name, '') LIKE @searchText
                            OR ISNULL(ec.Center_Name, '') LIKE @searchText
                        )
                        AND
                        (
                            @filter = 'All Distribution'
                            OR (@filter = 'Ongoing'   AND dist.Date_Distributed IS NULL)
                            OR (@filter = 'Completed' AND dist.Date_Distributed IS NOT NULL)
                        )
                    ORDER BY dist.Distribution_ID";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    cmd.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(filter) ? "All Distribution" : filter;
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new AdminDistribution
                            {
                                DistributionId = reader["Distribution_ID"].ToString(),
                                Beneficiary = reader["Beneficiary"] == DBNull.Value ? "" : reader["Beneficiary"].ToString(),
                                Event = reader["Event_Name"] == DBNull.Value ? "" : reader["Event_Name"].ToString(),
                                DistributionLocation = reader["DistributionLocation"] == DBNull.Value ? "" : reader["DistributionLocation"].ToString(),
                                DateDistributed = reader["Date_Distributed"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date_Distributed"]),
                            });
                        }
                    }
                }
            }
            return list;
        }

        public static async Task<List<AdminDistributionItem>> GetAdminDistributionItemsAsync(string filter, string searchText)
        {
            var list = new List<AdminDistributionItem>();
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"
                    SELECT
                        ditem.DistributionItem_ID,
                        ditem.Distribution_ID,
                        COALESCE(di.Item_Name, i.Item_Name, ditem.Distribution_ID) AS InventoryItem,
                        ditem.Quantity
                    FROM [Distribution Items] ditem
                    LEFT JOIN [Donated Items] di  ON ditem.Inventory_ID = di.DonatedItem_ID
                    LEFT JOIN Inventory       inv ON di.DonatedItem_ID = inv.DonatedItem_ID
                    LEFT JOIN Item            i   ON inv.Item_ID          = i.Item_ID
                    WHERE
                        @searchText = '%'
                        OR ditem.Distribution_ID LIKE @searchText
                        OR ditem.DistributionItem_ID LIKE @searchText
                        OR ISNULL(di.Item_Name, '')  LIKE @searchText
                    ORDER BY ditem.DistributionItem_ID";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new AdminDistributionItem
                            {
                                DistributionItemId = reader["DistributionItem_ID"].ToString(),
                                DistributionId = reader["Distribution_ID"].ToString(),
                                InventoryItem = reader["InventoryItem"] == DBNull.Value ? "" : reader["InventoryItem"].ToString(),
                                Quantity = reader["Quantity"] == DBNull.Value ? "" : reader["Quantity"].ToString(),
                            });
                        }
                    }
                }
            }
            return list;
        }

        /* ================================================================
           WASTE TRACKING
           ================================================================ */

        public static async Task<List<AdminWaste>> GetAdminWasteAsync(string filter, string searchText)
        {
            var list = new List<AdminWaste>();
            string searchPattern = "%" + (searchText ?? string.Empty).Trim() + "%";

            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"
                    SELECT
                        wt.Wasted_ID,
                        i.Item_Name AS InventoryItem,
                        wt.Quantity_Wasted,
                        wt.Waste_Reason,
                        wt.Date_Recorded
                    FROM [Waste Tracking] wt
                    LEFT JOIN Inventory inv ON wt.Inventory_ID = inv.Inventory_ID
                    LEFT JOIN Item i ON inv.Item_ID = i.Item_ID
                    WHERE
                        (
                            @searchText = '%'
                            OR wt.Wasted_ID LIKE @searchText
                            OR ISNULL(i.Item_Name, '') LIKE @searchText
                            OR ISNULL(wt.Waste_Reason, '') LIKE @searchText
                        )
                        AND
                        (
                            @filter = 'All Records'
                            OR wt.Waste_Reason = @filter
                        )
                    ORDER BY wt.Date_Recorded DESC";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@searchText", SqlDbType.VarChar, 255).Value = searchPattern;
                    cmd.Parameters.Add("@filter", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(filter) || filter == "Filter" ? "All Records" : filter;
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new AdminWaste
                            {
                                WasteId = reader["Wasted_ID"].ToString(),
                                InventoryItem = reader["InventoryItem"] == DBNull.Value ? "" : reader["InventoryItem"].ToString(),
                                Quantity = reader["Quantity_Wasted"] == DBNull.Value ? "" : reader["Quantity_Wasted"].ToString(),
                                Reason = reader["Waste_Reason"] == DBNull.Value ? "" : reader["Waste_Reason"].ToString(),
                                Date = reader["Date_Recorded"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date_Recorded"]),
                            });
                        }
                    }
                }
            }
            return list;
        }

        /* ================================================================
           WASTE — add item to waste tracking (manual + auto-expiry)
           ================================================================ */

        public static async Task AddToWasteAsync(string inventoryId, int quantity, string reason)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                // Generate next Waste ID (W + 4-digit)
                string genQ = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Wasted_ID, 2, LEN(Wasted_ID)) AS INT)), 0) + 1
                                FROM [Waste Tracking] WHERE Wasted_ID LIKE 'W[0-9]%'";
                int nextNum;
                await conn.OpenAsync();
                using (SqlCommand genCmd = new SqlCommand(genQ, conn))
                    nextNum = Convert.ToInt32(await genCmd.ExecuteScalarAsync());

                string newId = "W" + nextNum.ToString("D4");

                string insertQ = @"
                    INSERT INTO [Waste Tracking] (Wasted_ID, Inventory_ID, Quantity_Wasted, Waste_Reason, Date_Recorded)
                    VALUES (@id, @inventoryId, @quantity, @reason, GETDATE());
                    UPDATE Inventory SET Quantity_Available = Quantity_Available - @quantity
                    WHERE Inventory_ID = @inventoryId AND Quantity_Available >= @quantity";

                using (SqlCommand cmd = new SqlCommand(insertQ, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = newId;
                    cmd.Parameters.Add("@inventoryId", SqlDbType.VarChar, 10).Value = inventoryId;
                    cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                    cmd.Parameters.Add("@reason", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(reason) ? "Manually disposed" : reason;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Finds all inventory rows whose Expiration_Date has passed and still have
        /// Quantity_Available > 0, creates a Waste Tracking record for each, and
        /// zeroes their quantity. Returns the number of items moved to waste.
        /// </summary>
        public static async Task<int> MoveExpiredToWasteAsync()
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();

                string findQ = @"
                    SELECT Inventory_ID, Quantity_Available
                    FROM Inventory
                    WHERE Expiration_Date < CAST(GETDATE() AS DATE)
                      AND Quantity_Available > 0";

                var expired = new List<(string id, int qty)>();
                using (SqlCommand findCmd = new SqlCommand(findQ, conn))
                using (var reader = await findCmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                        expired.Add((reader["Inventory_ID"].ToString(),
                                     Convert.ToInt32(reader["Quantity_Available"])));

                foreach (var (invId, qty) in expired)
                {
                    string genQ = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Wasted_ID, 2, LEN(Wasted_ID)) AS INT)), 0) + 1
                                    FROM [Waste Tracking] WHERE Wasted_ID LIKE 'W[0-9]%'";
                    int nextNum;
                    using (SqlCommand genCmd = new SqlCommand(genQ, conn))
                        nextNum = Convert.ToInt32(await genCmd.ExecuteScalarAsync());

                    string newId = "W" + nextNum.ToString("D4");

                    string insertQ = @"
                        INSERT INTO [Waste Tracking] (Wasted_ID, Inventory_ID, Quantity_Wasted, Waste_Reason, Date_Recorded)
                        VALUES (@id, @inventoryId, @quantity, 'Expired', GETDATE());
                        UPDATE Inventory SET Quantity_Available = 0
                        WHERE Inventory_ID = @inventoryId";

                    using (SqlCommand cmd = new SqlCommand(insertQ, conn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = newId;
                        cmd.Parameters.Add("@inventoryId", SqlDbType.VarChar, 10).Value = invId;
                        cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = qty;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Transfers waste quantity back to inventory.
        /// Adds the wasted quantity back to Inventory and deletes the waste record.
        /// </summary>
        public static async Task TransferWasteToInventoryAsync(string wasteId)
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string inventoryId;
                        int quantity;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT Inventory_ID, Quantity_Wasted FROM [Waste Tracking] WHERE Wasted_ID = @id",
                            conn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = wasteId;
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                if (!await reader.ReadAsync())
                                    throw new Exception($"Waste record '{wasteId}' not found.");
                                inventoryId = reader["Inventory_ID"].ToString();
                                quantity    = Convert.ToInt32(reader["Quantity_Wasted"]);
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE Inventory SET Quantity_Available = Quantity_Available + @qty WHERE Inventory_ID = @invId",
                            conn, tx))
                        {
                            cmd.Parameters.Add("@qty",   SqlDbType.Int).Value         = quantity;
                            cmd.Parameters.Add("@invId", SqlDbType.VarChar, 10).Value = inventoryId;
                            int updated = await cmd.ExecuteNonQueryAsync();
                            if (updated == 0)
                                throw new Exception($"Inventory row '{inventoryId}' not found. Transfer aborted.");
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM [Waste Tracking] WHERE Wasted_ID = @wasteId",
                            conn, tx))
                        {
                            cmd.Parameters.Add("@wasteId", SqlDbType.VarChar, 10).Value = wasteId;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /* ================================================================
           ID GENERATORS
           ================================================================ */

        public static async Task<string> GenerateDonationIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Donation_ID, 3, LEN(Donation_ID)) AS INT)), 0) + 1
                             FROM Donation WHERE Donation_ID LIKE 'DN[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "DN" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateDonatedItemIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(DonatedItem_ID, 3, LEN(DonatedItem_ID)) AS INT)), 0) + 1
                             FROM [Donated Items] WHERE DonatedItem_ID LIKE 'DI[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "DI" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateRescuerIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Volunteer_ID, 2, LEN(Volunteer_ID)) AS INT)), 0) + 1
                             FROM Volunteer WHERE Volunteer_ID LIKE 'V[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "V" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateEventIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Event_ID, 2, LEN(Event_ID)) AS INT)), 0) + 1
                             FROM [Disaster Event] WHERE Event_ID LIKE 'E[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "E" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateOperationIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Operation_ID, 3, LEN(Operation_ID)) AS INT)), 0) + 1
                             FROM [Rescue Operation] WHERE Operation_ID LIKE 'RO[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "RO" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateAssignmentIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                // Format: OP0001, OP0002 (OP prefix, 4-digit zero-padded)
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Assignment_ID, 3, LEN(Assignment_ID)) AS INT)), 0) + 1
                             FROM [Operation Assignment] WHERE Assignment_ID LIKE 'OP[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "OP" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateLocationIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Location_ID, 2, LEN(Location_ID)) AS INT)), 0) + 1
                             FROM Location WHERE Location_ID LIKE 'L[0-9]%'";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "L" + next.ToString("D4");
                }
            }
        }

        public static async Task<string> GenerateUserIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT ISNULL(MAX(User_ID), 0) + 1 FROM Users";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return next.ToString();
                }
            }
        }

        /* ================================================================
           DROPDOWN DATA SOURCES
           ================================================================ */

        public static async Task<List<(string Id, string Name)>> GetAvailableDisasterEventsAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Event_ID, Event_Name FROM [Disaster Event] ORDER BY Event_Name";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Event_ID"].ToString(), reader["Event_Name"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Display)>> GetAvailableLocationsAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT Location_ID, Barangay + ', ' + City + ', ' + Province AS Display
                             FROM Location ORDER BY Province, City, Barangay";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Location_ID"].ToString(), reader["Display"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Name)>> GetAvailableRescuersAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Volunteer_ID, ISNULL(Volunteer_Name, Volunteer_ID) AS Volunteer_Name FROM Volunteer ORDER BY Volunteer_Name";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Volunteer_ID"].ToString(), reader["Volunteer_Name"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Display)>> GetAvailableOperationsAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ro.Operation_ID,
                                    ro.Operation_ID + ' — ' + ISNULL(de.Event_Name, 'Unknown Event') AS Display
                             FROM [Rescue Operation] ro
                             LEFT JOIN [Disaster Event] de ON ro.Event_ID = de.Event_ID
                             ORDER BY ro.Operation_ID";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Operation_ID"].ToString(), reader["Display"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Name)>> GetAvailableDonorsAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Donor_ID, ISNULL(Donor_Name, Donor_ID) AS Donor_Name FROM Donor ORDER BY Donor_Name";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Donor_ID"].ToString(), reader["Donor_Name"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Name)>> GetAvailableBeneficiariesAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Benificiary_ID, Name FROM Benificiary ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Benificiary_ID"].ToString(), reader["Name"].ToString()));
                }
            }
            return list;
        }

        public static async Task<List<(string Id, string Name)>> GetAvailableEvacuationCentersAsync()
        {
            var list = new List<(string, string)>();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Center_ID, Center_Name FROM [Evacuation Center] ORDER BY Center_Name";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                            list.Add((reader["Center_ID"].ToString(), reader["Center_Name"].ToString()));
                }
            }
            return list;
        }

        public static async Task<string> AddLocationAsync(string barangay, string city, string province)
        {
            string newId = await GenerateLocationIdAsync();
            using (SqlConnection conn = new SqlConnection(SQL.connectionString))
            {
                string q = @"INSERT INTO Location (Location_ID, Barangay, City, Province)
                             VALUES (@id, @barangay, @city, @province)";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = newId;
                    cmd.Parameters.Add("@barangay", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay.Trim();
                    cmd.Parameters.Add("@city", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city.Trim();
                    cmd.Parameters.Add("@province", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(province) ? (object)DBNull.Value : province.Trim();
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return newId;
        }

        //last minute
        public static async Task<bool> RegisterVolunteerAccountAsync(
    string email,
    string password,
    string volunteerName,
    string contactNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SQL.connectionString))
                using (SqlCommand command = new SqlCommand("usp_AddVolunteer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@email", SqlDbType.VarChar, 255).Value = email.Trim();
                    command.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = PasswordHelper.HashPassword(password);
                    command.Parameters.Add("@volunteerName", SqlDbType.VarChar, 255).Value = volunteerName.Trim();
                    command.Parameters.Add("@contactNumber", SqlDbType.VarChar, 50).Value =
                        string.IsNullOrWhiteSpace(contactNumber) ? (object)DBNull.Value : contactNumber.Trim();

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    MessageBox.Show(
                        "Account created! You may now log in.",
                        "Registration Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return true;
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("already registered") || ex.Number == 50000)
            {
                MessageBox.Show(
                    "An account with this email already exists. Please use a different email.",
                    "Registration Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Could not complete registration because of a database error.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while creating the account.\n\n" + ex.Message,
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

    }
}