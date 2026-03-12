using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class UserService : IUserService
    {
        private List<User> _users = new();

        public UserService()
        {
            // 添加一些测试数据
            _users.AddRange(new[]
            {
                new User { Id = 2, Name = "Utools", DownLoadUrl = "https://res.u-tools.cn/release/uTools-7.5.1.exe?auth_key=1773303685-9kyAK7cKryRXuOp4hs6ZFD4HNwplmE2D-0-5889e117a5248fdbbb9d16bff7b9c347",SavePath="D:\\2.exe"},
                new User { Id = 6, Name = "qq", DownLoadUrl = "https://dldir1v6.qq.com/qqfile/qq/QQNT/Windows/QQ_9.9.28_260311_x64_01.exe",SavePath="D:\\5.exe"}
            });
        }

        public Task<List<User>> GetUsersAsync()
        {
            return Task.FromResult(_users);
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<bool> AddUserAsync(User user)
        {
            try
            {
                user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
                _users.Add(user);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    existingUser.Name = user.Name;
                    existingUser.DownLoadUrl = user.DownLoadUrl;
                    existingUser.IsActive = user.IsActive;
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    _users.Remove(user);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<User?> GetUserByNameAsync(string name)
        {
            var user = _users.FirstOrDefault(u => u.Name == name);
            return Task.FromResult(user);
        }
    }
}