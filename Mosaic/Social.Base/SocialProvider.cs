using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facebook;

namespace Social.Base
{
    public class SocialProvider
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string AppId = "243679675658038";
        public static readonly string[] Permissions = new[] { "publish_stream", "read_stream", "read_friendlists", "friends_birthday", "friends_hometown", "friends_interests", "friends_location", "friends_relationships", "friends_hometown" };
        private FacebookOAuthResult oauthResult;
        public event EventHandler SignedIn;
        private const string PostUrl = "http://www.facebook.com/{0}/posts/{1}";

        public void SignIn()
        {
            var form = new Form();
            //form.Width = 640;
            //form.Height = 480;
            form.StartPosition = FormStartPosition.CenterScreen;

            var browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.Navigated += BrowserNavigated;

            IDictionary<string, object> loginParameters = new Dictionary<string, object>
                                                              {
                                                                  { "response_type", "token" },
                                                                  { "display", "popup" }
                                                              };
            browser.Navigate(FacebookOAuthClient.GetLoginUrl(AppId, null, Permissions, false, loginParameters));
            form.Controls.Add(browser);
            form.FormBorderStyle = FormBorderStyle.None;
            form.Opacity = 0;
            form.ShowInTaskbar = false;
            form.WindowState = FormWindowState.Minimized;
            form.ShowDialog();
        }

        void BrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var browser = (WebBrowser)sender;
            if (FacebookOAuthResult.TryParse(e.Url, out oauthResult))
            {
                if (SignedIn != null)
                    SignedIn(this, EventArgs.Empty);
                ((Form)(browser.Parent)).Close();
            }
            else
            {
                var form = ((Form)(browser.Parent));
                form.FormBorderStyle = FormBorderStyle.None;
                form.Opacity = 1;
                form.WindowState = FormWindowState.Maximized;
            }
        }

        public List<Friend> GetFriends()
        {
            var client = new FacebookClient(oauthResult.AccessToken);

            dynamic r = client.Get("/me/friends");
            var data = (IList<object>)r.data;
            if (data == null)
                return null;

            var result = new List<Friend>();
            foreach (IDictionary<string, object> o in data)
            {
                var friend = new Friend();
                friend.Id = (string)o["id"];
                friend.Name = (string)o["name"];
                result.Add(friend);
            }

            result = result.OrderBy(x => x.Name).ToList();

            return result;
        }

        private bool called;
        public Friend GetFriendInfoById(string id)
        {
            if (oauthResult == null)
            {
                SignIn();
            }

            if (oauthResult == null)
            {
                return null;
            }
            var client = new FacebookClient(oauthResult.AccessToken);
            dynamic r = null;
            try
            {
                r = client.Get("/" + id);
            }
            catch (FacebookOAuthException ex)
            {
                logger.Error(ex);
                SignIn(); //access token expired so we need to sign in again
                if (!called)
                {
                    GetFriendInfoById(id); //and call this method again so it will be transparent for user
                    called = true; //avoiding cycling this method is error is not related to acces token
                }
            }
            called = false;
            var result = new Friend();
            result.Id = id;
            result.Name = r.name;
            result.Birthday = r.birthday;
            if (r.hometown != null)
                result.Hometown = r.hometown["name"];
            result.Relationship = r.relationship_status;
            result.Gender = r.gender;
            return result;
        }

        public List<WallEntry> GetFriendStream(string id = "me")
        {
            var client = new FacebookClient(oauthResult.AccessToken);
            string request;
            if (id == "me")
                request = "/me/home";
            else
                request = "/" + id + "/feed";
            dynamic r = client.Get(request);
            var data = (IList<object>)r.data;
            if (data == null)
                return null;
            var result = new List<WallEntry>();

            foreach (IDictionary<string, object> o in data)
            {
                var entry = new WallEntry();
                entry.Id = (string)o["id"];
                var from = (IDictionary<string, object>)o["from"];
                entry.FromName = (string)from["name"];
                entry.FromId = (string)from["id"];
                if (from.ContainsKey("category"))
                    entry.IsPage = true;
                entry.EntryUrl = string.Format(PostUrl, entry.FromId, entry.Id.Split('_')[1]);
                if (o.ContainsKey("message"))
                    entry.Message = (string)o["message"];
                if (o.ContainsKey("link"))
                    entry.Link = (string)o["link"];
                entry.UserPic = string.Format("http://graph.facebook.com/{0}/picture?type=square", entry.FromId);

                if (o.ContainsKey("description"))
                    entry.Description = (string)o["description"];
                if (o.ContainsKey("name"))
                    entry.Name = (string)o["name"];
                if (o.ContainsKey("picture"))
                    entry.Picture = (string)o["picture"];
                entry.CreatedTime = DateTime.Parse((string)o["created_time"]);
                entry.UpdatedTime = DateTime.Parse((string)o["updated_time"]);
                if (o.ContainsKey("comments"))
                {
                    var comments = (IDictionary<string, object>)o["comments"];
                    if (comments.ContainsKey("data"))
                    {
                        var commentsData = (IList<object>)comments["data"];
                        entry.Comments = new List<WallComment>();
                        foreach (IDictionary<string, object> comment in commentsData)
                        {
                            var c = new WallComment();
                            c.Id = (string)comment["id"];
                            var commentFrom = (IDictionary<string, object>)comment["from"];
                            c.FromName = (string)commentFrom["name"];
                            c.FromId = (string)commentFrom["id"];
                            c.Message = (string)comment["message"];
                            c.CreatedTime = DateTime.Parse((string)comment["created_time"]);
                            entry.Comments.Add(c);
                        }
                    }
                    var count = Convert.ToInt32(comments["count"]);
                    entry.CommentsCount = count;
                }
                if (o.ContainsKey("likes"))
                {
                    var likes = (IDictionary<string, object>)o["likes"];
                    var count = Convert.ToInt32(likes["count"]);
                    entry.Likes = count;
                }

                if (o.ContainsKey("application"))
                {
                    var app = (IDictionary<string, object>)o["application"];
                    if (app != null)
                    {
                        entry.Application = (string)app["name"];
                    }
                }

                result.Add(entry);
            }

            return result;
        }
    }
}
