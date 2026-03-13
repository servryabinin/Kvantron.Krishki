using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CapDefectDetector.Authorization
{
    public class AuthManager
    {
        private static AuthManager instance;
        public static AuthManager Instance => instance ??= new AuthManager();

        public Role CurrentRole { get; private set; } = Role.Operator;

        private AuthConfig config;
        private string configPath;

        private Timer autoLogoutTimer;

        private AuthManager()
        {
            configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Auth",
                "auth.json"
            );

            LoadConfig();
            InitLogoutTimer();
        }

        private void InitLogoutTimer()
        {
            autoLogoutTimer = new Timer();
            autoLogoutTimer.Interval = 1000; // tick every second
            autoLogoutTimer.Tick += AutoLogoutTimer_Tick;
        }

        private int secondsLeft = -1;

        private void AutoLogoutTimer_Tick(object? sender, EventArgs e)
        {
            if (secondsLeft < 0)
                return;

            secondsLeft--;

            if (secondsLeft == 0)
            {
                SetRole(Role.Operator);
            }
        }

        private void LoadConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("Auth.json не найден, автоматически создан пустой шаблон.");
                    CreateDefaultConfig();
                }

                string json = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<AuthConfig>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки auth.json: " + ex.Message);
                CreateDefaultConfig();
            }
        }

        private void CreateDefaultConfig()
        {
            config = new AuthConfig
            {
                Roles = new Dictionary<string, RoleInfo>
            {
                { "Admin", new RoleInfo { PasswordHash = null } },
                { "Operator", new RoleInfo { PasswordHash = null } }
            },
                AutoLogoutSeconds = new Dictionary<string, int>
            {
                { "Admin", 300 },
                { "Operator", -1 }
            }
            };

            SaveConfig();
        }

        public void SaveConfig()
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }

        public bool ValidateAdminPassword(string password)
        {
            string? stored = config.Roles["Admin"].PasswordHash;

            if (string.IsNullOrEmpty(stored))
                return false;

            string hash = GetSHA256(password);

            return stored.Equals("SHA256:" + hash, StringComparison.OrdinalIgnoreCase);
        }

        public void SetRole(Role newRole)
        {
            CurrentRole = newRole;

            int timeout = config.AutoLogoutSeconds[newRole.ToString()];

            if (timeout > 0)
            {
                secondsLeft = timeout;
                autoLogoutTimer.Start();
            }
            else
            {
                secondsLeft = -1;
                autoLogoutTimer.Stop();
            }

            RoleChanged?.Invoke(newRole);
        }

        public event Action<Role> RoleChanged;

        public static string GetSHA256(string text)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public int GetSecondsLeft()
        {
            return secondsLeft;
        }

    }
}
