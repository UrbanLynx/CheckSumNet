﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace ChecksumNet.Model
{
    public class UserInfo
    {
        public UserInfo(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Authentication
    {
        public bool isLogedIn = false; // авторизован ли пользователь
        private List<UserInfo> users; // доступные для авторизации данные пользователей
        private UserInfo logedUser; // данные авторизованного пользоватля

        public Authentication()
        {
            DownloadAuthenticationInfo();
        }

        // Авторизация с использованием логина и пароля
        public bool Login(string username, string password)
        {
            logedUser = users.FirstOrDefault(user => (user.Username == username && user.Password == password));
            if (logedUser != null)
            {
                return true;
            }
            return false;
        }

        public string GetUsername()
        {
            return logedUser.Username;
        }

        // Загрузка в память доступных для авторизации данных пользователей
        public void DownloadAuthenticationInfo()
        {
            try
            {
                var filename = ConfigurationManager.AppSettings.Get("AuthenticationFile");
                users = new List<UserInfo>();
                using (var sr = File.OpenText(filename))
                {
                    string str = "";
                    string[] masStr;

                    while ((str = sr.ReadLine()) != null)
                    {
                        masStr = str.Split('\t');
                        users.Add(new UserInfo(masStr[0], masStr[1]));
                    }
                }
            }
            catch (Exception exc)
            {
                LogManager.GetCurrentClassLogger().Error("ОШИБКА. КТО: текущий пользователь. ЧТО: попытка загрузки файла аутентификации. РЕЗУЛЬТАТ: неудача.");   
            }
        }
    }
}