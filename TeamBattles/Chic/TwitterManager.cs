using System;
using System.IO;
using LinqToTwitter;
using LinqToTwitter.OAuth;

namespace TeamBattles.Chic
{
    public class TwitterManager
    {
        static TwitterContext Context;

        public static void Auth()
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = Environment.GetEnvironmentVariable("CONSUMERKEY"),
                    ConsumerSecret = Environment.GetEnvironmentVariable("CONSUMERSECRET"),
                    AccessToken = Environment.GetEnvironmentVariable("ACCESSTOKEN"),
                    AccessTokenSecret = Environment.GetEnvironmentVariable("ACCESSSECRET")
                }
            };

            Context = new TwitterContext(auth);
        }

        public static void Tweet(string status)
        {
            if (Context == null) Auth();

            Context.TweetAsync(status);
        }

        public static void TweetWithMedia(string filePath, string status)
        {
            if (Context == null) Auth();

            var media = Context.UploadMediaAsync(File.ReadAllBytes(filePath), "image/jpeg", "tweet_image").Result;
            Context.TweetAsync(status, new[] { media.MediaID });
        }
    }
}
