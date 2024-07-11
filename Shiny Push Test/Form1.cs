//using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Windows.Forms;

namespace Shiny_Push_Test
{
    public partial class Form1 : Form
    {
        private static readonly object _firebaseLock = new object();

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            List<string> pushTokens = new List<string>
        {
            "fWna9trU7E62spt6UlVWX5:APA91bHuNZmoHy50TWg_4yhK0eDmT884w7p1sVnAfGEqwNns9qJvNYKFQrHRQYa1cOawpwwnNeF8ZMBzVp-WT0XrkEdjyHjSY7ou1t0kFgC-RWf4HS1SCPPxB-Bfe2iTPQI36nQjXUrg",
        };
            string title = "Test notification title";
            string body = "Test notification body";
            string user = "user1@mydomain.com";
            Guid guid = Guid.NewGuid();

            await SendFirebaseNotification(pushTokens, title, body, user, guid);
        }

        private async Task<bool> SendFirebaseNotification(List<string> pushTokens, string title, string body, string user, Guid guid)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var credentialPath = Path.Combine(basePath, "private_key.json");

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    lock (_firebaseLock)
                    {
                        if (FirebaseApp.DefaultInstance == null)
                        {
                            FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromFile(credentialPath)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            Guid g = guid;
            var message = new FirebaseAdmin.Messaging.MulticastMessage()
            {
                Tokens = pushTokens,
                Apns = new ApnsConfig()
                {
                    Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" },   
                    { "apns-push-type", "alert" }       
                },
                    Aps = new Aps()
                    {
                        ContentAvailable = true,
                        Alert = new ApsAlert()
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default"   
                    }
                },
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body,
                },
                Data = new Dictionary<string, string>
            {
                { "click_action", "SHINY_PUSH_NOTIFICATION_CLICK" },
                { "data", "my_silent_data" },
                { "StaffID", "-1" },
                { "EventID", "-1" },
                { "StaffBooking", "-1" },
                { "Comments", "" },
                { "Action", "" },
                { "ActionPayload", "" },
                { "User", user },
                { "GUID", g.ToString() },
                { "Title", title },
                { "Message", body },
            },
                Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        ClickAction = "SHINY_PUSH_NOTIFICATION_CLICK",
                        Title = title,
                        Body = body,
                        Sound = "default",     
                    },
                },
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                if (response.SuccessCount == pushTokens.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FirebaseMessagingException ex)
            {
                return false;
            }
        }

    }
}
