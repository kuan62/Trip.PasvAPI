using System;
using System.IO;
using System.Net;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

namespace Trip.PasvAPI.AppCode
{
    public sealed class Website
    {
        public static readonly Website Instance = new Website();

        public IConfiguration Configuration { get; private set; }

        // Log4net
        public readonly ILog logger = LogManager.GetLogger(typeof(Website));

        // 資料庫連線
        public string SqlConnectionString { get; private set; }

        // B2D API User Token
        public string B2dApiAuthorToken { get; private set; }

        public string AgentAccount { get; private set; }
        public string SignKey { get; private set; }
        public string AgentCurrency { get; private set; }
        public string AesCryptoKey { get; private set; }

        // 主機站台識別
        public string StationID { get { return Dns.GetHostName(); } }
     
  
        private Website()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void Init(IConfiguration config)
        {
            this.Configuration = config;

            this.SqlConnectionString = config["Database:Npgsql"];
            this.B2dApiAuthorToken = config["B2D_API:AuthorToken"];
            this.AgentAccount = config["Proxy:Trip:Account"];
            this.SignKey = config["Proxy:Trip:SignKey"];
            this.AgentCurrency = config["AgentCurrency"];
            this.AesCryptoKey = config["AesCryptoKey"];

            LoadLog4netConfig();
            logger.Debug("StartUp!!!!!");
        }

        private void LoadLog4netConfig()
        {
            try
            {
                string logPath = Configuration["Log4netPath:Path"];

                var repository = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                         typeof(log4net.Repository.Hierarchy.Hierarchy)
                     );

                log4net.GlobalContext.Properties["LogFileName"] = logPath;
                log4net.GlobalContext.Properties["hostname"] = Environment.MachineName;
                log4net.GlobalContext.Properties["app_env"] = Configuration["app_env"];
                XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            }
            catch (Exception ex)
            {
                string qq = ex.Message.ToString();
            }
        }
    }
}