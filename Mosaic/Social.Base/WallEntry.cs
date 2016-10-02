using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Social.Base
{
    public class WallEntry
    {
        public string Id;
        public string FromId;
        public string FromName;
        public string Message;
        public WallEntryType Type;
        public int Likes;
        public int CommentsCount;
        public List<WallComment> Comments;
        public string UserPic;
        public DateTime CreatedTime;
        public DateTime UpdatedTime;
        public string EntryUrl;
        public string Application;
        public bool IsPage; //true - page, false - man
        //for links
        public string Name;
        public string Caption;
        public string Description;
        public string Picture;
        public string Link;
    }
}
