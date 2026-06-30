using ChatRoom.Client.Interfaces;
using Prism.Commands;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;

namespace ChatRoom.Client.ViewModels
{
    public class LoginViewModel : BindableBase
    {
        private readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        private readonly INavigationService _navigation;
        private string _userName;
        private string _errorMessage;

        public DelegateCommand<PasswordBox> LoginCommand { get; }
        public DelegateCommand<PasswordBox> RegisterCommand { get; }

        public LoginViewModel(INavigationService navigation)
        {
            _navigation = navigation;

            // 注册命令
            RegisterCommand = new DelegateCommand<PasswordBox>(async (passwordBox) =>
            {
                if (string.IsNullOrWhiteSpace(UserName) || passwordBox.Password.Length == 0)
                {
                    ErrorMessage = "用户名和密码不能为空";
                    return;
                }

                ErrorMessage = "正在注册...";
                var json = JsonSerializer.Serialize(new { UserName, Password = passwordBox.Password });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/api/auth/register", content);
                var result = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
                var success = result.GetProperty("success").GetBoolean();
                var msg = result.GetProperty("message").GetString();
                ErrorMessage = msg;
            });

            // 登录命令
            LoginCommand = new DelegateCommand<PasswordBox>(async (passwordBox) =>
            {
                if (string.IsNullOrWhiteSpace(UserName) || passwordBox.Password.Length == 0)
                {
                    ErrorMessage = "用户名和密码不能为空";
                    return;
                }

                ErrorMessage = "正在登录...";
                var json = JsonSerializer.Serialize(new { UserName, Password = passwordBox.Password });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/api/auth/login", content);
                var result = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
                var success = result.GetProperty("success").GetBoolean();
                var msg = result.GetProperty("message").GetString();

                if (success)
                {
                    var data = result.GetProperty("data");
                    var userId = data.GetProperty("userId").GetInt32();
                    var userName = data.GetProperty("userName").GetString();
                    _navigation.NavigateToChat(userId, userName);
                }
                else
                {
                    ErrorMessage = msg;
                }
            });
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
    }
}
